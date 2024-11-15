using Microsoft.AspNetCore.Mvc;

namespace dns_network_programming.Controllers
{
    public class DomainsController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult AddressRecord() => View();
        public IActionResult NameServerRecord() => View();
        public IActionResult CanonicalNameRecord() => View();
        public IActionResult MailExchangeRecord() => View();
    }
}
