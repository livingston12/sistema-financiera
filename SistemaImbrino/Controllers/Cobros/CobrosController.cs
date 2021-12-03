using Newtonsoft.Json;
using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using static SistemaImbrino.Models.Parameters;

namespace SistemaImbrino.Controllers
{
    [Authorize(Roles = "cobros,admin")]
    public class CobrosController : BaseController
    {
        private DB_IMBRINOEntities _db = new DB_IMBRINOEntities();
        private List<INGCUOTA> ListiNGCUOTAs = new List<INGCUOTA>();
        private List<ABOCUOTA> ListaBOCUOTA = new List<ABOCUOTA>();
        private List<INGOTRO> ListINGOtros = new List<INGOTRO>();
        private List<ABOOCARG> ListAboCarg = new List<ABOOCARG>();
        private List<OTROCARG> ListOtroCarg = new List<OTROCARG>();
        private message Messajson = new message();


        private string _FinID { get; set; }
        private int FinIDint = 0;
        private DateTime Fecha_pago { get; set; }

        private void ReturnFechapago(string _Fecha_pago)
        {
            try
            {
                Fecha_pago = _Fecha_pago != null ? DateTime.Parse(_Fecha_pago) : DateTime.Now;
            }
            catch (Exception)
            {
                Fecha_pago = DateTime.Now;
            }
            //return Fecha_pago;
        }

        // GET: Cobros
        public async Task<ActionResult> Index(string FinID = "", string _Fecha_pago = null)
        {
            _db = new DB_IMBRINOEntities();
            ReturnFechapago(_Fecha_pago);

            _FinID = FinID;
            string FinNum = FinID;
            string strOtro = "otros";
            
            int.TryParse(FinID, out FinIDint);
            await getTotalCuotas(FinIDint);

            VW_rptFinAtrasados listAtrasados = await _db.VW_rptFinAtrasados
                                                        .Where(x => x.C__FIN == FinIDint)
                                                        .FirstOrDefaultAsync();
            if (FinID == "" || listAtrasados == null)
                return RedirectToAction("Index", "CobrosHeader");
            ViewBag.ListAtrasados = listAtrasados;
            List<sp_cuotasVencidas_Result> ListCuotasVencidas = _db.sp_cuotasVencidas(Fecha_pago).Where(x => x.C__FIN == FinIDint && x.tipo == strOtro).OrderBy(x => x.fechadt).ToList();
            ListCuotasVencidas.AddRange(_db.sp_cuotasVencidas(Fecha_pago).Where(x => x.C__FIN == FinIDint && x.tipo != strOtro).OrderBy(x => x.fechadt).ThenBy(x => x.NUM_CUOTA).ToList());

            ViewBag.Mora = String.Format("{0:n}", ListCuotasVencidas.Sum(x => x.mora));
            ViewBag.ListCuotasVencidas = ListCuotasVencidas;
            ViewBag.isCobroTotal = ListCuotasVencidas.Where(x => x.NUM_CUOTA.isNumber() == false).Any();
            ViewBag.FechaPago = returnDate(Fecha_pago);
            return View();
        }

        public async Task<int?> getTotalCuotas(int numFin)
        {
            string fecha_pagado = string.Empty;
            ViewBag.CUOTA_PENDIENTE = await _db.CUOTA.Where(x => x.CUO_NUMFAC == numFin && x.CUO_FECHAP == fecha_pagado).CountAsync();
            ViewBag.CUOTA_TOTAL = await _db.CUOTA.Where(x => x.CUO_NUMFAC == numFin).CountAsync();
            return null;
        }

        public ActionResult _CobrosHeader()
        {

            //var finAtra =  db.VW_rptFinAtrasados.Where(x => x.C__FIN == FinID.ToString()).FirstOrDefault();

            return PartialView();
        }

