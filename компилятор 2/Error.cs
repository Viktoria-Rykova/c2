using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace компилятор_2
{
    public class Error
    {
        public string Message { get; set; }
        public string Place { get; set; }

        public Error(string message, string place)
        {
            Message = message;
            Place = place;
        }
    }

}
