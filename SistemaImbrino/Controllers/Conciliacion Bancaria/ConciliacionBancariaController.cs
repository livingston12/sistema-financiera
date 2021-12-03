using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using static SistemaImbrino.Models.Parameters;

namespace SistemaImbrino.Controllers.Conciliacion_Bancaria
{
    [Authorize(Roles = "conciliacion,admin")]
    public class ConciliacionBancariaController : BaseController
    {
        private DB_IMBRINOEntities _db = new DB_IMBRINOEntities();

        // GET: ConciliacionBancaria
        public ActionResult Index(string json, string cuentaBancaria, bool movimiento)
        {
            ViewBag.json = json;
            ViewBag.cuentaBancaria = cuentaBancaria;
            ViewBag.isMovimiento = movimiento;
            ViewBag.Title = movimiento ? "Movimientos bancarios" : "Conciliacion bancaria";
            if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(cuentaBancaria))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public JsonResult GetConciliacionBancaria(string json, string cuentaBancaria)
        {
            View_ResultValue<View_ConciliacionBancaria> result = new View_ResultValue<View_ConciliacionBancaria>();
            View_fechas fechas = new View_fechas();
            result.message = new message();

            try
            {

                fechas = Newtonsoft.Json.JsonConvert.DeserializeObject<View_fechas>(json);
                var dataValidation = validarCampos(fechas, cuentaBancaria);
                if (dataValidation.isError)
                {
                    result.message = dataValidation.data;
                }
                else
                {
                    result.data = GetConciliacionBancaria(fechas.FechaDesdeDt, fechas.FechaHastaDt, cuentaBancaria);
                    result.message.Is_Success = true;
                    result.message.Message = string.Empty;
                    ViewBag.BalanceCorte = getBalanceCuentaBancaria(cuentaBancaria);
                }
            }
            catch (Exception)
            {
                result.message.Is_Success = false;
                result.message.Message = MensajeErrorCath;
            }

            return Json(result);
        }

        public JsonResult SetValues(string json, string cuentaBancaria)
        {
            message result = new message();
            View_fechas fechas = new View_fechas();

            try
            {
                fechas = Newtonsoft.Json.JsonConvert.DeserializeObject<View_fechas>(json);
                var dataValidation = validarCampos(fechas, cuentaBancaria);
                if (dataValidation.isError)
                {
                    result = dataValidation.data;
                }
                else
                {
                    result.Is_Success = true;
                    result.Message = string.Empty;
                    ViewBag.json = json;
                    ViewBag.cuentaBancaria = cuentaBancaria;
                }
            }
            catch (Exception)
            {
                result.Is_Success = false;
                result.Message = MensajeErrorCath;
            }
            return Json(result);
        }

        private (message data, bool isError) validarCampos(View_fechas fechas, string cuentaBancaria)
        {
            List<message> messages = new List<message>();
            (message data, bool isError) result = (null, false);
            message message;
            string messageDescription = string.Empty;

            // Si fecha desde es mayor a fecha hasta
            if (fechas.FechaDesdeDt > fechas.FechaHastaDt)
            {
                messages.Add(new message()
                {
                    Message = $"El campo " +
                    $"<b>{nameof(fechas.FechaDesde)}</b> no puede ser mayor a " +
                    $"<b>{nameof(fechas.FechaHasta)}</b>"
                });
            }
            // Si cuenta bancaria es vacio
            if (string.IsNullOrEmpty(cuentaBancaria))
            {
                messages.Add(new message()
                {
                    Message = $"El campo " +
                    $"<b>{nameof(cuentaBancaria)}</b> no puede estar vacio "
                });
            }

            if (messages.Count > 0)
            {
                messageDescription = $"<li> {string.Join("</li><li>", messages.Select(x => x.Message))} </li>";
                message = new message()
                {
                    Is_Success = false,
                    Message = messageDescription
                };
                result.data = message;
                result.isError = true;
            }

            return result;
        }

        private IQueryable<View_ConciliacionBancaria> GetConciliacionBancaria(DateTime? fechaDesde, DateTime? fechaHasta, string cuentaBancaria)
        {
            IQueryable<View_ConciliacionBancaria> result = _db.VW_ConciliacionBancaria
                                           .Where(x =>
                                                 x.FECHA >= fechaDesde
                                                 && x.FECHA <= fechaHasta
                                                 && x.CUENTA == cuentaBancaria)
                                           .Select(x => new View_ConciliacionBancaria
                                           {
                                               Id = x.ID,
                                               Concepto = x.CONCEPTO,
                                               Credito = x.CREDITO,
                                               Cuenta = x.CUENTA,
                                               Debito = x.DEBITO,
                                               Fecha = x.FECHATXT,
                                               Transacion = x.TRANSACION,
                                               Validado = x.VALIDADO,
                                               BalanceGeneral = x.BALANCECORTE,
                                               Tipo = x.TIPO,
                                               Cerrado = x.CERRADO,
                                               FechaDt = x.FECHA
                                           })
                                           .OrderBy(x => x.FechaDt);
            return result;
        }

