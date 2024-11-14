using Microsoft.AspNetCore.Mvc;

namespace dns_network_programming.Controllers
{
  public class DomainController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }
  }
}
