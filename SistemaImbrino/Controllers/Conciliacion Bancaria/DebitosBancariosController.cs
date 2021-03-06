using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static SistemaImbrino.Models.Parameters;

namespace SistemaImbrino.Controllers.Conciliacion_Bancaria
{
    [Authorize(Roles = "debitos,admin")]
    public class DebitosBancariosController : BaseController
    {
        private DB_IMBRINOEntities _db = new DB_IMBRINOEntities();
        // GET: DebitosBancarios
        public ActionResult Index()
        {
            ViewBag.listFilters = _db.TIPODB1;
            return View();
        }

        public JsonResult getCurrentDebitoBancario(string btnId)
        {
            DateTime dateToSearch = DateTime.Now.AddDays(-45);
            string value = btnId.Replace("btn-", "").ToUpper();
            bool findFilter = string.Compare(value, "todos", StringComparison.CurrentCultureIgnoreCase) == 0 ?
                                false : true;
            
            IEnumerable<View_ListDebitosBancarios> ListDebitosBancarios =
                _db.OTROSDB
                    .Where(x => x.ACTIVO == true)
                    .Select(x => new View_ListDebitosBancarios()
                    {
                        ID = x.ID,
                        ID_BANCO = x.BANCO,
                        ID_CUENTA_BANCARIA = x.CUENTA_BANCARIA,
                        CONCEPTO = x.CONCEPTO,
                        ACTIVO = x.ACTIVO,
                        ID_TIPO_ENTRADA = x.TIPO_ENTRADA,
                        MONTO = x.MONTO,
                        FECHAdt = x.FECHA,
                        ID_TIPO_DEBITO = x.TIPO_DEBITO
                    })
                    .Where(x => x.FECHAdt >= dateToSearch)
                    .ToList()
                    .OrderByDescending(x => x.FECHAdt);
            if (findFilter)
            {
                int idTipoDebito = 0;
                switch (value)
                {
                    case "DESPOSITO":
                        idTipoDebito = 1;
                        break;
                    case "TRANFERENCIA":
                        idTipoDebito = 2;
                        break;
                    case "OTRO":
                        idTipoDebito = 3;
                        break;
                }
                ListDebitosBancarios = ListDebitosBancarios.Where(x => x.ID_TIPO_ENTRADA == idTipoDebito).ToList();
            }
            return Json(ListDebitosBancarios);
        }

