
using System;
using System.ComponentModel;
using System.Web.Mvc;

namespace CustomHtmlHelper.Helpers
{
    public static class HtmlExtensions
    {
        public static MvcHtmlString NumberFormat(this decimal number)
        {
            string html =String.Format("{0:n}", number);
              return new MvcHtmlString(html); 
        }

       
    }
}