using SistemaImbrino.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SistemaImbrino.Controllers
{
    public static class BaseExtetionController
    {
        public static bool GuardarLog(this TransactionLogs log)
        {
            bool isSaved = false;
            try
            {
                BaseController.db.TransactionLogs.Add(log);
                BaseController.db.SaveChanges();
            }
            catch (Exception)
            {
            }
            return isSaved;
        }

        public static bool GuardarLog(this List<TransactionLogs> logs)
        {
            bool isSaved = false;
            try
            {
                BaseController.db.TransactionLogs.AddRange(logs);
                BaseController.db.SaveChanges();
            }
            catch (Exception)
            {
            }
            return isSaved;
        }
    }
}