using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaImbrino.Models
{
    public class View_ConciliacionBancaria
    {
        public int Id { get; set; }
        public string Fecha { get; set; }
        public DateTime? FechaDt { get; set; }
        public string Transacion { get; set; }
        public string Cuenta { get; set; }
        public string Concepto { get; set; }
        public decimal? Debito { get; set; }
        public decimal? Credito { get; set; }
        public decimal? BalanceGeneral { get; set; }
        public bool? Validado { get; set; }
        public string Tipo { get; set; }
        public bool? Cerrado { get; set; }

    }
}