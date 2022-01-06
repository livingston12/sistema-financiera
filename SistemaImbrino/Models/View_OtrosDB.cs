using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaImbrino.Models
{
    public class View_OtrosDB
    {
        public int ID { get; set; }
        public int BANCO { get; set; }
        public int CUENTA_BANCARIA { get; set; }
        public int TIPO_ENTRADA { get; set; }
        public int TIPO_DEBITO { get; set; }
        public decimal MONTO { get; set; }
        public string FECHA { get; set; }
        public string CONCEPTO { get; set; }
        public bool ACTIVO { get; set; }
        public int STATUS { get; set; }
        public int NUM_FIN { get; set; }
        public int NUM_REC { get; set; }
        public bool VALIDADO { get; set; }
        public bool CERRADO { get; set; }
    }
}