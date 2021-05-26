using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaImbrino.Models
{
    public class View_cobrosHeader
    {
        public int ClienteId { get; set; }
        public string cliente { get; set; }
        public int FinID { get; set; }
        public decimal montoTotal { get; set; }        
        public decimal capitalTotal { get; set; }
        public decimal interesTotal { get; set; }
        public decimal moraTotal { get; set; }
        public int CountT { get; set; }
        
    }
}