        public JsonResult CrearDebitoBancario(OTROSDB DebitoBancario)
        {
            message message;
            try
            {
                message = validarDebitoBancario(DebitoBancario);
                if (!message.Is_Success)
                {
                    return Json(message);
                }
                DebitoBancario.ACTIVO = true;
                DebitoBancario.CERRADO = false;
                DebitoBancario.VALIDADO = false;
                DebitoBancario.STATUS = 5;

                modificarBalanceFecha(_db, DebitoBancario.CUENTA_BANCARIA, DebitoBancario.MONTO, tipoBalanceFecha.aumentar);
                _db.OTROSDB.Add(DebitoBancario);
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Debito bancario insertado correctamente",
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

        private message validarDebitoBancario(OTROSDB debitoBancario)
        {
            List<string> ListMessage = new List<string>();
            message message = new message();
            if (string.IsNullOrEmpty(debitoBancario.CONCEPTO))
            {
                ListMessage.Add($"<li>El campo {nameof(debitoBancario.CONCEPTO)} es obligatorio</li>");
            }
            else if (debitoBancario.CONCEPTO.Length > 150)
            {
                ListMessage.Add($"<li>El campo {nameof(debitoBancario.CONCEPTO)} no puede ser mayor que 150</li>");
            }
            if (debitoBancario.MONTO <= 0)
            {
                ListMessage.Add($"<li>El campo {nameof(debitoBancario.MONTO)} es obligatorio</li>");
            }
            if (debitoBancario.FECHA.HasValue == false)
            {
                ListMessage.Add($"<li>El campo {nameof(debitoBancario.FECHA)} es obligatorio</li>");
            }
            if (debitoBancario.BANCO <= 0)
            {
                ListMessage.Add($"<li>El campo {nameof(debitoBancario.BANCO)} es obligatorio</li>");
            }
            if (debitoBancario.TIPO_ENTRADA <= 0)
            {
                ListMessage.Add($"<li>El campo {nameof(debitoBancario.TIPO_ENTRADA).Replace("_", " ")} es obligatorio</li>");
            }
            if (debitoBancario.TIPO_DEBITO <= 0)
            {
                ListMessage.Add($"<li>El campo {nameof(debitoBancario.TIPO_DEBITO).Replace("_", " ")} es obligatorio</li>");
            }
            if (debitoBancario.CUENTA_BANCARIA <= 0)
            {
                ListMessage.Add($"<li>El campo {nameof(debitoBancario.CUENTA_BANCARIA).Replace("_", " ")} es obligatorio</li>");
            }

            message = new message
            {
                Is_Success = ListMessage.Count() == 0,
                Message = $"<ul>{string.Join("", ListMessage)} </ul>"
            };
            return message;
        }

        public JsonResult ActualizarDebitoBancario(OTROSDB debitoBancario)
        {
            message message;
            try
            {
                _db = new DB_IMBRINOEntities();
                var debito = _db.OTROSDB.Find(debitoBancario.ID);
                if (debito == null)
                {
                    message = new message()
                    {
                        Message = "Debito bancario no existe",
                        Is_Success = false
                    };
                    return Json(message);
                }
                
                

                debito.BANCO = debitoBancario.BANCO;
                debito.CONCEPTO = debitoBancario.CONCEPTO;
                debito.CUENTA_BANCARIA = debitoBancario.CUENTA_BANCARIA;
                debito.FECHA = debitoBancario.FECHA.HasValue ?
                                debitoBancario.FECHA : debito.FECHA;
                debito.MONTO = debitoBancario.MONTO;
                debito.TIPO_ENTRADA = debitoBancario.TIPO_ENTRADA;
                debito.TIPO_DEBITO = debitoBancario.TIPO_DEBITO;

                _db.Entry(debito).State = System.Data.Entity.EntityState.Modified;
                modificarBalanceFecha(_db, debitoBancario.CUENTA_BANCARIA, debito.MONTO, debitoBancario.MONTO);
                _db.SaveChanges();
                message = new message()
                {
                    Message = "Debito bancario actualizado correctamente",
                    Is_Success = true
                };
            }
            catch (Exception e)
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

        public JsonResult EliminarDebitoBancario(int id)
        {
            message message;
            try
            {
                var debitoBancario = _db.OTROSDB.Find(id);

                if (debitoBancario == null)
                {
                    message = new message()
                    {
                        Message = "Debito bancario no existe",
                        Is_Success = false
                    };
                    return Json(message);
                }
                debitoBancario.ACTIVO = false;
                _db.Entry(debitoBancario).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Debito bancario eliminado correctamente",
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

        public ActionResult _popup_editar()
        {
            return PartialView();
        }

        public ActionResult _popup_create()
        {
            return PartialView();
        }

        public JsonResult GetDebitoBancarioDetail(int id)
        {
            View_ListDebitosBancarios ListDebitosBancarios =
                _db.OTROSDB
                    .Where(x => x.ACTIVO == true)
                    .Select(x => new View_ListDebitosBancarios()
                    {
                        ID = x.ID,
                        ID_BANCO = x.BANCO,
                        ID_CUENTA_BANCARIA = x.CUENTA_BANCARIA,
                        CONCEPTO = x.CONCEPTO,
                        ACTIVO = x.ACTIVO,
                        ID_TIPO_ENTRADA = x.TIPO_ENTRADA,
                        MONTO = x.MONTO,
                        FECHAdt = x.FECHA,
                        ID_TIPO_DEBITO = x.TIPO_DEBITO
                    })
                    .FirstOrDefault(x => x.ID == id);
            return Json(ListDebitosBancarios);
        }

        public JsonResult GetBancos()
        {
            var list = _db.BANCO.Select(x => new { id = x.BCO_CODIGO, value = x.BCO_NOMBRE }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTipoEntrada()
        {
            var list = _db.TIPODB1.Select(x => new { id = x.ID, value = x.DESCRIPCION }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetTipoDebito()
        {
            var list = _db.TIPODB2.Select(x => new { id = x.ID, value = x.DESCRIPCION }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCuentasBancarias(string id)
        {
            var list = _db.CTABANCO.Where(x => x.CTA_BANCO == id).Select(x => new { id = x.CTA_CODIGO, value = x.CTA_NUMERO }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PrintReport(string json)
        {
            message mensajeReturn = new message();
            View_fechas fechas = new View_fechas();
            mensajeReturn.Is_Success = true;

            try
            {
                fechas = Newtonsoft.Json.JsonConvert.DeserializeObject<View_fechas>(json);
                string intialPath = Server.MapPath(Parameters.rutaReporte);

                if (fechas.FechaDesdeDt == null)
                {
                    if (fechas.FechaDesdeDt == null)
                    {
                        mensajeReturn.Message = "Fecha desde es obligatoria";
                        mensajeReturn.Is_Success = false;
                    }
                }
                else if (fechas.FechaDesdeDt > fechas.FechaHastaDt)
                {
                    mensajeReturn.Message = "Fecha desde no puede ser mayor a fecha hasta";
                    mensajeReturn.Is_Success = false;
                }

                if (mensajeReturn.Is_Success)
                {
                    List<Parameters> prt = new List<Parameters>()
                    {
                        new Parameters(){ParameterName="FechaDesde",ParameterValue=fechas.FechaDesdeDt },
                        new Parameters(){ParameterName="FechaHasta",ParameterValue=fechas.FechaHastaDt }
                    };
                    Parameters.guardarReporte(Response, ReportName.DebitosBancarios, prt, intialPath);
                }
            }
            catch (Exception e)
            {
                mensajeReturn.Message = "Error inesperado: " + e.Message;
                mensajeReturn.Is_Success = false;
            }

            return Json(mensajeReturn);
        }

        public ActionResult generateReport()
        {

            try
            {
                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();
                string ruta = Server.MapPath(string.Format("{0}/{1}.pdf", Parameters.rutaReporte, ReportName.DebitosBancarios));

                var fsResult = Parameters.viewReportPDF(ruta);
                return fsResult;
            }
            catch (Exception e)
            {
                return Content("Error inesperado en el reporte: " + e.Message);

            }
        }
    }
}