using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static SistemaImbrino.Models.Parameters;

namespace SistemaImbrino.Controllers.Conciliacion_Bancaria
{
    public class CreditosBancariosController : BaseController
    {
        private DB_IMBRINOEntities _db = new DB_IMBRINOEntities();

        // GET: CreditosBancarios
        public ActionResult Index()
        {
            ViewBag.listFilters = _db.TIPOCR1;
            return View();
        }

        public JsonResult getCurrentCreditoBancario(string btnId)
        {
            string value = btnId.Replace("btn-", "").ToUpper();
            bool findFilter = string.Compare(value, "todos", StringComparison.CurrentCultureIgnoreCase) == 0 ?
                                false : true;
            List<View_ListCreditosBancarios> ListCreditosBancarios =
                _db.OTROSCR
                    .Where(x => x.Activo == true)
                    .Select(x => new View_ListCreditosBancarios()
                    {
                        ID = x.ID,
                        ID_BANCO = x.BANCO,
                        ID_CUENTA_BANCARIA = x.CUENTA_BANCARIA,
                        BENEFICIARIO = x.BENEFICIARIO,
                        NUMERO_CHEQUE = x.NUMERO_CHEQUE,
                        ID_TIPO_CREDITO = x.TIPO_CREDITO,
                        CONCEPTO = x.CONCEPTO,
                        Activo = x.Activo,
                        ID_TIPO_SALIDA = x.TIPO_SALIDA,
                        MONTO = x.MONTO,
                        FECHAdt = x.FECHA
                    })
                    .ToList();
            if (findFilter)
            {
                ListCreditosBancarios = ListCreditosBancarios.Where(x => x.TIPO_CREDITO == value).ToList();
            }
            return Json(ListCreditosBancarios);
        }

