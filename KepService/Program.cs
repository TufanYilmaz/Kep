using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KepService
{
    class Program
    {
        static void Main(string[] args)
        {
            Services.JobService();
            Services.SenderStart();
            Services.StatusCheckStart();
            do
            {
                Console.ReadKey();
            } while (true);
        }
    }
}