        public ActionResult _CobrosDetalle()
        {

            //var finAtra =  db.VW_rptFinAtrasados.Where(x => x.C__FIN == FinID.ToString()).FirstOrDefault();

            return PartialView();
        }

        public ActionResult _popup_Cobro(string id_numFin, string id_cuota, string tipo, string idIndex, string _Fecha_pago = null)
        {
            ReturnFechapago(_Fecha_pago);
            int.TryParse(id_numFin, out FinIDint);
            ViewBag.tipoCobro = tipo;
            ViewBag.Index = idIndex;
            var model = _db.sp_cuotasVencidas(Fecha_pago).Where(x => x.C__FIN == FinIDint && x.NUM_CUOTA == id_cuota).FirstOrDefault();
            return PartialView(model);
        }

        public ActionResult _popup_ProcesarPagoTotal(string _FinID = "", string _Fecha_pago = null)
        {
            ReturnFechapago(_Fecha_pago);
            int.TryParse(_FinID, out FinIDint);
            var cliente = _db.sp_cuotasVencidas(Fecha_pago).Where(x => x.C__FIN == FinIDint).FirstOrDefault().CLIENTE;
            View_cobrosDetalle model = CobrosHeader_SP(cliente, fecha_pago: Fecha_pago).Where(x => x.cliente == _FinID).FirstOrDefault();
            return PartialView(model);
        }


        public ActionResult _popup_ProcesarPago()
        {
            return View();
        }
        public async Task<JsonResult> Cobrar(string json, string tipoPago, string _Fecha_pago = null, int aumentoRecibo = 0, string JsonDeposito = "")
        {
            ReturnFechapago(_Fecha_pago);
            if (_db.Database.Connection.State == System.Data.ConnectionState.Open)
                _db.Database.Connection.Close();
            (bool validar, int numRecibo) result = (true, 0);
            var tran = _db.Database.BeginTransaction();
            bool error = false;
            try
            {

                View_Cobrar _View_Cobrar = JsonConvert.DeserializeObject<View_Cobrar>(json);
                OTROSDB ViewDeposito = new OTROSDB();

                if (tipoPago != "E" && tipoPago != "C")
                {
                    try
                    {
                        ViewDeposito = JsonConvert.DeserializeObject<OTROSDB>(JsonDeposito);
                    }
                    catch (Exception)
                    {
                        Messajson.Message = "Ocurrio un problema con el pago: Favor rellenar los campos requeridos";
                        Messajson.Is_Success = false;
                        tran.Rollback();
                        return Json(Messajson);
                    }
                    Messajson = validarMetodoPago(tipoPago, ViewDeposito);
                }
                else
                {
                    Messajson.Is_Success = true;
                }
                decimal descuento = 0; int num = 0;
                error = !Messajson.Is_Success;

                if (Messajson.Is_Success)
                {
                    foreach (var item in _View_Cobrar.Cobros)
                    {
                        if (item.tipoCobro == "Registrar mora como cargo")
                        {
                            addNewOtroCarg(item, Fecha_pago);
                        }
                        if (!int.TryParse(item.numCuota, out int cuota))
                        {
                            result = await aplicarCargosAdiccionales(item, tipoPago, Fecha_pago);
                            if (result.validar)
                            {
                                error = true;
                                Messajson = new message()
                                {
                                    Is_Success = false,
                                    Message = "Error al procesar Cargo adicional"
                                };
                                break;
                            }
                        }
                        else
                        {
                            // ref descuento, ref descuento, ref num, ref result.numRecibo,
                            var resultCobro = await RealizarCobro(item, tipoPago, descuento, descuento, num, result.numRecibo, aumentoRecibo: aumentoRecibo);
                            if (resultCobro.cobro == false)
                            {
                                error = true;
                                Messajson = new message()
                                {
                                    Is_Success = false,
                                    Message = "Error al procesar Cobro"
                                };
                                break;
                            }
                            result.numRecibo = resultCobro.numRecibo;
                        }
                    }

                }

                if (error == false)
                {
                    if (tipoPago == "D" || tipoPago == "T")
                    {
                        int numFin = _View_Cobrar.Cobros.FirstOrDefault().numfin;

                        var resultCuota = await MaxiNGCUOTARef();
                        var inserto = await GuardarCuota(ViewDeposito, numFin, resultCuota.numRec);
                        if (inserto == 0)
                        {
                            Messajson.Message = "Error el cobro no se inserto correctamente";
                            Messajson.Is_Success = false;
                            return Json(Messajson);
                        }
                        result.numRecibo = resultCuota.numRec;
                    }
                    await GuardarCuota();
                    tran.Commit();
                    Messajson.NumRecibo = result.numRecibo;
                    Messajson.Message = "Se completo el pago correctamente";
                    Messajson.Is_Success = true;
                }
                else
                {
                    tran.Rollback();
                }
            }
            catch (Exception e)
            {
                Messajson.Message = "Ocurrio un problema con el pago: " + e.Message;
                Messajson.Is_Success = false;
                tran.Rollback();
            }

            return Json(Messajson);
        }



