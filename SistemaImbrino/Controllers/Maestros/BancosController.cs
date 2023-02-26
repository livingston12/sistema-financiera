using SistemaImbrino.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SistemaImbrino.Controllers.Maestros
{
    [Authorize(Roles = "bancos,admin")]
    public class BancosController : BaseController
    {
        private DB_IMBRINOEntities _db = new DB_IMBRINOEntities();

        public ActionResult Index()
        {
            ViewBag.ListBancos = _db.BANCO.ToList();
            return View();
            
        }

        public JsonResult CrearBanco(BANCO BANCO)
        {
            message message;
            try
            {
                if (!validarBanco(BANCO))
                {
                    message = new message()
                    {
                        Message = "Todos los campos son obligatorios favor rellenar todos los campos",
                        Is_Success = false
                    };
                    return Json(message);
                }
                BANCO.BCO_CODIGO = lastCodBanco();
                _db.BANCO.Add(BANCO);
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Banco insertado correctamente",
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

        public JsonResult ActualizarBanco(BANCO BANCO)
        {
            message message;
            
            try
            {
                var searchBanco = _db.BANCO.Where(x => x.BCO_CODIGO == BANCO.BCO_CODIGO).FirstOrDefault();
                if (searchBanco == null)
                {
                    message = new message()
                    {
                        Message = "Banco no existe",
                        Is_Success = false
                    };
                    return Json(message);
                }
                searchBanco.BCO_NOMBRE = BANCO.BCO_NOMBRE;
                searchBanco.BCO_ABREVI = BANCO.BCO_ABREVI;
                searchBanco.BCO_SUCURS = BANCO.BCO_SUCURS;
                searchBanco.BCO_TELEF1 = BANCO.BCO_TELEF1;
                searchBanco.BCO_TELEF2 = BANCO.BCO_TELEF2;
                searchBanco.BCO_TELEF3 = BANCO.BCO_TELEF3;

                _db.Entry(searchBanco).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Banco actualizado correctamente",
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

        public JsonResult EliminarBanco(int id)
        {
            message message;
            try
            {
                var BANCO = _db.BANCO.Where(x => x.BCO_CODIGO == id).ToList();

                if (!BANCO.Any())
                {
                    message = new message()
                    {
                        Message = "Banco no existe",
                        Is_Success = false
                    };
                    return Json(message);
                }
                _db.BANCO.Remove(BANCO.FirstOrDefault());
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Banco eliminado correctamente",
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

        public ActionResult _BancosDetalle()
        {
            return PartialView();
        }

        public ActionResult _popup_editar()
        {
            return PartialView();
        }

        public JsonResult GetCurrentBanco(int id = 0)
        {
            _db = new DB_IMBRINOEntities();
            var currentBanco = _db.BANCO.Where(x => x.BCO_CODIGO == id)
                                           .Select(x =>
                                           new
                                           {
                                               BCO_NOMBRE = x.BCO_NOMBRE.Trim(),
                                               BCO_ABREVI = x.BCO_ABREVI.Trim(),
                                               BCO_SUCURS = x.BCO_SUCURS.Trim(),
                                               BCO_TELEF1 = x.BCO_TELEF1.Trim(),
                                               BCO_TELEF2 = x.BCO_TELEF2.Trim(),
                                               BCO_TELEF3 = x.BCO_TELEF3.Trim()
                                           })
                                                   .FirstOrDefault();
            return Json(currentBanco);
        }
    }
}