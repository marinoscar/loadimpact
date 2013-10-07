using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;
using Newtonsoft.Json;

namespace LoadImpact
{
    public class RestApi
    {

        private readonly string _token;
        private readonly RestClient _restClient;
        private int _requestCount;
        private const int Limit = 100;
        private DateTime _firstRequest;

        public RestApi(string apiToken)
        {
            _token = apiToken;
            _restClient = new RestClient("https://api.loadimpact.com/v2")
                {
                    Authenticator = new HttpBasicAuthenticator(_token, string.Empty)
                };
        }


        public IEnumerable<TestItem> GetAllTestResults()
        {
            var userScenarios = GetUserScenarios();
            var tests = GetTests().Where(i => i.Status == 3).ToList();
            var result = new List<TestItem>();
            var total = tests.Count;
            var progress = 1;
            foreach (var test in tests)
            {
                result.AddRange(GetTestResults(test, userScenarios));
                ShowProgressInConsole(total, progress, "Api");
                progress++;
            }
            return result;
        }

        protected void ShowProgressInConsole(int totalCycles, int progress, string title)
        {
            var percentage = (((double)progress / (double)totalCycles) * 100).ToString("N2");
            Console.Write("\r{0} Progress: {1}%".PadRight(50), title, percentage);
        }

        public void StoreResultsInFile(string filePath)
        {
            var results = GetAllTestResults().ToList();
            var total = results.Count;
            var progress = 1;
            var stream = new StreamWriter(filePath);
            stream.WriteLine(TestItem.ToCsvTitle());
            foreach (var result in results)
            {
                stream.WriteLine(result.ToCsvString());
                ShowProgressInConsole(total, progress, "File");
                progress++;
            }
            stream.Close();
        }

        public IEnumerable<Test> GetTests()
        {
            var request = new RestRequest("tests", Method.GET);
            var response = ExecuteRequest(request);
            return JsonConvert.DeserializeObject<List<Test>>(response.Content);
        }

        public Test GetTests(int id)
        {
            var request = new RestRequest(string.Format("tests/{0}", id), Method.GET);
            var response = ExecuteRequest(request);
            return JsonConvert.DeserializeObject<Test>(response.Content);
        }

        public IRestResponse ExecuteRequest(IRestRequest request)
        {
            if (_requestCount <= 0)
                _firstRequest = DateTime.Now;
            _requestCount++;
            if (_requestCount >= 99)
            {
                var waitTime = DateTime.Now.Subtract(_firstRequest).TotalSeconds;
                if (waitTime <= 60)
                {
                    var totalTimeToWait = Convert.ToInt32(Math.Ceiling(65 - waitTime));
                    Console.WriteLine("Waiting {0} seconds", totalTimeToWait);
                    Thread.Sleep(totalTimeToWait*1000);
                }
                _requestCount = 0;
            }
            return _restClient.Execute(request);
        }

        public IEnumerable<UserScenario> GetUserScenarios()
        {
            var request = new RestRequest("user-scenarios", Method.GET);
            var response = ExecuteRequest(request);
            return JsonConvert.DeserializeObject<List<UserScenario>>(response.Content);
        }


        public void GetTestResults()
        {
            var userScenarios = GetUserScenarios();
            var tests = GetTests();
            var validTests = tests.Where(i => i.Status == 3).ToList();
            foreach (var test in validTests)
            {
                GetTestResults(test, userScenarios);
            }
        }


        public void GetTestResults(int testId)
        {
            GetTestResults(GetTests(testId), GetUserScenarios());
        }


        public void GetTestResults(Test test)
        {
            GetTestResults(test, GetUserScenarios());
        }

        public IEnumerable<TestItem> GetTestResults(Test test, IEnumerable<UserScenario> userScenarios)
        {
            var result = new List<TestItem>();
            var page = "__li_page_" + GetHash("Dashboard") + ":1:";
            var testScenarios = GetScenarioBasedOnTestTitle(test.Title, userScenarios).ToList();
            //var us = userScenarios.First(i => i.Id == 1505057);
            foreach (var scenario in testScenarios)
            {
                var id = page + scenario.Id.ToString();
                var requestUrl = string.Format("tests/{0}/results?ids={1}", test.Id, id);
                var request = new RestRequest(requestUrl, Method.GET);
                var response = ExecuteRequest(request);
                var json = JsonConvert.DeserializeObject<JObject>(response.Content);
                var items = (JArray)json[id];
                foreach (JObject item in items)
                {
                    result.Add(ParseTestResult(item, test));
                }
            }
            return result;
        }

        public TestItem ParseTestResult(JObject item, Test test)
        {
            var result = new TestItem()
                {
                    TestType = GetTestTypeFromTitle(test.Title),
                    TestSize = GetTestSizeFromTitle(test.Title),
                    TestRange = GetTestRangeFromTitle(test.Title),
                    Name = Convert.ToString(item["name"]),
                    Type = Convert.ToString(item["type"]),
                    Value = Convert.ToDouble(item["avg"]),
                };
            var span = new DateTime(Convert.ToInt64(item["timestamp"]));
            result.Timestamp = new DateTime(test.Started.Year, test.Started.Month, test.Started.Day, span.Hour, span.Minute, span.Second, span.Millisecond);
            return result;
        }

        private string GetTestTypeFromTitle(string title)
        {
            var result = "Standard";
            if (title.ToLower().Contains("cache")) result = "Cache";
            if (title.ToLower().Contains("uq")) result = "Unbounded Query";
            return result;
        }

        private string GetTestSizeFromTitle(string title)
        {
            var result = "30GB";
            if (title.ToLower().Contains("15gb")) result = "15GB";
            if (title.ToLower().Contains("7gb")) result = "7GB";
            if (title.ToLower().Contains("1gb")) result = "1GB";
            return result;
        }

        private string GetTestRangeFromTitle(string title)
        {
            var result = "30d";
            if (title.ToLower().Contains("week")) result = "7d";
            if (title.ToLower().Contains("day")) result = "1d";
            return result;
        }

        public IEnumerable<UserScenario> GetScenarioBasedOnTestTitle(string testTitle, IEnumerable<UserScenario> scenarios)
        {
            IEnumerable<UserScenario> result;
            //Filter by size
            result = scenarios.Where(i => !i.Name.Contains("GB"));

            if (testTitle.Contains("30GB")) result = scenarios.Where(i => !i.Name.Contains("GB"));
            if (testTitle.Contains("15GB")) result = scenarios.Where(i => i.Name.Contains("15GB - "));
            if (testTitle.Contains("1GB")) result = scenarios.Where(i => i.Name.Contains("1GB - "));
            if (testTitle.Contains("7GB")) result = scenarios.Where(i => i.Name.Contains("7GB - "));

            //Filter by time
            if (testTitle.Contains("Month")) result = result.Where(i => i.Name.Contains("Month"));
            if (testTitle.Contains("Week")) result = result.Where(i => i.Name.Contains("Week"));
            if (testTitle.Contains("Day")) result = result.Where(i => i.Name.Contains("Day"));

            //Filter by Dashboard size
            if (testTitle.Contains("(10")) result = result.Where(i => i.Name.Contains("(10"));
            if (testTitle.Contains("(5")) result = result.Where(i => i.Name.Contains("(5"));


            return result;
        }

        private string GetHash(string value)
        {
            // step 1, calculate MD5 hash from input
            var md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(value);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