        private async Task<(bool validar, int numRecibo)> aplicarCargosAdiccionales(cl_cobro cobro, string tipoPago, DateTime Fecha_pago)
        {
            List<sp_cuotasVencidas_Result> ListCuotaVen = _db.sp_cuotasVencidas(Fecha_pago).Where(x => x.C__FIN == cobro.numfin && x.NUM_CUOTA == cobro.numCuota).ToList();
            (bool validar, int numRecibo) result = (false, 0);
            try
            {
                if (ListCuotaVen.Any())
                {
                    string descripcion = string.Empty
                        , fecha = string.Empty
                        , strIngreso = string.Empty;
                    string cliente = ListCuotaVen.FirstOrDefault().CLIENTE;
                    int status = 0, idCargo = 0;
                    double montoCargo = cobro.otroCargo;
                    string strmontoCargo = cobro.otroCargo.ToString();
                    string numCuota = await getCuotaAdiccional(cobro.numCuota);

                    fecha = Fecha_pago.ToString(formatoFecha);
                    OTROCARG otroCarg = await _db.OTROCARG
                                                    .Where(x =>
                                                                x.CAR_NUMFIN == cobro.numfin.ToString() &&
                                                                x.CAR_SECU == numCuota
                                                           )
                                                    .FirstOrDefaultAsync();
                    var ingreso = await getTipoIngresoAsync(otroCarg.CAR_CODCAR);

                    if (otroCarg == null || string.IsNullOrEmpty(ingreso))
                    {
                        result.validar = true;
                    }
                    else
                    {
                        strIngreso = ingreso;
                        int.TryParse(otroCarg.CAR_CODCAR, out idCargo);
                        var resultNcf = await MaxiNGCUOTARef();

                        TipoCobro tipoCobro = await ReturnTipoCobro(cobro.tipoCobro);

                        if (tipoCobro.ToString() == TipoCobro.Pago.ToString())
                        {
                            status = (int)TipoCobro.Pago;
                            descripcion = string.Format("{0} ${1}", strIngreso, montoCargo);
                        }
                        else if (tipoCobro.ToString() == TipoCobro.Abono.ToString())
                        {
                            status = (int)TipoCobro.Abono;
                            descripcion = string.Format("{0} {1} ${2}", "ABONO A", strIngreso, montoCargo);
                            await Task.Run(() => addNewAboCargo(otroCarg, strmontoCargo, Fecha_pago));
                        }
                        await Task.Run(() =>
                                            addNewIngOtro(
                                                            fecha,
                                                            tipoPago,
                                                            descripcion,
                                                            montoCargo,
                                                            resultNcf.ncf,
                                                            cobro.numfin,
                                                            cliente,
                                                            resultNcf.numRec,
                                                            status.ToString(),
                                                            idCargo
                                                        ));
                        await Task.Run(() =>
                                            pagarOtroCargoAdiccional(
                                                                        otroCarg,
                                                                        fecha,
                                                                        tipoCobro,
                                                                        resultNcf.numRec
                                                                    ));
                        result.numRecibo = resultNcf.numRec;
                    }
                }
                else
                {
                    result.validar = true;
                }
            }
            catch (Exception)
            {
                result.validar = true;
            }

            return result;
        }

