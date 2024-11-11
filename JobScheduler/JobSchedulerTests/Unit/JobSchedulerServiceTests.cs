using JobScheduler.Models;
using JobScheduler.Repository;
using JobScheduler.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JobSchedulerTests.Unit
{
    [TestClass]
    public class JobSchedulerServiceTests
    {
        private JobSchedulerService _jobSchedulerService;
        private IJobRepository _jobRepository;

        [TestInitialize]
        public void Setup()
        {
            // Set up dependency injection container
            var serviceProvider = new ServiceCollection()
                .AddLogging() // Adds logging services
                .AddSingleton<IJobRepository, JobRepository>() // Register actual JobRepository
                .AddSingleton<JobSchedulerService>() // Register JobSchedulerService
                .BuildServiceProvider();

            _jobRepository = serviceProvider.GetService<IJobRepository>()!;
            _jobSchedulerService = serviceProvider.GetService<JobSchedulerService>()!;
        }

        [TestMethod]
        public async Task ExecuteJob()
        {
            await _jobRepository.DeleteAllJobsAsync();

            var job = new Job
            {
                Name = "ExecutionTestJob",
                ScriptCode = "System.Console.WriteLine(\"Running Job...\");",
                ExecutionTime = DateTime.Now.AddSeconds(-1), // Execute immediately
                MaxOccurrences = 1,
                OccurrencesExecuted = 0,
            };

            await _jobRepository.SaveJobsAsync(new List<Job> { job });

            var cts = new CancellationTokenSource();
            var executeTask = _jobSchedulerService.StartAsync(cts.Token);

            // Wait to allow job execution
            await Task.Delay(TimeSpan.FromSeconds(1));

            cts.Cancel(); // Stop the service after waiting
            await executeTask; // Await the completion of the JobSchedulerService

            var jobs = await _jobRepository.LoadJobsAsync();

            var updatedJob = jobs.FirstOrDefault();

            Assert.IsTrue(updatedJob.OccurrencesExecuted > 0,
                "Job repository should reflect execution.");

            Assert.IsTrue(updatedJob.IsCompleted, 
                "Job was expected to compleate execution");
        }
    }
}
