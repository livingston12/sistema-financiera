using SistemaImbrino.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaImbrino.Models
{
    public class View_ListDebitosBancarios
    {
        public int ID { get; set; }
        public int ID_BANCO { get; set; }
        public string BANCO
        {
            get { return BaseController.getBanco(ID_BANCO); }
        }
        public int ID_CUENTA_BANCARIA { get; set; }
        
        public string CUENTA_BANCARIA
        {
            get { return BaseController.getCuentaBancaria(ID_CUENTA_BANCARIA); }
        }
        public int ID_TIPO_ENTRADA { get; set; }
        public string TIPO_ENTRADA
        {
            get { return BaseController.getTipoEntrada(ID_TIPO_ENTRADA); }
        }
        public int ID_TIPO_DEBITO { get; set; }
        public string TIPO_DEBITO
        {
            get { return BaseController.getTipoDebito(ID_TIPO_ENTRADA); }
        }
        public decimal MONTO { get; set; }
        public DateTime? FECHAdt { get; set; }
        public string FECHA
        {
            get { return BaseController.returDateFormat(FECHAdt); }
        }
        public string CONCEPTO { get; set; }
        public bool ACTIVO { get; set; }
    }
}