using JobScheduler.Models;
using JobScheduler.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace JobSchedulerTests.Unit
{
    [TestClass]
    public class RepositoryTests
    {
        private IJobRepository _jobRepository;

        [TestInitialize]
        public void Setup()
        {
            // Use in-memory DI for isolated tests, without affecting real data
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IJobRepository, JobRepository>() // Register repository
                .BuildServiceProvider();

            _jobRepository = serviceProvider.GetService<IJobRepository>()!;
        }

        [TestMethod]
        public async Task SaveJobs()
        {
            // Arrange: Create test jobs
            await _jobRepository.DeleteAllJobsAsync();
            var jobs = GenerateJobs(2, "SaveTest");

            // Act: Save jobs
            await _jobRepository.SaveJobsAsync(jobs);

            // Assert: Verify that the jobs are saved correctly
            var jobsInDatabase = await _jobRepository.LoadJobsAsync();

            Assert.AreEqual(jobs.Count, jobsInDatabase.Count, 
                "The number of saved jobs does not match.");
        }

        [TestMethod]
        public async Task DeleteAllJobs()
        {
            var jobs = GenerateJobs(2, "DeletionTest");
            await _jobRepository.SaveJobsAsync(jobs);

            var jobsInDatabase = await _jobRepository.LoadJobsAsync();

            Assert.IsTrue(jobsInDatabase.Count > 0,
                "Some job definitions are expected before checking deletion");

            await _jobRepository.DeleteAllJobsAsync();

            jobsInDatabase = await _jobRepository.LoadJobsAsync();

            Assert.AreEqual(0, jobsInDatabase.Count, 
                "Expected no jobs in the database after deletion.");
        }

        [TestMethod]
        public async Task UpdateJobs()
        {
            var prefixToUpdate = "Updated";
            await _jobRepository.DeleteAllJobsAsync();

            var jobs = GenerateJobs(2, "Test");
            await _jobRepository.SaveJobsAsync(jobs);

            var jobsToUpdate = await _jobRepository.LoadJobsAsync();

            Assert.IsTrue(jobsToUpdate.Count > 0,
                "Some job definitions are expected before checking update functionality");

            jobsToUpdate.ForEach(job => job.Name = prefixToUpdate + job.Name);

            foreach (var job in jobsToUpdate)
            {
                await _jobRepository.UpdateJobAsync(job);
            }

            //Verify that all job names have been updated correctly
            var jobsInDatabase = await _jobRepository.LoadJobsAsync();

            Assert.IsTrue(jobsInDatabase.All(job => job.Name.StartsWith(prefixToUpdate)),
                $"Expected all jobs to have names starting with '{prefixToUpdate}' after update.");
        }

        private List<Job> GenerateJobs(int count, string namePrefix)
        {
            var jobs = new List<Job>();
            for (int i = 0; i < count; i++)
            {
                jobs.Add(new Job
                {
                    ExecutionTime = DateTime.Now.AddMinutes(i),
                    MaxOccurrences = 1,
                    Name = $"{namePrefix} {i + 1}",
                    ScriptCode = $"Console.WriteLine(\"test job {namePrefix}{i + 1}\")"
                });
            }
            return jobs;
        }
    }
}
