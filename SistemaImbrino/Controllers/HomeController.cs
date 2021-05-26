using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaImbrino.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            string automaStr = System.Configuration.ConfigurationManager.AppSettings["Automatic"].ToString();
            bool auto = false;

            bool.TryParse(automaStr, out auto);

            if (auto)
            {
                Automatitation at = new Automatitation();
                at.automatizarArchivos();
            }
         
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}