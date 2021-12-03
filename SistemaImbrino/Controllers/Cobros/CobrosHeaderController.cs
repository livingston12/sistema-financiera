using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SistemaImbrino.Controllers
{
    [Authorize(Roles = "cobros,admin")]
    public class CobrosHeaderController : BaseController
    {      

        // GET: CobrosHeader
        public async Task<ActionResult> Index()
        {
            EntityFramewrowkExtension.page = 1;
            EntityFramewrowkExtension.limit = int.MaxValue;

            View_cobrosHeader listCobrosHeader = await CobrosHeader();
            return View(listCobrosHeader);
        }

        // GET: CobrosHeader
        public async Task<ActionResult> prueba(int page, int limit)
        {
            EntityFramewrowkExtension.page = page;
            EntityFramewrowkExtension.limit = int.MaxValue;

            View_cobrosHeader listCobrosHeader = await CobrosHeader();
            return View("Index", listCobrosHeader);
        }

        // GET: CobrosHeader

        public async Task<ActionResult> Index2(string findID)
        {
            View_cobrosHeader listCobrosHeader = await CobrosHeader();
            ViewBag.valor = findID;
            return View("Index", listCobrosHeader);
        }

        public ActionResult DetalleCobro(string cliente)
        {
           View_cobrosHeader listCobrosHeader = CobrosHeader(cliente).Result;
            return PartialView(listCobrosHeader);
        }



    }
}