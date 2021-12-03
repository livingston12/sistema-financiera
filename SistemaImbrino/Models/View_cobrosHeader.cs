using System.Collections.Generic;

namespace SistemaImbrino.Models
{
    public class View_cobrosHeader : PageIndex
    {
        public IEnumerable<View_cobrosDetalle> Detalle { get; set; }
    }
    public class View_cobrosDetalle
    {
        public int ClienteId { get; set; }
        public string cliente { get; set; }
        public int FinID { get; set; }
        public decimal montoTotal { get; set; }
        public decimal capitalTotal { get; set; }
        public decimal interesTotal { get; set; }
        public decimal moraTotal { get; set; }
        public decimal otrosTotal { get; set; }
        public int CountT { get; set; }
    }
}