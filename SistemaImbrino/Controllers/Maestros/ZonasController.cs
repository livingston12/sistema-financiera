using SistemaImbrino.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace SistemaImbrino.Controllers.Maestros
{
    [Authorize(Roles = "zonas,admin")]
    public class ZonasController : BaseController
    {
        private DB_IMBRINOEntities _db = new DB_IMBRINOEntities();

        public ActionResult Index()
        {
            ViewBag.ListZonas = _db.ZONA.ToList();
            return View();
        }

        public JsonResult CrearZona(ZONA ZONA)
        {
            message message;
            try
            {

                if (!validarZona(ZONA))
                {
                    message = new message()
                    {
                        Message = "Todos los campos son obligatorios favor rellenar todos los campos",
                        Is_Success = false
                    };
                    return Json(message);
                }
                ZONA.ZON_CODIGO = lastCodZona();
                _db.ZONA.Add(ZONA);
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Zona insertado correctamente",
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

        public JsonResult ActualizarZona(ZONA ZONA)
        {
            message message;
            try
            {
                var searchZona = _db.ZONA.Where(x => x.ZON_CODIGO == ZONA.ZON_CODIGO).FirstOrDefault();
                if (searchZona == null)
                {
                    message = new message()
                    {
                        Message = "Zona no existe",
                        Is_Success = false
                    };
                    return Json(message);
                }
                searchZona.ZON_DESCRI = ZONA.ZON_DESCRI;
               

                _db.Entry(searchZona).State = EntityState.Modified;
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Zona actualizada correctamente",
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

        public JsonResult EliminarZona(string id)
        {
            message message;
            try
            {
                var ZONA = _db.ZONA.Where(x => x.ZON_CODIGO == id).ToList();

                if (!ZONA.Any())
                {
                    message = new message()
                    {
                        Message = "Zona no existe",
                        Is_Success = false
                    };
                    return Json(message);
                }
                _db.ZONA.Remove(ZONA.FirstOrDefault());
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Zona eliminado correctamente",
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

        public ActionResult _zonasDetalle()
        {
            return PartialView();
        }

        public ActionResult _popup_editar()
        {
            return PartialView();
        }

        public JsonResult GetCurrentZona(string id = "0")
        {
            _db = new DB_IMBRINOEntities();
            var currentZona = _db.ZONA.Where(x => x.ZON_CODIGO == id)
                                           .Select(x =>
                                           new
                                           {
                                               ZON_DESCRI = x.ZON_DESCRI.Trim(),
                                           })
                                                   .FirstOrDefault();
            return Json(currentZona);
        }
    }
}