        private Task<string> getCuotaAdiccional(string numCuota)
        {
            string cuota = string.Empty;
            try
            {
                var listPartition = numCuota.Split('-');
                int maxNum = listPartition.Length - 1;
                cuota = listPartition[maxNum];
            }
            catch (Exception)
            {
                cuota = string.Empty;
            }

            return Task.Run(() => cuota);
        }

        private async Task<int> GuardarCuota(OTROSDB viewDeposito = null, int numFin = 0, int numRec = 0)
        {
            int result = 0;
            if (viewDeposito != null)
            {
                viewDeposito.ACTIVO = true;
                viewDeposito.NUM_FIN = numFin;
                viewDeposito.NUM_REC = numRec;
                viewDeposito.STATUS = (int)Status.NUEVO;
                _db.OTROSDB.Add(viewDeposito);
            }
            else
            {
                _db.ABOCUOTA.AddRange(ListaBOCUOTA);
                _db.INGCUOTA.AddRange(ListiNGCUOTAs);
                _db.ABOOCARG.AddRange(ListAboCarg);
                _db.INGOTRO.AddRange(ListINGOtros);
                _db.OTROCARG.AddRange(ListOtroCarg);
            }
            result = await _db.SaveChangesAsync();
            return result;
        }

        private async Task<(bool cobro, int numRecibo, (decimal descuentoMora, decimal descuentoInteres, int totalDecuotasPagar) descuentos)> RealizarCobro(cl_cobro cobro, string tipoPago, decimal descuentoMora, decimal descuentoInte, int totalCuotasPagar, int numRecibo, bool isOne = true, int aumentoRecibo = 0)
        {
            (bool cobro, int numRecibo, (decimal descuentoMora, decimal descuentoInteres, int totalDecuotasPagar) descuentos) result = (false, numRecibo, (descuentoMora, descuentoInte, totalCuotasPagar));
            int numfin = cobro.numfin, idCuota = 0, NUMREC = 0;
            double mora = 0;
            decimal MontoCapital = 0, MontoCapitalOr = 0
                , montoInteres = 0
                , montoPagado = cobro.montoPagado;
            var _tipoCobro = await ReturnTipoCobro(cobro.tipoCobro);
            string tipoCobro = _tipoCobro.ToString()
            , fecha = Fecha_pago.ToString(formatoFecha)
            , formaPago = tipoPago
            , descri = ""
            , TotalCuota = ""
            , ncf = ""
            , numCuota = cobro.numCuota
            , descriMora = ""
            , _status = ""
            , id_tipoCobro = "0";

            int.TryParse(cobro.numCuota, out idCuota);
            INGCUOTA iNGCUOTA = new INGCUOTA();
            //Buscar cuota
            List<sp_cuotasVencidas_Result> ListCuotaVen = await Task.Run(() =>
            {
                return _db.sp_cuotasVencidas(Fecha_pago)
                    .Where(x =>
                            x.C__FIN == numfin &&
                            x.NUM_CUOTA == numCuota
                            )
                    .ToList();
            });
            if (ListCuotaVen.Any() == false)
                return result;
            _status = ListCuotaVen.FirstOrDefault().CUO_STATUS;

            // Asignar valores de la cuota existente
            MontoCapital = ListCuotaVen.FirstOrDefault().CapitalTotal;
            montoInteres = ListCuotaVen.FirstOrDefault().InteresTotal;
            TotalCuota = ListCuotaVen.FirstOrDefault().CUOTA;

            string MontoTotal = (MontoCapital + montoInteres).ToString();
            string MontoTotalDesc = (MontoCapital + montoInteres).ToString("N0");
            Messajson = message.messageErrors(cobro.montoPagado.ToString(), cobro.mora.ToString(), MontoTotal, ListCuotaVen.FirstOrDefault().mora.ToString(), numCuota);

            if (Messajson.Is_Success == false && isOne)
                return result;

            var cuota = await _db.CUOTA.Where(x => x.CUO_NUMFAC == numfin && x.CUO_NUMCUO == idCuota).FirstOrDefaultAsync();


            if (tipoCobro == TipoCobro.Abono.ToString())
            {
                MontoCapitalOr = MontoCapital;
                //Se aplica el abono capital primero se descuento hasta que se pague completo luego interes
                MontoCapital = MontoCapital - montoPagado;

                // si el capital es negativo se le resta ese mismo monto al interes
                if (MontoCapital < 0)
                {
                    montoInteres = MontoCapital * -1;
                    MontoCapital = MontoCapitalOr;
                }
                else
                {
                    montoInteres = 0;
                    MontoCapital = montoPagado;
                }
                montoInteres = montoInteres > 0 ? montoInteres : 0;
                MontoTotal = (MontoCapital + montoInteres).ToString();

                id_tipoCobro = ((int)TipoCobro.Abono).ToString();
                descri = string.Format("{2} {0} ${1}", TotalCuota, MontoTotal, TipoCobro.Abono.ToString().ToUpper());

                int Id_Abono = await MaxABOCUOTA(numCuota, numfin);

                // Agregar Abono de cuota
                ABOCUOTA aBOCUOTA = new ABOCUOTA()
                {
                    ABO_NUMCUO = numCuota,
                    ABO_NUMFAC = numfin.ToString(),
                    ABO_FECHA = fecha,
                    ABO_MONTOC = MontoCapital,
                    ABO_MONTOI = montoInteres,
                    ABO_MONTOT = decimal.Parse(MontoTotal),
                    ABO_NUMABO = (Id_Abono + 1).ToString()
                };
                ListaBOCUOTA.Add(aBOCUOTA);
                var resultCuota = await MaxiNGCUOTARef();
                NUMREC = resultCuota.numRec;
                ncf = resultCuota.ncf;
            }
            else if (tipoCobro == TipoCobro.Pago.ToString())
            {
                if (isOne == false)
                {
                    if (result.descuentos.descuentoInteres >= montoInteres)
                    {
                        result.descuentos.descuentoInteres -= montoInteres;
                        montoInteres = 0;
                    }
                    else
                    {
                        montoInteres -= result.descuentos.descuentoInteres;
                        result.descuentos.descuentoInteres = 0;
                    }

                    if (result.descuentos.descuentoMora >= cobro.mora)
                    {
                        result.descuentos.descuentoMora -= cobro.mora;
                        cobro.mora = 0;
                    }
                    else
                    {
                        cobro.mora -= result.descuentos.descuentoMora;
                        result.descuentos.descuentoMora = 0;
                    }
                    MontoTotal = (MontoCapital + montoInteres).ToString();
                    MontoTotalDesc = (MontoCapital + montoInteres).ToString("N0");
                }
                string strAbonado = ((int)Status.ABONO).ToString();
                string strSaldo = (cuota.CUO_STATUS == strAbonado) ? "COMPLETIVO" : "SALDO";

                descri = string.Format("{2} {0} ${1}", TotalCuota, MontoTotalDesc, strSaldo);
                id_tipoCobro = ((int)TipoCobro.Pago).ToString();
                NUMREC = 0;
                var resultCuota = await MaxiNGCUOTARef();
                NUMREC = resultCuota.numRec;
                ncf = resultCuota.ncf;
            }
            descriMora = string.Format("{2} {0} ${1}", TotalCuota, cobro.mora.ToString("N0"), "MORA");
            _status = id_tipoCobro;
            // si La cuota creada ya existe solo se suma el monto
            if (ListiNGCUOTAs.Count > 0)
            {
                string montoTMora = ListINGOtros.Any() ? ListINGOtros.FirstOrDefault().ING_MONTOT.ToString() : "0";
                string descritxt = ListiNGCUOTAs.FirstOrDefault().ING_DESCRI;
                string descritxtMora = ListINGOtros.Any() ? ListINGOtros.FirstOrDefault().ING_DESCRI : "";
                decimal MontoTotalNum = 0, montoInteresNum = 0, montoTotalMora = 0, MontoCapitalNum = 0;

                decimal.TryParse(ListiNGCUOTAs.FirstOrDefault().ING_MONTOC.ToString(), out MontoCapitalNum);
                decimal.TryParse(ListiNGCUOTAs.FirstOrDefault().ING_MONTOI.ToString(), out montoInteresNum);
                decimal.TryParse(ListiNGCUOTAs.FirstOrDefault().ING_MONTOT.ToString(), out MontoTotalNum);
                decimal.TryParse(montoTMora, out montoTotalMora);

                string[] ListDescri = null;
                string[] ListDescriMora = null;

                if (isOne == false)
                {
                    ListDescri = new string[] { descri, descritxt };
                    if (cobro.mora > 0)
                    {
                        ListDescriMora = new string[] { descriMora, descritxtMora };
                    }
                }
                else
                {
                    ListDescri = new string[] { descritxt, descri };
                    if (cobro.mora > 0)
                    {
                        ListDescriMora = new string[] { descritxtMora, descriMora };
                    }
                }

                descritxt = string.Join(", ", ListDescri);
                if (ListDescriMora != null)
                {
                    descritxtMora = string.Join(", ", ListDescriMora);
                }

                // Para aplicar a la cuota
                MontoCapitalNum += MontoCapital;
                montoInteresNum += montoInteres;
                MontoTotalNum += decimal.Parse(MontoTotal);

                ListiNGCUOTAs.FirstOrDefault().ING_DESCRI = descritxt;
                ListiNGCUOTAs.FirstOrDefault().ING_MONTOC = MontoCapitalNum;
                ListiNGCUOTAs.FirstOrDefault().ING_NCF = ncf;
                ListiNGCUOTAs.FirstOrDefault().ING_MONTOI = montoInteresNum;
                ListiNGCUOTAs.FirstOrDefault().ING_MONTOT = MontoTotalNum;

                if (cobro.mora > 0)
                {
                    montoTotalMora += cobro.mora;
                    double.TryParse(montoTotalMora.ToString(), out mora);
                    if (ListINGOtros.Any() == false)
                        await Task.Run(() =>
                        {
                            addNewIngOtro(Fecha_pago.ToString(formatoFecha), formaPago, descriMora, mora, ncf, numfin, ListCuotaVen.FirstOrDefault().CLIENTE, NUMREC, _status);
                        });
                    ListINGOtros.FirstOrDefault().ING_DESCRI = descritxtMora;
                    ListINGOtros.FirstOrDefault().ING_MONTOT = mora;
                    ListINGOtros.FirstOrDefault().ING_NCF = ncf;
                }
            }
            else
            {
                NUMREC += aumentoRecibo;
                iNGCUOTA = new INGCUOTA()
                {
                    ING_NUMFIN = numfin,
                    ING_NUMREC = NUMREC,
                    ING_FORMPG = formaPago,
                    ING_DESCRI = descri,
                    ING_MONTOC = MontoCapital,
                    ING_MONTOI = montoInteres,
                    ING_MONTOT = decimal.Parse(MontoTotal),
                    ING_NCF = ncf,
                    ING_STATUS = _status,
                    ING_FECHA = Fecha_pago.ToString("dd/MM/yyyy"),
                    isCuadrada = false
                };
                ListiNGCUOTAs.Add(iNGCUOTA);

                if (cobro.mora > 0)
                {
                    double.TryParse(cobro.mora.ToString(), out mora);
                    await Task.Run(() =>
                    {
                        addNewIngOtro(Fecha_pago.ToString(formatoFecha), formaPago, descriMora, mora, ncf, numfin, ListCuotaVen.FirstOrDefault().CLIENTE, NUMREC, _status);
                    });
                }
            }

            //Actualizar estatus de la cuota             
            cuota.CUO_STAANT = cuota.CUO_STATUS;
            cuota.CUO_STATUS = id_tipoCobro;
            cuota.CUO_NUMREC = NUMREC;
            cuota.CUO_FECHAP = Fecha_pago.ToString(formatoFecha);
            result.numRecibo = NUMREC;
            _db.Entry(cuota).State = EntityState.Modified;
            result.cobro = true;
            return result;
        }

