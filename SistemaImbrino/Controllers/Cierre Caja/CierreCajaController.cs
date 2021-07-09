using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SistemaImbrino.Controllers.Cierre_Caja
{
    public class CierreCajaController : BaseController
    {
       private DB_IMBRINOEntities _db = new DB_IMBRINOEntities();

        // GET: CierreCaja
        public ActionResult Index()
        {
            DateTime Fecha = DateTime.Now.AddDays(-10);
            ViewBag.ListCierreCaja = GetListCierreCaja(Fecha);
            return View();
        }
        public ActionResult _cierreCajaResumen()
        {
            return PartialView();
        }

    }
}