        public JsonResult CrearCreditoBancario(OTROSCR CreditoBancario)
        {
            message message;
            try
            {
                message = validarCreditoBancario(CreditoBancario);
                if (!message.Is_Success)
                {
                    return Json(message);
                }
                CreditoBancario.Activo = true;
                CreditoBancario.NUMERO_CHEQUE = CreditoBancario.NUMERO_CHEQUE == null ?
                                                    string.Empty :
                                                    CreditoBancario.NUMERO_CHEQUE;
                CreditoBancario.BENEFICIARIO = CreditoBancario.BENEFICIARIO == null ?
                                                    string.Empty :
                                                    CreditoBancario.BENEFICIARIO;
                // disminuir monto cuenta bancaria
                CreditoBancario.CERRADO = false;
                CreditoBancario.VALIDADO = false;
                modificarBalanceFecha(_db, CreditoBancario.CUENTA_BANCARIA, CreditoBancario.MONTO,tipoBalanceFecha.disminuir);
                
                _db.OTROSCR.Add(CreditoBancario);
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Credito bancario insertado correctamente",
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

        private message validarCreditoBancario(OTROSCR creditoBancario)
        {
            List<string> ListMessage = new List<string>();
            message message = new message();
            if (string.IsNullOrEmpty(creditoBancario.CONCEPTO))
            {
                ListMessage.Add($"<li>El campo {nameof(creditoBancario.CONCEPTO)} es obligatorio</li>");
            }
            if (creditoBancario.MONTO <= 0)
            {
                ListMessage.Add($"<li>El campo {nameof(creditoBancario.MONTO)} es obligatorio</li>");
            }
            if (creditoBancario.FECHA.HasValue == false)
            {
                ListMessage.Add($"<li>El campo {nameof(creditoBancario.FECHA)} es obligatorio</li>");
            }
            if (creditoBancario.BANCO <= 0)
            {
                ListMessage.Add($"<li>El campo {nameof(creditoBancario.BANCO)} es obligatorio</li>");
            }
            if (creditoBancario.TIPO_CREDITO <= 0)
            {
                ListMessage.Add($"<li>El campo {nameof(creditoBancario.TIPO_CREDITO).Replace("_", " ")} es obligatorio</li>");
            }
            if (creditoBancario.TIPO_SALIDA <= 0)
            {
                ListMessage.Add($"<li>El campo {nameof(creditoBancario.TIPO_SALIDA).Replace("_", " ")} es obligatorio</li>");
            }
            if (creditoBancario.CUENTA_BANCARIA <= 0)
            {
                ListMessage.Add($"<li>El campo {nameof(creditoBancario.CUENTA_BANCARIA).Replace("_", " ")} es obligatorio</li>");
            }

            message = new message
            {
                Is_Success = ListMessage.Count() == 0,
                Message = $"<ul>{string.Join("", ListMessage)} </ul>"
            };
            return message;
        }

        public JsonResult ActualizarCreditoBancario(OTROSCR creditoBancario)
        {
            message message;
            try
            {
                _db = new DB_IMBRINOEntities();
                var credito = _db.OTROSCR.Find(creditoBancario.ID);
                if (credito == null)
                {
                    message = new message()
                    {
                        Message = "Credito bancario no existe",
                        Is_Success = false
                    };
                    return Json(message);
                }
                string beneficiario = string.Empty;
                string numeroCheque = string.Empty;

                beneficiario = creditoBancario.BENEFICIARIO == null ?
                                string.Empty :
                                creditoBancario.BENEFICIARIO;
                numeroCheque = creditoBancario.NUMERO_CHEQUE == null ?
                           string.Empty :
                           creditoBancario.NUMERO_CHEQUE;

                credito.BANCO = creditoBancario.BANCO;
                credito.BENEFICIARIO = beneficiario;
                credito.CONCEPTO = creditoBancario.CONCEPTO;
                credito.CUENTA_BANCARIA = creditoBancario.CUENTA_BANCARIA;
                credito.FECHA = creditoBancario.FECHA;
                credito.MONTO = creditoBancario.MONTO;
                credito.NUMERO_CHEQUE = numeroCheque;
                credito.TIPO_CREDITO = creditoBancario.TIPO_CREDITO;
                credito.TIPO_SALIDA = creditoBancario.TIPO_SALIDA;
                
                _db.Entry(credito).State = System.Data.Entity.EntityState.Modified;
                modificarBalanceFecha(_db, creditoBancario.CUENTA_BANCARIA, credito.MONTO, creditoBancario.MONTO);
                _db.SaveChanges();
                message = new message()
                {
                    Message = "Credito bancario actualizado correctamente",
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

        public JsonResult EliminarCreditoBancario(int id)
        {
            message message;
            try
            {
                var creditoBancario = _db.OTROSCR.Find(id);

                if (creditoBancario == null)
                {
                    message = new message()
                    {
                        Message = "Credito bancario no existe",
                        Is_Success = false
                    };
                    return Json(message);
                }
                creditoBancario.Activo = false;
                _db.Entry(creditoBancario).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();

                message = new message()
                {
                    Message = "Credito bancario eliminado correctamente",
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

        public JsonResult GetCreditoBancarioDetail(int id)
        {
            View_ListCreditosBancarios ListCreditosBancarios =
                _db.OTROSCR
                    .Where(x => x.Activo == true)
                    .Select(x => new View_ListCreditosBancarios()
                    {
                        ID = x.ID,
                        ID_BANCO = x.BANCO,
                        ID_CUENTA_BANCARIA = x.CUENTA_BANCARIA,
                        BENEFICIARIO = x.BENEFICIARIO,
                        NUMERO_CHEQUE = x.NUMERO_CHEQUE,
                        ID_TIPO_CREDITO = x.TIPO_CREDITO,
                        CONCEPTO = x.CONCEPTO,
                        Activo = x.Activo,
                        ID_TIPO_SALIDA = x.TIPO_SALIDA,
                        MONTO = x.MONTO,
                        FECHAdt = x.FECHA
                    })
                    .FirstOrDefault(x => x.ID == id);
            return Json(ListCreditosBancarios);
        }

        public JsonResult GetBancos()
        {
            var list = _db.BANCO.Select(x => new { id = x.BCO_CODIGO, value = x.BCO_NOMBRE }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTipoCredito()
        {
            var list = _db.TIPOCR1.Select(x => new { id = x.ID, value = x.DESCRIPCION }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTipoSalida()
        {
            var list = _db.TIPOCR2.Select(x => new { id = x.ID, value = x.DESCRIPCION }).ToList();
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
                    Parameters.guardarReporte(Response, ReportName.CreditosBancarios, prt, intialPath);
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
                string ruta = Server.MapPath(string.Format("{0}/{1}.pdf", Parameters.rutaReporte, ReportName.CreditosBancarios));

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