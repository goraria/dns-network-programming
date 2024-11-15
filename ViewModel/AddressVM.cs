using System.ComponentModel.DataAnnotations;

namespace dns_network_programming.ViewModel
{
    public class AddressVM
    {
        [Display(Name = "Nhap domain")]
        public string Domain { get; set; }
        [Display(Name = "Ip Address")]
        public string IpAddress { get; set; }
        [Display(Name = "Host Name")]
        public string HostName { get; set; }
    }
}
