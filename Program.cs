using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadImpact
{
    class Program
    {
        static void Main(string[] args)
        {
            var api = new RestApi("0dfb7b336a3d1ed03d16f9e00ab0791356814de2317f46498d12434a3a4c82f9");
            var start = DateTime.Now;
            Console.WriteLine("Starting to get data at {0}", start);
            api.StoreResultsInFile(@"C:\EWM\TestResults.csv");
            Console.WriteLine("\n\nCompleted at {0} duration of {1}", DateTime.Now, DateTime.Now.Subtract(start));
            Console.ReadKey();
        }
    }
}
