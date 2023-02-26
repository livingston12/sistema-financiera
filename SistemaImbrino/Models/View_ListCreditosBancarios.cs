using SistemaImbrino.Controllers;
using System;

namespace SistemaImbrino.Models
{
    public class View_ListCreditosBancarios
    {
        public int ID { get; set; }
        public int ID_BANCO { get; set; }
        public string BANCO
        {
            get { return BaseController.getBanco(ID_BANCO).Result; }
        }
        public int ID_CUENTA_BANCARIA { get; set; }

        public string CUENTA_BANCARIA
        {
            get { return BaseController.getCuentaBancaria(ID_CUENTA_BANCARIA); }
        }
        public string NUMERO_CHEQUE { get; set; }
        public int ID_TIPO_CREDITO { get; set; }
        public string TIPO_CREDITO
        {
            get { return BaseController.getTipoCredito(ID_TIPO_CREDITO); }
        }
        public int ID_TIPO_SALIDA { get; set; }
        public string TIPO_SALIDA
        {
            get { return BaseController.getTipoSalida(ID_TIPO_SALIDA); }
        }
        public decimal MONTO { get; set; }
        public DateTime? FECHAdt { get; set; }
        public string FECHA
        {
            get { return BaseController.returDateFormat(FECHAdt); }
        }
        public int CLIENTE { get; set; }
        public string CONCEPTO { get; set; }
        public string BENEFICIARIO { get; set; }
        public bool Activo { get; set; }
    }

    public class View_fechas
    {
        public string FechaDesde { get; set; }
        public string FechaHasta { get; set; }

        public DateTime? FechaDesdeDt
        {
            get
            {
                try
                {
                    var datePart = FechaDesde.Split('/');
                    DateTime fechaDesde = new DateTime();
                    if (datePart.Length > 1)
                    {
                        string month = datePart[1];
                        if (int.TryParse(month, out int monthNumber))
                        {
                            fechaDesde = DateTime.Parse(FechaDesde);
                        }
                        else
                        {
                            string day = datePart[0];
                            string year = datePart[2];
                            fechaDesde = DateTime
                                .Parse(
                                $"{BaseController.returMonthNumber(month)}/{day}/{year}"
                                );
                        }
                    }
                    else
                    {
                        fechaDesde = DateTime.Parse(FechaDesde);
                    }
                    return fechaDesde;
                }
                catch (Exception)
                {
                    return DateTime.Now;
                }
            }
        }

        public DateTime FechaHastaDt
        {
            get
            {
                try
                {
                    var datePart = FechaHasta.Split('/');
                    DateTime fechaHasta = new DateTime();
                    if (datePart.Length > 1)
                    {
                        string month = datePart[1];
                        if (int.TryParse(month, out int monthNumber))
                        {
                            fechaHasta = DateTime.Parse(FechaDesde);
                        }
                        else
                        {
                            string day = datePart[0];
                            string year = datePart[2];
                            fechaHasta = DateTime
                                .Parse(
                                $"{BaseController.returMonthNumber(month)}/{day}/{year}"
                                );
                        }
                    }
                    else
                    {
                        fechaHasta = DateTime.Parse(FechaHasta);
                    }
                    return fechaHasta;
                }
                catch (Exception)
                {
                    return DateTime.Now;
                }
            }
        }


    }
}