        public async Task<JsonResult> cobroTotal(string _numFin, decimal descuentoInte, decimal descuentoMora, string tipoPago, string _Fecha_pago = null)
        {
            message Messajson = new message();
            ReturnFechapago(_Fecha_pago);
            var tran = _db.Database.BeginTransaction();
            int numRecibo = 0;

            try
            {

                int intFinID = int.Parse(_numFin);
                List<sp_cuotasVencidas_Result> cuotasVenDesc = _db.sp_cuotasVencidas(Fecha_pago).Where(x => x.C__FIN == intFinID).OrderByDescending(x => x.NUM_CUOTA).ToList();
                if (cuotasVenDesc.Where(x => x.NUM_CUOTA.isNumber() == false).Any())
                {
                    Messajson.Message = "Debe saldar todos los cargos antes de hacer un saldo total";
                    Messajson.Is_Success = false;
                }
                else
                {
                    cl_cobro cobro = new cl_cobro();
                    int totalCuotasPagar = cuotasVenDesc.Count;

                    foreach (var cu in cuotasVenDesc)
                    {
                        cobro = new cl_cobro()
                        {
                            numfin = intFinID,
                            numCuota = cu.NUM_CUOTA.ToString(),
                            tipoCobro = TipoCobro.Pago.ToString(),
                            mora = cu.mora,
                            montoPagado = decimal.Parse((cu.capital + cu.INTERES).ToString())
                        };

                        var result = await RealizarCobro(cobro, tipoPago, descuentoMora, descuentoInte, totalCuotasPagar, numRecibo, false);
                        descuentoMora = result.descuentos.descuentoMora;
                        descuentoInte = result.descuentos.descuentoInteres;
                    }
                    await GuardarCuota();
                    tran.Commit();
                    Messajson.NumRecibo = numRecibo;
                    Messajson.Message = "Se completo el pago correctamente";
                    Messajson.Is_Success = true;
                }
            }
            catch (Exception e)
            {
                Messajson.Message = "Ocurrio un problema con el pago: " + e.Message;
                Messajson.Is_Success = false;
                tran.Rollback();
            }

            return Json(Messajson);
        }

