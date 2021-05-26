using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SistemaImbrino.Controllers
{
    public class CobrosHeaderController : BaseController
    {
       

        // GET: CobrosHeader
        public  ActionResult Index()
        {
            var listCobrosHeader = CobrosHeader();
            ViewBag.listCobrosHeader = listCobrosHeader.OrderBy(x=>x.cliente).ToList();          
            return  View();
        }

        // GET: CobrosHeader
        
        public ActionResult Index2(string findID)
        {
            var listCobrosHeader = CobrosHeader();
            ViewBag.listCobrosHeader = listCobrosHeader.OrderBy(x => x.cliente).ToList();
            ViewBag.valor = findID;
            return View("Index");
        }

        public ActionResult _CobrosDetalleCliente()//(string cliente)
        {
            //var detalleCuotasCliente = CobrosHeader(cliente);
           
            //return PartialView(detalleCuotasCliente);
            return PartialView();
        }

  
        
    }
}