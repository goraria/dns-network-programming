using System.ComponentModel.DataAnnotations;

namespace dns_network_programming.ViewModel
{
  public class DomainVM
  {
    [Display(Name = "Nhap dia chi IP")]
    public string IPAdress { get; set; }
    [Display(Name = "Nhap domain")]
    public string Domain { get; set; }
    [Display(Name = "Danh sach name server")]
    public string NameServer { get; set; } = string.Empty;
  }
}
