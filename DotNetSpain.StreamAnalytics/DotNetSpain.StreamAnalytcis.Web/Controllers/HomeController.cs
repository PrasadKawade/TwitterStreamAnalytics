using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Microsoft.ServiceBus;

namespace DotNetSpain.StreamAnalytcis.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(WebConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"]);
            var list = namespaceManager.GetEventHubs();
            List<SelectListItem> items = new List<SelectListItem>();
            foreach (var item in list)
            {
                items.Add(new SelectListItem() { Text = item.Path, Value = item.Path });
            }

            ViewBag.EventHubs = items;
            return View();
        }
    }
}
