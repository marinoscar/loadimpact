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
            var consoleArgs = new Arguments(args);
            var apiKey = "";
            if (!consoleArgs.IsSwitchPresent("/apiKey"))
            {
                Console.WriteLine("Enter your api key");
                apiKey = Console.ReadLine();
            }
            Console.WriteLine("Enter your api key");
            var api = new RestApi(apiKey);
            var start = DateTime.Now;
            Console.WriteLine("Starting to get data at {0}", start);
            api.StoreResultsInFile(@"C:\EWM\TestResults.csv");
            Console.WriteLine("\n\nCompleted at {0} duration of {1}", DateTime.Now, DateTime.Now.Subtract(start));
            Console.ReadKey();
        }
    }
}
