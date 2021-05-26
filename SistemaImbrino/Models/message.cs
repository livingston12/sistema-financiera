using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SistemaImbrino.Models
{
    public class message
    {
        public string Message { get; set; }
        public bool Is_Success { get; set; }

        public static message messageErrors(string monto,string mora, string _TotalMonto,string _totalMora,string numCuota) {
            message message = new message();
            string _mensaje = "";
            message.Is_Success = true;

            try
            {
                double _monto = 0, _mora = 0, TotalMonto = 0, totalMora = 0;
                _monto = double.Parse(monto);
                _mora = double.Parse(mora);
                TotalMonto = double.Parse(_TotalMonto);
                totalMora = double.Parse(_totalMora);

                if (_monto == 0 && _mora == 0)
                {
                    _mensaje = "Debe introducir un monto o cuota a pagar";
                    message.Is_Success = false;
                }                
                                 
                else if (_monto > TotalMonto)
                {
                    _mensaje = "El monto no puede exceder al monto total pendiente de pago";
                    message.Is_Success = false;
                }
                               
                //else if (_mora > totalMora)
                //{
                //    _mensaje = "La mora no puede acceder a la mora total pendiente de pago";
                //    message.Is_Success = false;
                //}  

            }
            catch (Exception)
            {
                message.Message = "Error de conversion a numeros Favor introducir valores numericos";
                message.Is_Success = false;

            }

            if (message.Is_Success == false)            
                message.Message = string.Format("Error cuota #{0}: {1}", numCuota, _mensaje);
            
           

            return message;

        }

    }


}