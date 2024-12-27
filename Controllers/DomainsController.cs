using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Microsoft.CodeAnalysis;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using AspnetCoreMvcFull.Models;
using dns_network_programming.ViewModel;

namespace dns_network_programming.Controllers
{
    public class DomainsController : Controller
    {
        #region Index

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(Record model)
        {
            return View();
        }

        #endregion

        #region AddressRecord

        [HttpGet]
        public IActionResult AddressRecord()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddressRecord(Record model)
        {
            return View();
        }

        #endregion

        #region NameServerRecord

        [HttpGet]
        public IActionResult NameServerRecord()
        {
            return View();
        }

        [HttpPost]
        public IActionResult NameServerRecord(Record model)
        {
            return View();
        }

        #endregion

        #region CanonicalNameRecord

        [HttpGet]
        public IActionResult CanonicalNameRecord()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CanonicalNameRecord(Record model)
        {
            return View();
        }

        #endregion

        #region MailExchangeRecord

        [HttpGet]
        public IActionResult MailExchangeRecord()
        {
            return View();
        }

        [HttpPost]
        public IActionResult MailExchangeRecord(Record model)
        {
            return View();
        }

        #endregion
    }
}
