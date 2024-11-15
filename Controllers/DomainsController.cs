using dns_network_programming.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Microsoft.CodeAnalysis;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;

namespace dns_network_programming.Controllers
{
    public class DomainsController : Controller
    {
        [HttpGet]
        public IActionResult Index() => View();
        #region AddressRecord
        [HttpGet]
        public IActionResult AddressRecord() => View();
        [HttpPost]
        public IActionResult AddressRecord(AddressVM model)
        {
            byte[] DNSQuery;
            byte[] DNSReply;
            UdpClient dnsClient = new UdpClient(model.Domain, 53);
            DNSQuery = makeQuery(DateTime.Now.Millisecond * 60, model.Domain);
            dnsClient.Send(DNSQuery, DNSQuery.GetLength(0)); IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0); DNSReply = dnsClient.Receive(ref remoteEP);
            model.IpAddress = ParseDnsResponse(DNSReply,model.Domain);
            IPHostEntry hostEntry = Dns.GetHostEntry(model.Domain);
            model.HostName = hostEntry.HostName;
            return View(model);
        }
        #endregion
        [HttpGet]
        public IActionResult NameServerRecord() => View();
        [HttpGet]
        public IActionResult CanonicalNameRecord() => View();
        [HttpGet]
        #region MailExchangeRecord
        public IActionResult MailExchangeRecord() => View();
        [HttpPost]
        public IActionResult MailExchangeRecord(DomainVM model)
        {
            byte[] DNSQuery;
            byte[] DNSReply;
            UdpClient dnsClient = new UdpClient(model.IPAdress, 53);
            DNSQuery = makeQuery(DateTime.Now.Millisecond * 60, model.Domain);
            dnsClient.Send(DNSQuery, DNSQuery.GetLength(0)); IPEndPoint endpoint = null; DNSReply = dnsClient.Receive(ref endpoint);
            model.NameServer = makeResponse(DNSReply, model.Domain);
            return View(model);
        }
        #endregion
        public byte[] makeQuery(int id, string name)
        {
            byte[] data = new byte[512];
            byte[] Query;
            data[0] = (byte)(id >> 8); data[1] = (byte)(id & 0xFF);
            data[2] = (byte)1; data[3] = (byte)0;
            data[4] = (byte)0; data[5] = (byte)1;

            data[6] = (byte)0; data[7] = (byte)0;
            data[8] = (byte)0; data[9] = (byte)0; data[10] = (byte)0; data[11] = (byte)0;
            string[] tokens = name.Split(new char[] { '.' }); string label;
            int position = 12;
            for (int j = 0; j < tokens.Length; j++)
            {
                label = tokens[j];
                data[position++] = (byte)(label.Length & 0xFF);
                byte[] b = System.Text.Encoding.ASCII.GetBytes(label); for (int k = 0; k < b.Length; k++)
                {
                    data[position++] = b[k];
                }
            }
            data[position++] = (byte)0; data[position++] = (byte)0; data[position++] = (byte)15; data[position++] = (byte)0; data[position++] = (byte)1;
            Query = new byte[position + 1]; for (int i = 0; i <= position; i++)
            {
                Query[i] = data[i];
            }
            return Query;
        }
        public string makeResponse(byte[] data, string name)
        {
            int qCount = ((data[4] & 0xFF) << 8) | (data[5] & 0xFF); int aCount = ((data[6] & 0xFF) << 8) | (data[7] & 0xFF);
            int position = 12;
            for (int i = 0; i < qCount; ++i)
            {
                name = "";
                position = proc(position, data, ref name); position += 4;
            }
            string Response = "";
            for (int i = 0; i < aCount; ++i)
            {
                name = "";
                position = proc(position, data, ref name); position += 12;
                name = "";
                position = proc(position, data, ref name); Response += name + "\r\n";
            }
            return Response;
        }
        private int proc(int position, byte[] data, ref string name)
        {
            int len = (data[position++] & 0xFF);
            if (len == 0)
            {
                return position;
            }
            int offset;

            do
            {
                if ((len & 0xC0) == 0xC0)
                {
                    if (position >= data.GetLength(0))
                    {
                        return -1;
                    }
                    offset = ((len & 0x3F) << 8) | (data[position++] & 0xFF);
                    proc(offset, data, ref name); return position;
                }
                else
                {
                    if ((position + len) > data.GetLength(0))
                    {
                        return -1;
                    }
                    name += Encoding.ASCII.GetString(data, position, len); position += len;
                }
                if (position > data.GetLength(0))
                {
                    return -1;
                }
                len = data[position++] & 0xFF; if (len != 0)
                {
                    name += ".";
                }
            }
            while (len != 0); return position;
        }
        public string ParseDnsResponse(byte[] data,string name)
        {
            {
                int qCount = ((data[4] & 0xFF) << 8) | (data[5] & 0xFF);
                int aCount = ((data[6] & 0xFF) << 8) | (data[7] & 0xFF);
                int position = 12;
                for (int i = 0; i < qCount; ++i)
                {
                    name = "";
                    position = proc(position, data, ref name);
                    position += 4;
                }
                StringBuilder responseBuilder = new StringBuilder();
                for (int i = 0; i < aCount; ++i)
                {
                    name = ""; position = proc(position, data, ref name);
                    position += 12;
                    string ipAddress = $"{data[position++]}.{data[position++]}.{data[position++]}.{data[position++]}";
                    responseBuilder.AppendLine($"{ipAddress}");
                }
                return responseBuilder.ToString();
            }
        }
    }
}
