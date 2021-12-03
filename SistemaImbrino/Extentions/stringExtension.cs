using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace System
{
    public static class stringExtension
    {
        public static bool isNumber(this string value)
        {
            return int.TryParse(value, out int converter);
        }
        public static string AsPhone(this string value)
        {
            string tel = $"{value.Substring(0,3)}-{value.Substring(2, 3)}-{value.Substring(5, 4)}";
            return tel;
        }
        
    }
}