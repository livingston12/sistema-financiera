using SistemaImbrino.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace SistemaImbrino.Controllers.Maestros
{
    [Authorize(Roles = "promotores,admin")]
    public class VendedoresController : BaseController
    {
        private DB_IMBRINOEntities _db = new DB_IMBRINOEntities();

        public ActionResult Index()
        {
            ViewBag.ListVendedores = _db.VENDEDOR.ToList();
            return View();
        }

        public JsonResult CrearVendedor(VENDEDOR VENDEDOR)
        {
            message message;
            try
            {

                if (!validarVendedor(VENDEDOR))
                {
                    message = new message()
                    {
                        Message = "Todos los campos son obligatorios favor rellenar todos los campos",
                        Is_Success = false
                    };
                    return Json(message);
                }
                VENDEDOR.VEN_CODIGO = lastCodVendedor();
                _db.VENDEDOR.Add(VENDEDOR);
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Vendedor insertado correctamente",
                    Is_Success = true
                };
            }

            catch (Exception)
            {

                message = new message()
                {
                    Message = MensajeErrorCath,
                    Is_Success = false
                };
                return Json(message);
            }





            return Json(message);
        }

        public JsonResult ActualizarVendedor(VENDEDOR VENDEDOR)
        {
            message message;
            try
            {
                var searchVendedor = _db.VENDEDOR.Where(x => x.VEN_CODIGO == VENDEDOR.VEN_CODIGO).FirstOrDefault();
                if (searchVendedor == null)
                {
                    message = new message()
                    {
                        Message = "Vendedor no existe",
                        Is_Success = false
                    };
                    return Json(message);
                }
                searchVendedor.VEN_NOMBRE = VENDEDOR.VEN_NOMBRE;
                

                _db.Entry(searchVendedor).State = EntityState.Modified;
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Vendedor actualizado correctamente",
                    Is_Success = true
                };
            }
            catch (Exception)
            {

                message = new message()
                {
                    Message = MensajeErrorCath,
                    Is_Success = false
                };
                return Json(message);
            }
            return Json(message);
        }

        public JsonResult EliminarVendedor(int id)
        {
            message message;
            try
            {
                var VENDEDOR = _db.VENDEDOR.Where(x => x.VEN_CODIGO == id).ToList();

                if (!VENDEDOR.Any())
                {
                    message = new message()
                    {
                        Message = "Vendedor no existe",
                        Is_Success = false
                    };
                    return Json(message);
                }
                _db.VENDEDOR.Remove(VENDEDOR.FirstOrDefault());
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Vendedor eliminado correctamente",
                    Is_Success = true
                };
            }
            catch (Exception)
            {

                message = new message()
                {
                    Message = MensajeErrorCath,
                    Is_Success = false
                };
                return Json(message);
            }
            return Json(message);
        }

        public ActionResult _vendedoresDetalle()
        {
            return PartialView();
        }

        public ActionResult _popup_editar()
        {
            return PartialView();
        }

        public JsonResult GetCurrentVendedor(int id = 0)
        {
            _db = new DB_IMBRINOEntities();
            var currentVendedor = _db.VENDEDOR.Where(x => x.VEN_CODIGO == id)
                                           .Select(x =>
                                           new
                                           {
                                               VEN_NOMBRE = x.VEN_NOMBRE.Trim()                                               
                                           })
                                                   .FirstOrDefault();
            return Json(currentVendedor);
        }
    }
}