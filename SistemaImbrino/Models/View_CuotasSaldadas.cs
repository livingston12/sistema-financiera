using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaImbrino.Models
{
    public class View_Cuotas
    {
        public int NumFin { get; set; }
        public int NumCuo { get; set; }
        public int NumRec { get; set; }
        public Nullable<double> MontoTotal { get; set; }
        public Nullable<double> MontoCapital { get; set; }
        public Nullable<double> MontoInteres { get; set; }
        public string FechaVencimiento { get; set; }
        public string FechaPago { get; set; }
        public string Status { get; set; }

    }
}