using Newtonsoft.Json;
using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace SistemaImbrino.Controllers
{
    public class CobrosController : BaseController
    {
        private DB_IMBRINOEntities _db = new DB_IMBRINOEntities();
        private List<INGCUOTA> ListiNGCUOTAs = new List<INGCUOTA>();
        private List<ABOCUOTA> ListaBOCUOTA = new List<ABOCUOTA>();
        private List<INGOTRO> ListINGOtros = new List<INGOTRO>();
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
        public ActionResult Index(string FinID = "", string _Fecha_pago = null)
        {
            ReturnFechapago(_Fecha_pago);

            _FinID = FinID;
            string FinNum = FinID;

            int.TryParse(FinID, out FinIDint);
            getTotalCuotas(FinIDint);

            VW_rptFinAtrasados listAtrasados = db.VW_rptFinAtrasados.Where(x => x.C__FIN == FinIDint).FirstOrDefault();
            if (FinID == "" || listAtrasados == null)
                return RedirectToAction("Index", "CobrosHeader");
            ViewBag.ListAtrasados = listAtrasados;
            List<sp_cuotasVencidas_Result> ListCuotasVencidas = db.sp_cuotasVencidas(Fecha_pago).Where(x => x.C__FIN == FinIDint).ToList(); //db.VW_rptCuotasVencidas.Where(x => x.C__FIN == FinIDint).ToList();

            ViewBag.Mora = String.Format("{0:n}", ListCuotasVencidas.Sum(x => x.mora));
            ViewBag.ListCuotasVencidas = ListCuotasVencidas;
            ViewBag.FechaPago = Fecha_pago.ToString("yyyy-MM-dd");
            return View();
        }

        public void getTotalCuotas(int numFin)
        {
            string fecha_pagado = string.Empty;
            ViewBag.CUOTA_PENDIENTE = db.CUOTA.Where(x => x.CUO_NUMFAC == numFin  && x.CUO_FECHAP == fecha_pagado).Count();
            ViewBag.CUOTA_TOTAL = db.CUOTA.Where(x => x.CUO_NUMFAC == numFin).Count();
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

        public ActionResult _popup_Cobro(string id_numFin, int id_cuota, string tipo, string idIndex, string _Fecha_pago = null)
        {
            ReturnFechapago(_Fecha_pago);
            int.TryParse(id_numFin, out FinIDint);
            ViewBag.tipoCobro = tipo;
            ViewBag.Index = idIndex;
            var model = db.sp_cuotasVencidas(Fecha_pago).Where(x => x.C__FIN == FinIDint && x.NUM_CUOTA == id_cuota).FirstOrDefault();
            return PartialView(model);
        }

        public ActionResult _popup_ProcesarPagoTotal(string _FinID = "", string _Fecha_pago = null)
        {
            ReturnFechapago(_Fecha_pago);
            int.TryParse(_FinID, out FinIDint);
            var cliente = db.sp_cuotasVencidas(Fecha_pago).Where(x => x.C__FIN == FinIDint).FirstOrDefault().CLIENTE;
            View_cobrosHeader model = CobrosHeader_SP(cliente, fecha_pago: Fecha_pago).Where(x => x.cliente == _FinID).FirstOrDefault();
            return PartialView(model);
        }


        public ActionResult _popup_ProcesarPago()
        {
            return View();
        }
        public JsonResult Cobrar(string json, string tipoPago, string _Fecha_pago = null,int aumentoRecibo = 0)
        {
            ReturnFechapago(_Fecha_pago);
            if (db.Database.Connection.State == System.Data.ConnectionState.Open)
                db.Database.Connection.Close();

            var tran = db.Database.BeginTransaction();
            bool error = false;
            try
            {
                View_Cobrar _View_Cobrar = JsonConvert.DeserializeObject<View_Cobrar>(json);
                decimal descuento = 0; int num = 0;
                foreach (var item in _View_Cobrar.Cobros)
                {

                    if (RealizarCobro(item, tipoPago, ref descuento, ref descuento, ref num,aumentoRecibo: aumentoRecibo) == false)
                    {
                        error = true;
                        break;
                    }

                }

                if (error == false)
                {
                    GuardarCuota();
                    tran.Commit();
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

        private void GuardarCuota()
        {
            db.ABOCUOTA.AddRange(ListaBOCUOTA);
            db.INGCUOTA.AddRange(ListiNGCUOTAs);
            db.INGOTRO.AddRange(ListINGOtros);

            db.SaveChanges();
        }

        private bool RealizarCobro(cl_cobro cobro, string tipoPago, ref decimal decuentoMora, ref decimal descuentoInteres, ref int totalDecuotasPagar, bool isOne = true,int aumentoRecibo = 0)
        {

            int numfin = cobro.numfin, idCuota = 0, NUMREC = 0;

            double mora = 0;

            decimal MontoCapital = 0, MontoCapitalOr = 0
                , montoInteres = 0
                , montoPagado = cobro.montoPagado;

            string tipoCobro = ReturnTipoCobro(cobro.tipoCobro).ToString()
            , fecha = Fecha_pago.ToString("dd/MM/yyyy") //DateTime.Now.ToString("dd/MM/yyyy")
            , formaPago = tipoPago
            , descri = ""
            , TotalCuota = ""
            //, NUMREC = ""
            , ncf = ""
            , numCuota = cobro.numCuota
            , descriMora = ""
            , _status = "";



            idCuota = int.Parse(numCuota);

            INGCUOTA iNGCUOTA = new INGCUOTA();
            INGOTRO iNGOTRO;

            //Buscar cuota
            List<sp_cuotasVencidas_Result> ListCuotaVen = db.sp_cuotasVencidas(Fecha_pago).Where(x => x.C__FIN == numfin && x.NUM_CUOTA == idCuota).ToList();
            if (ListCuotaVen.Any() == false)
                return false;
            _status = ListCuotaVen.FirstOrDefault().CUO_STATUS;

            // Asignar valores de la cuota existente
            MontoCapital = ListCuotaVen.FirstOrDefault().CapitalTotal;
            montoInteres = ListCuotaVen.FirstOrDefault().InteresTotal;
            TotalCuota = ListCuotaVen.FirstOrDefault().CUOTA;

            string MontoTotal = (MontoCapital + montoInteres).ToString();
            string MontoTotalDesc = (MontoCapital + montoInteres).ToString("N0");
            Messajson = message.messageErrors(cobro.montoPagado.ToString(), cobro.mora.ToString(), MontoTotal, ListCuotaVen.FirstOrDefault().mora.ToString(), numCuota);

            if (Messajson.Is_Success == false && isOne)
                return false;

            var cuota = db.CUOTA.Where(x => x.CUO_NUMFAC == numfin && x.CUO_NUMCUO == idCuota).FirstOrDefault();
            //montoInteres = montoPagado > MontoCapital  ? MontoCapital - montoPagado : 0;
            string id_tipoCobro = "0";

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


                int Id_Abono = 0;
                MaxABOCUOTA(numCuota, numfin, ref Id_Abono);

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
                MaxiNGCUOTARef(ref NUMREC, ref ncf);
            }
            else if (tipoCobro == TipoCobro.Pago.ToString())
            {
                if (isOne == false)
                {
                    if (descuentoInteres >= montoInteres)
                    {
                        descuentoInteres -= montoInteres;
                        montoInteres = 0;
                    }
                    else
                    {
                        montoInteres -= descuentoInteres;
                        descuentoInteres = 0;
                    }

                    if (decuentoMora >= cobro.mora)
                    {
                        decuentoMora -= cobro.mora;
                        cobro.mora = 0;
                    }
                    else
                    {
                        cobro.mora -= decuentoMora;
                        decuentoMora = 0;
                    }

                    //cobro.mora -= decuentoMora;
                    //cobro.mora = cobro.mora < 0 ? 0 : cobro.mora;

                    MontoTotal = (MontoCapital + montoInteres).ToString();
                    MontoTotalDesc = (MontoCapital + montoInteres).ToString("N0");

                }
                string strAbonado = ((int)Status.ABONO).ToString();
                string strPagado = ((int)Status.PAGADO).ToString();
                string strSaldo = (cuota.CUO_STATUS == strAbonado) ? "COMPLETIVO" : "SALDO";

                descri = string.Format("{2} {0} ${1}", TotalCuota, MontoTotalDesc, strSaldo);
                //descriMora = string.Format("{2} {0} ${1}", TotalCuota, cobro.mora, "MORA");

                id_tipoCobro = ((int)TipoCobro.Pago).ToString();
                NUMREC = 0;
                MaxiNGCUOTARef(ref NUMREC, ref ncf);
            }
            descriMora = string.Format("{2} {0} ${1}", TotalCuota, cobro.mora, "MORA");
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
                        addNewIngOtro(Fecha_pago.ToString("dd/M/yyyy"), formaPago, descriMora, mora, ncf, numfin, ListCuotaVen.FirstOrDefault().CLIENTE, NUMREC, _status);


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
                    ING_FECHA = Fecha_pago.ToString("dd/M/yyyy") //DateTime.Now.ToString("dd/M/yyyy")

                };
                ListiNGCUOTAs.Add(iNGCUOTA);

                if (cobro.mora > 0)
                {


                    double.TryParse(cobro.mora.ToString(), out mora);
                    addNewIngOtro(Fecha_pago.ToString("dd/M/yyyy"), formaPago, descriMora, mora, ncf, numfin, ListCuotaVen.FirstOrDefault().CLIENTE, NUMREC, _status);

                }

            }



            //Actualizar estatus de la cuota             
            cuota.CUO_STAANT = cuota.CUO_STATUS;
            cuota.CUO_STATUS = id_tipoCobro;
            cuota.CUO_NUMREC = NUMREC;
            cuota.CUO_FECHAP = Fecha_pago.ToString("M/dd/yyyy");

            db.Entry(cuota).State = EntityState.Modified;
            //db.SaveChanges();
            return true;
        }

        private void addNewIngOtro(string fechaPago, string formaPago, string descriMora, double mora, string ncf, int numfin, string cLIENTE, int nUMREC, string status)
        {
            INGOTRO iNGOTRO = new INGOTRO()
            {
                ING_CODING = (int)typeOTRO.MORA,
                ING_FECHA = fechaPago, //DateTime.Now.ToString("dd/M/yyyy"),
                ING_FORMPG = formaPago,
                ING_DESCRI = descriMora,
                ING_MONTOT = mora,//cobro.mora,
                ING_NCF = ncf,
                ING_NUMFIN = numfin,
                ING_NOMBRE = cLIENTE,
                ING_NUMREC = nUMREC,
                ING_STATUS = status,
                ING_REGIST = ""
            };
            ListINGOtros.Add(iNGOTRO);
        }

        public JsonResult cobroTotal(string _numFin, decimal descuentoInte, decimal descuentoMora, string tipoPago, string _Fecha_pago = null)
        {
            message Messajson = new message();
            ReturnFechapago(_Fecha_pago);
            var tran = db.Database.BeginTransaction();

            try
            {

                int intFinID = int.Parse(_numFin);
                List<sp_cuotasVencidas_Result> cuotasVenDesc = db.sp_cuotasVencidas(Fecha_pago).Where(x => x.C__FIN == intFinID).OrderByDescending(x => x.NUM_CUOTA).ToList();


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


                    RealizarCobro(cobro, tipoPago, ref descuentoMora, ref descuentoInte, ref totalCuotasPagar, false);
                    //actualizarCuota(cu);
                }



                GuardarCuota();
                tran.Commit();
                Messajson.Message = "Se completo el pago correctamente";
                Messajson.Is_Success = true;
            }
            catch (Exception e)
            {
                Messajson.Message = "Ocurrio un problema con el pago: " + e.Message;
                Messajson.Is_Success = false;
                tran.Rollback();
            }


            return Json(Messajson);


        }

        private void MaxABOCUOTA(string numCuota, int numfin, ref int Id_Abono)
        {

            var abo = db.ABOCUOTA.Where(x => x.ABO_NUMFAC == numfin.ToString()).ToList();
            //var abo2 = db.ABOCUOTA.ToList();
            if (abo.Any())
                int.TryParse(abo.Max(x => x.ABO_NUMABO), out Id_Abono);
            else
                Id_Abono = 0;

        }

        private void MaxiNGCUOTARef(ref int NUMREC, ref string ncf)
        {

            //var IngAbo = db.INGCUOTA.Where(x => x.ING_NCF.StartsWith("B01")).OrderByDescending(x => x.ING_NUMREC).Take(1).ToList();
            var IngAbo = db.INGCUOTA.OrderByDescending(x => x.ING_NUMREC).Take(1).ToList();
            int idNcf = 0, idnumRef = 0;


            if (IngAbo.Any())
            {
                //if(NUMREC != "")
                //{
                //NUMREC = NUMREC != "" ? IngAbo.FirstOrDefault().ING_NUMREC : "";
                NUMREC = IngAbo.FirstOrDefault().ING_NUMREC;
                idnumRef = NUMREC;
                idnumRef++;
                NUMREC = idnumRef;
                ncf = IngAbo.Max(x => x.ING_NCF);
                int startIndex = ncf.IndexOf("00");
                idNcf = int.Parse(ncf.Substring(startIndex, 8));
                idNcf++;
                ncf = string.Format("B01{0}", idNcf.ToString().PadLeft(8, '0'));
                //}


            }
            else
            {
                NUMREC = 1;//NUMREC != "" ? "1" : "";
                ncf = string.Format("B01{0}", idNcf.ToString().PadLeft(8, '0'));
            }

        }
    }
}