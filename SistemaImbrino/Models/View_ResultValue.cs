using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaImbrino.Models
{
    public class View_ResultValue<T> where T : class
    {
        public IQueryable<T> data { get; set; }
        public message message { get; set; }

    }
}