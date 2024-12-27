using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace nsr
{
    public class Record
    {
        public string Domain { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;        // A Record (IP Address)
        public string NameServer { get; set; } = string.Empty;       // NS Record (Name Server)
        public string CanonicalName { get; set; } = string.Empty;    // CNAME Record (Canonical Name)
        public string MailExchange { get; set; } = string.Empty;     // MX Record (Mail Exchange)
        public string HostName { get; set; } = string.Empty;
    }

    class Program
    {
        static void Main(string[] args)
        {
            string domain = "jetbrains.com"; // Domain to query
            int transactionId = new Random().Next(1, ushort.MaxValue);

            try
            {
                // Create a Record instance
                Record record = new Record { Domain = domain };

                // Create DNS query for NS record
                byte[] query = MakeQuery(transactionId, domain);

                // Send query over UDP
                byte[] response = SendDnsQuery(query);

                // Parse and populate the Record object
                PopulateRecord(response, domain, record);

                // Display the record
                DisplayRecord(record);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        static byte[] MakeQuery(int id, string name)
        {
            byte[] data = new byte[512];
            byte[] query;
            data[0] = (byte)(id >> 8); data[1] = (byte)(id & 0xFF); // ID
            data[2] = (byte)1; data[3] = (byte)0; // Recursion desired
            data[4] = (byte)0; data[5] = (byte)1; // One question
            data[6] = (byte)0; data[7] = (byte)0;
            data[8] = (byte)0; data[9] = (byte)0; data[10] = (byte)0; data[11] = (byte)0;

            // Build the domain section
            string[] tokens = name.Split(new char[] { '.' });
            int position = 12;
            foreach (var label in tokens)
            {
                data[position++] = (byte)(label.Length & 0xFF);
                byte[] b = Encoding.ASCII.GetBytes(label);
                foreach (var ch in b)
                {
                    data[position++] = ch;
                }
            }
            data[position++] = (byte)0; // End of domain
            data[position++] = (byte)0; data[position++] = (byte)2; // Type NS
            data[position++] = (byte)0; data[position++] = (byte)1; // Class IN

            query = new byte[position];
            Array.Copy(data, query, position);
            return query;
        }

        static byte[] SendDnsQuery(byte[] query)
        {
            using (UdpClient client = new UdpClient("8.8.8.8", 53)) // Google Public DNS
            {
                client.Client.ReceiveTimeout = 5000; // 5 seconds timeout
                client.Send(query, query.Length);

                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                return client.Receive(ref remoteEP);
            }
        }

        static void PopulateRecord(byte[] data, string domainName, Record record)
        {
            int questionCount = ((data[4] & 0xFF) << 8) | (data[5] & 0xFF);
            int answerCount = ((data[6] & 0xFF) << 8) | (data[7] & 0xFF);
            int position = 12;

            // Skip question section
            for (int i = 0; i < questionCount; ++i)
            {
                domainName = "";
                position = ProcessDomainName(position, data, ref domainName);
                position += 4; // QTYPE and QCLASS
            }

            // Process answer section
            for (int i = 0; i < answerCount; ++i)
            {
                string parsedDomainName = "";
                position = ProcessDomainName(position, data, ref parsedDomainName); // Parse domain name

                // Extract Type, Class, TTL
                if (position + 10 > data.Length) throw new IndexOutOfRangeException("Invalid DNS response.");
                int recordType = (data[position] << 8) | data[position + 1];
                int ttl = (data[position + 4] << 24) | (data[position + 5] << 16) | (data[position + 6] << 8) | data[position + 7];
                int dataLength = (data[position + 8] << 8) | data[position + 9];
                position += 10;

                if (position + dataLength > data.Length) throw new IndexOutOfRangeException("Invalid DNS data length.");

                if (recordType == 2) // NS record
                {
                    string nsRecord = "";
                    position = ProcessDomainName(position, data, ref nsRecord);
                    record.NameServer += nsRecord + "\n";
                }
                else
                {
                    position += dataLength; // Skip unsupported record types
                }
            }
        }

        static void DisplayRecord(Record record)
        {
            Console.WriteLine($"Domain: {record.Domain}");
            Console.WriteLine($"Name Servers:\n{record.NameServer}");
        }

        static int ProcessDomainName(int position, byte[] data, ref string name)
        {
            int len = (data[position++] & 0xFF);

            if (len == 0) // End of domain name
            {
                return position;
            }

            int offset;
            while (len != 0)
            {
                if ((len & 0xC0) == 0xC0) // Compression
                {
                    offset = ((len & 0x3F) << 8) | (data[position++] & 0xFF);
                    ProcessDomainName(offset, data, ref name);
                    return position;
                }
                else
                {
                    name += Encoding.ASCII.GetString(data, position, len) + ".";
                    position += len;
                }

                len = data[position++] & 0xFF;
            }

            name = name.TrimEnd('.');
            return position;
        }
    }
}