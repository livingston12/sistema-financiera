using SistemaImbrino.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SistemaImbrino.Controllers.Maestros
{
    [Authorize(Roles = "fiadores,admin")]
    public class FiadoresController : BaseController
    {
        private DB_IMBRINOEntities _db = new DB_IMBRINOEntities();
        // GET: Fiadores
        public ActionResult Index()
        {
            ViewBag.ListFiadores = _db.FIADOR.ToList();
            return View();
        }

        public JsonResult CrearFiador(FIADOR FIADOR)
        {
            message message;
            try
            {

                if (!validarFiador(FIADOR))
                {
                    message = new message()
                    {
                        Message = "Todos los campos son obligatorios favor rellenar todos los campos",
                        Is_Success = false
                    };
                    return Json(message);
                }
                FIADOR.FIA_CODIGO = lastCodFiador();
                _db.FIADOR.Add(FIADOR);
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Fiador insertado correctamente",
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

        public JsonResult ActualizarFiador(FIADOR FIADOR)
        {
            message message;
            try
            {
                var searchFiador = _db.FIADOR.Where(x => x.FIA_CODIGO == FIADOR.FIA_CODIGO).FirstOrDefault();
                if (searchFiador == null)
                {
                    message = new message()
                    {
                        Message = "Fiador no existe",
                        Is_Success = false
                    };
                    return Json(message);
                }
                searchFiador.FIA_CEDULA = FIADOR.FIA_CEDULA;
                searchFiador.FIA_DIRECC = FIADOR.FIA_DIRECC;
                searchFiador.FIA_NOMBRE = FIADOR.FIA_NOMBRE;
                searchFiador.FIA_TELEFO = FIADOR.FIA_TELEFO;

                _db.Entry(searchFiador).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Fiador actualizado correctamente",
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

        public JsonResult EliminarFiador(int id)
        {
            message message;
            try
            {
                var FIADOR = _db.FIADOR.Where(x => x.FIA_CODIGO == id).ToList();

                if (!FIADOR.Any())
                {
                    message = new message()
                    {
                        Message = "Fiador no existe",
                        Is_Success = false
                    };
                    return Json(message);
                }
                _db.FIADOR.Remove(FIADOR.FirstOrDefault());
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Fiador eliminado correctamente",
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

        public ActionResult _fiadoresDetalle()
        {
            return PartialView();
        }

        public ActionResult _popup_editar()
        {
            return PartialView();
        }

        public JsonResult GetCurrentFiador(int id = 0)
        {
            _db = new DB_IMBRINOEntities();
            var currentFiador = _db.FIADOR.Where(x => x.FIA_CODIGO == id)
                                           .Select(x =>
                                           new
                                           {
                                               FIA_NOMBRE = x.FIA_NOMBRE.Trim(),
                                               FIA_DIRECC = x.FIA_DIRECC.Trim(),
                                               x.FIA_CEDULA,
                                               x.FIA_TELEFO
                                           })
                                                   .FirstOrDefault();
            return Json(currentFiador);
        }
    }
}