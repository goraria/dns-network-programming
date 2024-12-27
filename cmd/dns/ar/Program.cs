using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ar // sai
{
    public class Record
    {
        public string Domain { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty; // Lưu danh sách địa chỉ IP
    }

    class Program
    {
        static void Main(string[] args)
        {
            string domain = "jetbrains.com"; // Domain cần truy vấn
            int transactionId = new Random().Next(1, ushort.MaxValue); // Tạo ID ngẫu nhiên cho truy vấn

            try
            {
                // Tạo một đối tượng Record
                Record record = new Record { Domain = domain };

                // Tạo DNS query cho A Record
                byte[] query = MakeQuery(transactionId, domain);

                // Gửi query qua Google Public DNS
                byte[] response = SendDnsQuery(query);

                // Phân tích và điền thông tin vào đối tượng Record
                PopulateRecord(response, record);

                // Hiển thị kết quả
                DisplayRecord(record);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        static byte[] MakeQuery(int id, string name)
        {
            byte[] data = new byte[512]; // Gói DNS tối đa 512 byte
            int position = 0;

            // Header
            data[position++] = (byte)(id >> 8); // Transaction ID (high byte)
            data[position++] = (byte)(id & 0xFF); // Transaction ID (low byte)
            data[position++] = 0x01; // Flags: recursion desired
            data[position++] = 0x00;
            data[position++] = 0x00; data[position++] = 0x01; // Questions: 1
            data[position++] = 0x00; data[position++] = 0x00; // Answer RRs
            data[position++] = 0x00; data[position++] = 0x00; // Authority RRs
            data[position++] = 0x00; data[position++] = 0x00; // Additional RRs

            // Question section
            foreach (string label in name.Split('.'))
            {
                byte[] labelBytes = Encoding.ASCII.GetBytes(label);
                data[position++] = (byte)labelBytes.Length; // Label length
                Array.Copy(labelBytes, 0, data, position, labelBytes.Length);
                position += labelBytes.Length;
            }
            data[position++] = 0x00; // End of domain name
            data[position++] = 0x00; data[position++] = 0x01; // Type A (Address)
            data[position++] = 0x00; data[position++] = 0x01; // Class IN (Internet)

            // Cắt gói dữ liệu chính xác kích thước
            byte[] query = new byte[position];
            Array.Copy(data, query, position);
            return query;
        }

        static byte[] SendDnsQuery(byte[] query)
        {
            using (UdpClient client = new UdpClient("8.8.8.8", 53)) // Google Public DNS
            {
                client.Client.ReceiveTimeout = 5000; // Timeout 5 giây
                client.Send(query, query.Length); // Gửi gói truy vấn

                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0); // Chấp nhận tất cả các địa chỉ IP
                return client.Receive(ref remoteEP); // Nhận phản hồi
            }
        }

        static void PopulateRecord(byte[] data, Record record)
        {
            int questionCount = ((data[4] & 0xFF) << 8) | (data[5] & 0xFF);
            int answerCount = ((data[6] & 0xFF) << 8) | (data[7] & 0xFF);
            int position = 12;

            // Bỏ qua phần câu hỏi
            for (int i = 0; i < questionCount; i++)
            {
                string domainName = "";
                position = ProcessDomainName(position, data, ref domainName);
                position += 4; // Bỏ qua QTYPE và QCLASS
            }

            // Xử lý phần trả lời
            for (int i = 0; i < answerCount; i++)
            {
                string parsedDomainName = "";
                position = ProcessDomainName(position, data, ref parsedDomainName); // Đọc domain

                // Extract Type, Class, TTL, Data Length
                if (position + 10 > data.Length) throw new IndexOutOfRangeException("Invalid DNS response.");
                int recordType = (data[position] << 8) | data[position + 1];
                int dataLength = (data[position + 8] << 8) | data[position + 9];
                position += 10;

                if (position + dataLength > data.Length) throw new IndexOutOfRangeException("Invalid DNS data length.");

                if (recordType == 1) // A Record (Address Record)
                {
                    // Đọc địa chỉ IP
                    string ipAddress = $"{data[position]}.{data[position + 1]}.{data[position + 2]}.{data[position + 3]}";
                    record.IpAddress += ipAddress + "\n";
                    position += dataLength;
                }
                else
                {
                    position += dataLength; // Bỏ qua các loại bản ghi khác
                }
            }
        }

        static void DisplayRecord(Record record)
        {
            Console.WriteLine($"Domain: {record.Domain}");
            Console.WriteLine("IP Address(es):");

            // Lấy danh sách địa chỉ IP
            string[] ipAddresses = record.IpAddress.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            // Hiển thị từng địa chỉ
            foreach (var ip in ipAddresses)
            {
                Console.WriteLine(ip);
            }
        }

        static int ProcessDomainName(int position, byte[] data, ref string name)
        {
            int len = data[position++];
            while (len != 0)
            {
                if ((len & 0xC0) == 0xC0) // Compression
                {
                    int offset = ((len & 0x3F) << 8) | data[position++];
                    ProcessDomainName(offset, data, ref name);
                    return position;
                }
                else
                {
                    name += Encoding.ASCII.GetString(data, position, len) + ".";
                    position += len;
                }
                len = data[position++];
            }

            name = name.TrimEnd('.'); // Xóa dấu chấm ở cuối
            return position;
        }
    }
}