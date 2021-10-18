using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KepService
{
    public static class Logger
    {
        public static void WriteLine(string log, bool status = true)
        {
            if (!status)
            {
                log += "Hata: ";
            }
            Console.WriteLine(DateTime.Now + " " + log);
        }
    }
}
