
using Microsoft.CodeAnalysis.CSharp.Scripting;
using JobScheduler.Models;
using JobScheduler.Repository;
using Microsoft.CodeAnalysis.Scripting;

namespace JobScheduler.Services
{
    public class JobSchedulerService : BackgroundService
    {
        private static ScriptOptions scriptOptions = ScriptOptions.Default
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
            _jobs = await _jobRepository.LoadJobsAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                TimeSpan currentTime = DateTime.Now.TimeOfDay;

                foreach (var job in _jobs.Where(j => !j.IsCompleted && j.ExecutionTime <= currentTime).ToList())
                {
                    //no reason to await for job to compleate execution
                    _ = ExecuteJobAsync(job, stoppingToken);
                }

                // Wait for 2 seconds before checking again
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }

        private async Task ExecuteJobAsync(Job job, CancellationToken cancellationToken)
        {
            try
            {
                switch (job.JobType)
                {
                    case JobType.CSharpScript:
                
                        await CSharpScript.EvaluateAsync(job.ScriptCode, scriptOptions/*cannot pass cancelation tolken here*/);
                        break;

                    default:
                        throw new NotSupportedException($"job type {job.JobType} is not yet supported");

                }

                job.OccurrencesExecuted++;

                // Update the job in the repository to reflect the execution
                await _jobRepository.UpdateJobAsync(job);

                _logger.LogInformation($"Executed job '{job.Name}' successfully. " +
                    $"({job.OccurrencesExecuted} out of {job.MaxOccurrences} iterations)");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to execute job '{job.Name}': {ex.Message}");
            }
        }

        public async Task RegisterJob(Job job)
        {
            _jobs.Add(job);
            await _jobRepository.SaveJobsAsync(_jobs);
        }

        public IReadOnlyList<Job> GetJobs()
        {
            return _jobs.AsReadOnly();
        }
    }

}
