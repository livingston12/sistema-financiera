using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaImbrino.Extentions
{
    public static class dateExtension
    {
        public static bool NotExpirationDate(this DateTime? expirationDate)
        {
            return  expirationDate > DateTime.Today;
        }
    }
}