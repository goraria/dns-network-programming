namespace AspnetCoreMvcFull.Models;

public class Record
{
    public string Domain { get; set; } = string.Empty;
    // Properties for different DNS records
    public List<string> IpAddress { get; set; } = new List<string>();        // A Record (IP Address)
    public List<string> NameServers { get; set; } = new List<string>();      // NS Record (Name Server)
    public List<string> CanonicalName { get; set; } = new List<string>();    // CNAME Record (Canonical Name)
    public List<string> MailExchange { get; set; } = new List<string>();     // MX Record (Mail Exchange)
    public string HostName { get; set; } = string.Empty;
}
