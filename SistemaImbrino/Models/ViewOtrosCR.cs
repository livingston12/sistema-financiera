using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaImbrino.Models
{
    public class ViewOtrosCR
    {
        public int ID { get; set; }
        public int BANCO { get; set; }
        public int CUENTA_BANCARIA { get; set; }
        public string NUMERO_CHEQUE { get; set; }
        public int TIPO_CREDITO { get; set; }
        public int TIPO_SALIDA { get; set; }
        public decimal MONTO { get; set; }
        public string FECHA { get; set; }
        public int CLIENTE { get; set; }
        public string CONCEPTO { get; set; }
        public string BENEFICIARIO { get; set; }
        public bool Activo { get; set; }
        public bool? VALIDADO { get; set; }
        public bool? CERRADO { get; set; }
    }
}