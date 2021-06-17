using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaImbrino.Models
{
    public class View_consultaFinanciamientos
    {
        public int FinID { get; set; }
        public string Cliente { get; set; }
        public string Fecha { get; set; }      
        public string Estatus { get; set; }
        public string Descripcion { get; set; }
        public decimal BalanceGeneral { get; set; }
        public consultaFinanciamientosDetalle detail { get; set; }
    }

    public class consultaFinanciamientosDetalle
    {        
        public double Monto { get; set; }       
        public int Cuotas { get; set; }        
        public decimal Capital { get; set; }
        public decimal Interes { get; set; }
        public decimal Balance { get; set; }
        public string Tipo { get; set; }
    }
}