        public JsonResult GetCuentas()
        {

            var list = _db.CTABANCO
                .ToList()
                .Select(x => new
                {
                    id = x.CTA_NUMERO.Trim(),
                    value = $"{GetCuentas(x.CTA_NUMERO, x.CTA_BANCO)}"
                })
                .ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        private static string GetCuentas(string cuenta, string banco)
        {
            string nombreBanco = string.Empty;
            int.TryParse(banco, out int bancoId);
            var listBanco = db.BANCO.Where(x => x.BCO_CODIGO == bancoId).ToList();
            if (listBanco.Any())
            {
                nombreBanco = listBanco.FirstOrDefault().BCO_ABREVI;
            }

            return $"{nombreBanco} - {cuenta}";
        }

        public async Task<JsonResult> validarConciliacionAsync(int id, string tipo, bool validado)
        {
            message result = new message()
            {
                Is_Success = false
            };
            try
            {
                if (tipo == "DEBITO")
                {
                    var currentDebito = await _db.OTROSDB.FindAsync(id);
                    if (currentDebito == null)
                    {
                        throw new Exception();
                    }
                    currentDebito.VALIDADO = validado;
                    _db.Entry(currentDebito).State = System.Data.Entity.EntityState.Modified;
                }
                else if (tipo == "CREDITO")
                {
                    var currentCredito = await _db.OTROSCR.FindAsync(id);
                    if (currentCredito == null)
                    {
                        throw new Exception();
                    }
                    currentCredito.VALIDADO = validado;
                    _db.Entry(currentCredito).State = System.Data.Entity.EntityState.Modified;
                }
                await _db.SaveChangesAsync();
                result.Is_Success = true;
            }
            catch (Exception)
            {

            }
            return Json(result);
        }

        public async Task<JsonResult> CerrarConciliacionAsync(string json,DateTime fechaCorte)
        {

            message result = new message()
            {
                Is_Success = false
            };
            try
            {
                List<View_cierreConciliacion> cierres = Newtonsoft.Json.JsonConvert.DeserializeObject<List<View_cierreConciliacion>>(json);
                int cuenta = 0;
                int counter = 0;
                foreach (var cierre in cierres)
                {
                    if (cierre.tipo == "DEBITO")
                    {
                        var currentDebito = await _db.OTROSDB.FindAsync(cierre.id);
                        if (currentDebito == null)
                        {
                            throw new Exception("No existe el valor a buscar");
                        }
                        currentDebito.CERRADO = true;
                        _db.Entry(currentDebito).State = System.Data.Entity.EntityState.Modified;
                        if (counter == 0)
                        {
                            cuenta = currentDebito.CUENTA_BANCARIA;
                        }
                    }
                    else if (cierre.tipo == "CREDITO")
                    {
                        var currentCredito = await _db.OTROSCR.FindAsync(cierre.id);
                        if (currentCredito == null)
                        {
                            throw new Exception("No existe el valor a buscar");
                        }
                        currentCredito.CERRADO = true;
                        _db.Entry(currentCredito).State = System.Data.Entity.EntityState.Modified;
                        if (counter == 0)
                        {
                            cuenta = currentCredito.CUENTA_BANCARIA;
                        }
                    }
                    counter++;
                }
                if (cierres.Any())
                {
                    await actualizarCuenta(cuenta, cierres.FirstOrDefault().balance, fechaCorte);
                    await _db.SaveChangesAsync();
                    result.Is_Success = true;
                    result.Message = "Se realizo el corte correctamente";
                }
                else
                {
                    result.Is_Success = false;
                    result.Message = "No existen regitros para cerrar conciliacion";
                }
                
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return Json(result);
        }

        private async Task<bool> actualizarCuenta(int cuentaBancaria, string balanceCorte,DateTime fechaCorte)
        {
            bool result = false;
            try
            {
                CTABANCO ctaBanco = await _db.CTABANCO.FindAsync(cuentaBancaria);
                ctaBanco.CTA_BALCOR = balanceCorte.Replace(",", "");
                ctaBanco.CTA_FECCOR = fechaCorte;
                _db.Entry(ctaBanco).State = System.Data.Entity.EntityState.Modified;
            }
            catch (Exception)
            {
                throw new Exception("Ocurrio un problema para actualizar la cuenta");
            }

            return result;
        }

        public ActionResult PrintReport(string json, string filtroCuenta, string balance,bool isMovimiento)
        {
            message mensajeReturn = new message();
            mensajeReturn.Is_Success = true;

            try
            {
                View_fechas fechas = Newtonsoft.Json.JsonConvert.DeserializeObject<View_fechas>(json);
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
                        new Parameters(){ParameterName="FechaHasta",ParameterValue=fechas.FechaHastaDt },
                        new Parameters(){ParameterName="FiltroCuenta",ParameterValue=filtroCuenta },
                        new Parameters(){ParameterName="FechaCorte",ParameterValue=fechas.FechaHastaDt },
                        new Parameters(){ParameterName="Balance",ParameterValue=balance },
                        new Parameters(){ParameterName="isMovimiento",ParameterValue=isMovimiento }
                    };
                    Parameters.guardarReporte(Response, ReportName.ConciliacionBancaria, prt, intialPath);
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
                string ruta = Server.MapPath(string.Format("{0}/{1}.pdf", Parameters.rutaReporte, ReportName.ConciliacionBancaria));

                var fsResult = Parameters.viewReportPDF(ruta);
                return fsResult;
            }
            catch (Exception e)
            {
                return Content("Error inesperado en el reporte: " + e.Message);

            }
        }

        public JsonResult GetFechaCorte(string cuentaId)
        {
            string result = string.Empty;
            try
            {
                var cuenta = _db.CTABANCO.Where(x => x.CTA_NUMERO == cuentaId).FirstOrDefault();
                if (cuenta != null)
                {
                    result = cuenta.CTA_FECCOR.HasValue ?
                        cuenta.CTA_FECCOR.Value.AddDays(1).ToString("yyyy-MM-dd")
                        : DateTime.MinValue.ToString("yyyy-MM-dd");

                }
            }
            catch (Exception)
            {
            }

            return Json(result);
        }
    }
}