        private void addNewIngOtro(string fechaPago, string formaPago, string descriMora, double mora, string ncf, int numfin, string cLIENTE, int nUMREC, string status, int IdIngreso = 50)
        {
            INGOTRO iNGOTRO = new INGOTRO()
            {
                ING_CODING = IdIngreso,
                ING_FECHA = fechaPago,
                ING_FORMPG = formaPago,
                ING_DESCRI = descriMora,
                ING_MONTOT = mora,
                ING_NCF = ncf,
                ING_NUMFIN = numfin,
                ING_NOMBRE = cLIENTE,
                ING_NUMREC = nUMREC,
                ING_STATUS = status,
                ING_REGIST = ""
            };
            ListINGOtros.Add(iNGOTRO);
        }

        private void addNewAboCargo(OTROCARG otroCargo, string abono, DateTime fechaPago)
        {
            string fecha = fechaPago.ToString(formatoFecha);
            ABOOCARG aboCarg = new ABOOCARG()
            {
                ABO_NUMFIN = otroCargo.CAR_NUMFIN,
                ABO_MONTOT = abono,
                ABO_CODCAR = otroCargo.id.ToString(),
                ABO_SECCAR = otroCargo.CAR_SECU,
                ABO_FECHA = fecha,
                ABO_NUMABO = MaxNumAbo(otroCargo.CAR_NUMFIN)
            };
            ListAboCarg.Add(aboCarg);
        }

