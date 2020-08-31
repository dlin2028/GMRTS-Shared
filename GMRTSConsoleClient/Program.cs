using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GMRTSConsoleClient
{
    class Program
    {
        static event Action a = null;
        static void Main(string[] args)
        {
            a += Program_a;
            a?.Invoke();

            Guid guid = Guid.NewGuid();
            ;
        }

        private static void Program_a()
        {
            Console.WriteLine("hi");
        }
    }
}
