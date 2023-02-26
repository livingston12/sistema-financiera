using SistemaImbrino.App_Start;
using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaImbrino.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private static DB_IMBRINOEntities db = new DB_IMBRINOEntities();

        public ActionResult Index()
        {
            DateTime today = DateTime.Today;
            DateTime fechaDesde = DateTime.Parse($"{today.Month}-01-{today.Year}");
            var dataNcf = db.VW_rptRegistroNCF
                    .Where(x => x.fechadt >= fechaDesde && x.fechadt <= today)
                    .ToList();

            var data = View_generalClass.GetListCuadreCaja();
            decimal? entradas = data.Detalle
                .Where(x => x.Tipo != "SALIDAS")
                .Sum(x => x.Detalle.Sum(y => y.MontoTotal));
            decimal? salidas = data.Detalle
                .Where(x => x.Tipo == "SALIDAS")
                .Sum(x => x.Detalle.Sum(y => y.MontoTotal));
            ViewBag.totalBalanceCaja = entradas - salidas;
            decimal? totalCobrados = dataNcf.Any()
                ? dataNcf.Sum(x => x.ING_MONTOT) : 0;

           ViewBag.totalCobrados = totalCobrados;
           ViewBag.totalCuotasVencidas = db.VW_rptCuotasVencidas
                .Where(x => x.fechadt < DateTime.Now)
                .Sum(x => x.MONTO);
            return View();
        }
       
    }
}