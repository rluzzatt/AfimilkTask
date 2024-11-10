﻿
using Microsoft.CodeAnalysis.CSharp.Scripting;
using JobScheduler.Models;
using JobScheduler.Repository;
using Microsoft.CodeAnalysis.Scripting;

namespace JobScheduler.Services
{
    public class JobSchedulerService : BackgroundService
    {
        private static ScriptOptions scriptOptions = ScriptOptions.Default
            .WithReferences(typeof(JobSchedulerService).Assembly)
            .AddImports("System"); // Add the System namespace to allow 'Console.WriteLine' scripts

        private readonly IJobRepository _jobRepository;
        private readonly ILogger<JobSchedulerService> _logger;

        private List<Job> _jobs = new();

        public JobSchedulerService(IJobRepository jobRepository, ILogger<JobSchedulerService> logger)
        {
            _jobRepository = jobRepository;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Load jobs from the repository on startup
            _jobs = await _jobRepository.LoadJobsAsync();

            // Run a loop to check and execute jobs periodically
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var job in _jobs.Where(j => !j.IsCompleted && j.ExecutionTime <= DateTime.Now))
                {
                    await ExecuteJobAsync(job, stoppingToken);
                }

                // Wait for 1 minute before checking again
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        private async Task ExecuteJobAsync(Job job, CancellationToken cancellationToken)
        {
            try
            {
                // Note: cannot pass cancelation tolken here, 
                await CSharpScript.EvaluateAsync(job.ScriptCode, scriptOptions);
                job.OccurrencesExecuted++;

                // Update the job in the repository to reflect the execution
                await _jobRepository.UpdateJobAsync(job);
                _logger.LogInformation($"Executed job '{job.Name}' successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to execute job '{job.Name}': {ex.Message}");
            }
        }

        // Method to register a new job
        public async Task RegisterJob(Job job)
        {
            _jobs.Add(job);
            await _jobRepository.SaveJobsAsync(_jobs);
        }
    }

}
