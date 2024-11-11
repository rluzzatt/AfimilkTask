using JobScheduler.Dto;
using JobScheduler.Models;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;

namespace JobSchedulerTests.SystemTests
{
    [TestClass]
    public class JobsControllerTests
    {
        private const string JobSchedulerProccessName = "JobScheduler.exe";
        private Process _webAppProcess;
        private HttpClient _client;
        private string _baseUrl = "http://localhost:5268"; // The URL where the app is hosted locally

        [TestInitialize]
        public async Task Setup()
        {
            KillJobSchedulerProcesses();

            _webAppProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = JobSchedulerProccessName, 
                    WorkingDirectory = Directory.GetCurrentDirectory(), 
                    UseShellExecute = true,
                    CreateNoWindow = true
                }
            };

            _webAppProcess.Start();

            // Give the server time to start up
            await Task.Delay(2000); 

            // Create an HttpClient to make requests to the server
            _client = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };
        }

        [TestMethod]
        public async Task RegisterJob()
        {
            var now = DateTime.Now.TimeOfDay;

            var registerJobDto = new RegisterJobDto
            {
                Name = "SystemRegisterTest" + now,
                ExecutionTime = now,
                MaxOccurrences = 1,
                ScriptCode = "Console.WriteLine(\"RegisterJob - test job\");"
            };

            var content = new StringContent(JsonSerializer.Serialize(registerJobDto), Encoding.UTF8, "application/json");

            //register job to start execute
            var response = await _client.PostAsync("/api/jobs/register", content);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK, $"response was not ok. recived code: {response.StatusCode}");

            await Task.Delay(1000); // allow job execution time

            var getResponse = await _client.GetAsync("/api/jobs/all");
            var responseString = await getResponse.Content.ReadAsStringAsync();
            var jobs = JsonSerializer.Deserialize<List<Job>>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var executedJob = jobs.FirstOrDefault(x => x.Name == registerJobDto.Name);


            Assert.IsNotNull(executedJob.OccurrencesExecuted > 0,"Expected job to have been executed");
        }

        [TestMethod]
        public async Task GetJobs()
        {
            var now = DateTime.Now.TimeOfDay;

            var registerJobDto = new RegisterJobDto
            {
                Name = "SystemGetJobsTest" + now,
                ExecutionTime = now,
                MaxOccurrences = 1,
                ScriptCode = "Console.WriteLine(\"GetJobs - test job\");"
            };

            var content = new StringContent(JsonSerializer.Serialize(registerJobDto), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/jobs/register", content);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

            var getResponse = await _client.GetAsync("/api/jobs/all");
            Assert.AreEqual(getResponse.StatusCode, HttpStatusCode.OK);

            // Read the response content as string
            var responseString = await getResponse.Content.ReadAsStringAsync();
            var jobs = JsonSerializer.Deserialize<List<Job>>(responseString,new JsonSerializerOptions { PropertyNameCaseInsensitive = true});

            Assert.IsTrue(jobs.Any(), "The job list should contain at least one job.");

            var registeredJob = jobs.FirstOrDefault(j => j.Name == registerJobDto.Name);
            Assert.IsNotNull(registeredJob, "The expected job was not found in the list of jobs.");
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Stop the web application
            if (!_webAppProcess.HasExited)
            {
                _webAppProcess.Kill();
            }

            _client.Dispose();
        }

        private static void KillJobSchedulerProcesses()
        {
            try
            {
                var processes = Process.GetProcessesByName("JobScheduler");

                foreach (var process in processes)
                {
                    process.Kill();
                    process.WaitForExit(); 
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while killing processes: {ex.Message}");
            }
        }
    }
}
