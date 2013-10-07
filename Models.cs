using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadImpact
{
    public class Test
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Public_Url { get; set; }
        public string Status_Text { get; set; }
        public int Status { get; set; }
        public DateTime Started { get; set; }
        public DateTime Ended { get; set; }
        public string Url { get; set; }

        public override string ToString()
        {
            return string.Format("Id: {0} Title {1} Started {2} Ended {3} Status: {4}", Id, Title, Started, Ended, Status_Text);
        }
    }

    public class UserScenario
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Load_Script { get; set; }
        public string Script_Type { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

    }

    public class TestItem
    {
        public string TestType { get; set; }
        public string TestSize { get; set; }
        public string TestRange { get; set; }
        public DateTime Timestamp { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public string Type { get; set; }

        public string ToCsvString()
        {
            return string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}",
                TestType, TestSize, TestRange, Timestamp.ToString("G"), Name, Value, Type);
        }

        public static string ToCsvTitle()
        {
            return "TestType,TestSize,TestRange,Timestamp,Name,Value,Type";
        }

    }
}
