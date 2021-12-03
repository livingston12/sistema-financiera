using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaImbrino.Models
{

    public class View_CuadreCajaGeneral
    {
        public IEnumerable<View_CuadreCaja> Detalle { get; set; }
        public View_CuadreCajaResumen Resumen { get; set; }

        public bool ExisteDetalle
        {
            get { return Detalle.FirstOrDefault() != null; }
        }
    }
    public class View_rptNCFAgrupado
    {
        public string Ids { get; set; }

        public int Id
        {
            get {
                return getId(FormaPago, NCFDetalle.ING_MONTOT, NCFDetalle.OTRO);
            }
        }
        public string FormaPago { get; set; }
        public string Tipo
        {
            get
            {
                return getTipo(Id).tipo;
            }
        }
        public string TipoTexto
        {
            get
            {
                return getTipo(Id).tipoTexto;
            }
        }
        public vw_CuadreCaja NCFDetalle { get; set; }

        private int getId(string FormaPago, decimal? montoT,  decimal? otro = null)
        {
            string ids = "1,2";
            if (!string.IsNullOrWhiteSpace(Ids))
            {
                ids = Ids;
            }
            else if (montoT == 0 && otro > 0)
            {
                ids = "3,4";
            }
            List<(int id, List<string> values)> listFormasPago = new List<(int id, List<string> values)>();
            var list = ids.Split(',').ToArray();
            int id = 1;
            
            if (list.Length > 1)
            {
                int.TryParse(list[0], out int idEfectivo);
                int.TryParse(list[1], out int idDeposito);
                (int id, List<string> values) formaPagoEfectivo = (idEfectivo, new List<string> { "E", "C" });
                (int id, List<string> values) formaPagoDeposito = (idDeposito, new List<string> { "D", "T" });
                listFormasPago.Add(formaPagoEfectivo);
                listFormasPago.Add(formaPagoDeposito);
                id = listFormasPago
                        .Where(x => x.values.Contains(FormaPago))
                        .FirstOrDefault().id;
                var tipo = getTipo(id);

            }
            return id;
        }

        private (string tipo, string tipoTexto) getTipo(int id)
        {
            (string tipo, string tipoTexto) result = (string.Empty, string.Empty);
            switch (id)
            {
                case 1:
                    result = ("CUOTAS_EC", "INGRESO POR CUOTA - EF y CHQ");
                    break;
                case 2:
                    result = ("CUOTAS_DT", "INGRESO POR CUOTA - DEP y TR");
                    break;
                case 3:
                    result = ("OTROS_EC", "INGRESO POR OTROS CARGOS - EF y CHQ");
                    break;
                case 4:
                    result = ("OTROS_DT", "INGRESO POR OTROS CARGOS - DEP y TR");
                    break;
                case 5:
                    result = ("SALIDAS", "SALIDAS");
                    break;
                default:
                    break;
            }
            return result;
        }
    }
    public class View_CuadreCajaLista
    {
        public IEnumerable<View_CuadreCaja> CUOTAS_EC { get; set; }
        public IEnumerable<View_CuadreCaja> CUOTAS_DT { get; set; }
        public IEnumerable<View_CuadreCaja> OTROS_EC { get; set; }
        public IEnumerable<View_CuadreCaja> OTROS_DT { get; set; }
        public IEnumerable<View_CuadreCaja> SALIDAS { get; set; }

    }

    public class View_CuadreCaja
    {
        public int ID { get; set; }
        public int KeyID { get; set; }
        public string Tipo { get; set; }
        public string TipoTexto { get; set; }
        public IEnumerable<View_CuadreCajaDetalle> Detalle  { get; set; }
        public decimal? TotalTipo { get; set; }
        public decimal? TotalTipoAnterior { get; set; }
    }

    public class View_CuadreCajaDetalle
    {
        public long keyId { get; set; }
        public string Cliente { get; set; }
        public string Recibo { get; set; }
        public string Descripcion { get; set; }
        public decimal? MontoTotal { get; set; }
        public decimal? MontoCapital { get; set; }
        public decimal? MontoInteres { get; set; }
        public DateTime? Fecha { get; set; }
        public string FechaTexto { get; set; }
    }
    public class View_CuadreCajaReport
    {
        public int ID { get; set; }
        public string Tipo { get; set; }
        public string TextoTipo { get; set; }
        public string Cliente { get; set; }
        public string Recibo { get; set; }
        public string Descripcion { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal MontoCapital { get; set; }
        public decimal MontoInteres { get; set; }
        public DateTime Fecha { get; set; }
        public string FechaTexto { get; set; }
    }

    public class View_CuadreCajaResumen
    {
        private decimal? _TotalCapital;

        public decimal? TotalCapital
        {
            get { return  _TotalCapital == null ? 0 : _TotalCapital; }
            set { _TotalCapital = value; }
        }
        private decimal? _TotalInteres;
        public decimal? TotalInteres
        {
            get { return _TotalInteres == null ? 0 : _TotalInteres; }
            set { _TotalInteres = value; }
        }
        private decimal? _IngresosNoNetos;
        public decimal? IngresosNoNetos
        {
            get { return _IngresosNoNetos == null ? 0 : _IngresosNoNetos; }
            set { _IngresosNoNetos = value; }
        }
        private decimal? _OtrosIngresosNetos;
        public decimal? OtrosIngresosNetos
        {
            get { return _OtrosIngresosNetos == null ? 0 : _OtrosIngresosNetos; }
            set { _OtrosIngresosNetos = value; }
        }
        private decimal? _DepositoTranferencia;
        public decimal? DepositoTranferencia
        {
            get { return _DepositoTranferencia == null ? 0 : _DepositoTranferencia; }
            set { _DepositoTranferencia = value; }
        }
       
        public decimal? TotalIngresado
        {
            get
            { return 
                    TotalCapital 
                    + TotalInteres 
                    + IngresosNoNetos
                    + OtrosIngresosNetos;
            }            
        }

        public decimal? TotalIngresosNeto
        {
            get
            {
                return
                      TotalInteres
                      + OtrosIngresosNetos;
            }
        }

    }
}