        private void addNewOtroCarg(cl_cobro Cobro, DateTime FechaPago)
        {

            string finID = Cobro.numfin.ToString();

            OTROCARG iNGOTRO = new OTROCARG()
            {
                CAR_NUMFIN = finID,
                CAR_CODCAR = ((int)typeOTRO.MORA).ToString(),
                CAR_FECHAR = FechaPago.ToString(formatoFecha),
                CAR_MONTOT = Cobro.otroCargo.ToString(),
                CAR_NUMREC = null,
                CAR_STATUS = ((int)Status.NUEVO).ToString(),
                CAR_FECHAP = null,
                CAR_SECU = MaxCarSecun(finID)

            };
            ListOtroCarg.Add(iNGOTRO);
        }


        private async Task<int> MaxABOCUOTA(string numCuota, int numfin)
        {
            int Id_Abono = 0;
            var abo = await _db.ABOCUOTA.Where(x => x.ABO_NUMFAC == numfin.ToString()).ToListAsync();
            //var abo2 = db.ABOCUOTA.ToList();
            if (abo.Any())
                int.TryParse(abo.Max(x => x.ABO_NUMABO), out Id_Abono);

            return Id_Abono;
        }

        private async Task<(int numRec, string ncf)> MaxiNGCUOTARef()
        {
            (int numRec, string ncf) result = (0, string.Empty);
            var IngCuota = await _db.INGCUOTA.OrderByDescending(x => x.ING_NUMREC).Take(1).ToListAsync();
            var ingOtro = await _db.INGOTRO.OrderByDescending(x => x.ING_NUMREC).Take(1).ToListAsync();
            int idNcf = 0;

            if (IngCuota.Any())
            {
                int maxOtro = 0, maxCuota = 0;
                if (ingOtro.Any())
                {
                    maxOtro = ingOtro.FirstOrDefault().ING_NUMREC.Value + 1;
                }
                maxCuota = IngCuota.FirstOrDefault().ING_NUMREC + 1;
                result.numRec = maxCuota >= maxOtro ? maxCuota : maxOtro;
                result.ncf = IngCuota.Max(x => x.ING_NCF);
                int startIndex = result.ncf.IndexOf("00");
                idNcf = int.Parse(result.ncf.Substring(startIndex, 8));
                idNcf++;
                result.ncf = string.Format("B01{0}", idNcf.ToString().PadLeft(8, '0'));
            }
            else
            {
                result.numRec = 1;
                result.ncf = string.Format("B01{0}", idNcf.ToString().PadLeft(8, '0'));
            }
            return result;
        }

        public ActionResult PrintReport(int ReciboID)
        {
            message mensajeReturn = new message();
            mensajeReturn.Is_Success = true;

            try
            {
                string intialPath = Server.MapPath(Parameters.rutaReporte);


                if (mensajeReturn.Is_Success)
                {
                    List<Parameters> prt = new List<Parameters>()
                    {
                        new Parameters(){ParameterName="ReciboID",ParameterValue=ReciboID }
                    };
                    Parameters.guardarReporte(Response, ReportName.ReciboIngreso, prt, intialPath);
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
                string ruta = Server.MapPath(string.Format("{0}/{1}.pdf", Parameters.rutaReporte, ReportName.ReciboIngreso));

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