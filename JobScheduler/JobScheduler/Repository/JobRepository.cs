namespace JobScheduler.Repository
{
    using JobScheduler.Models;
    using System.Text.Json;
    using System.Threading;

    public class JobRepository : IJobRepository
    {
        private readonly string _filePath = "c:\\temp\\jobs.json";
        private readonly ILogger<JobRepository> _logger;

        private readonly SemaphoreSlim _readSemaphore = new SemaphoreSlim(1, 1); 
        private readonly SemaphoreSlim _writeSemaphore = new SemaphoreSlim(1, 1); 

        public JobRepository(ILogger<JobRepository> logger)
        {
            _logger = logger;
        }

        public async Task DeleteAllJobsAsync()
        {
            await _writeSemaphore.WaitAsync(); 
            try
            {
                if (File.Exists(_filePath))
                {
                    File.Delete(_filePath);
                }
            }
            finally
            {
                _writeSemaphore.Release(); 
            }
        }

        public async Task<List<Job>> LoadJobsAsync()
        {
            await _readSemaphore.WaitAsync(); 
            try
            {
                if (!File.Exists(_filePath))
                {
                    _logger.LogInformation("No jobs file found at {FilePath}, returning empty list.", _filePath);
                    return new List<Job>(); 
                }

                try
                {
                    var json = await File.ReadAllTextAsync(_filePath);
                    if (string.IsNullOrEmpty(json)) return new List<Job>();

                    var jobs = JsonSerializer.Deserialize<List<Job>>(json) ?? new List<Job>();

                    return jobs;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error deserializing jobs from {FilePath}. Returning empty list.", _filePath);
                    return new List<Job>(); 
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred while loading jobs from {FilePath}. Returning empty list.", _filePath);
                    return new List<Job>(); 
                }
            }
            finally
            {
                _readSemaphore.Release(); 
            }
        }

        public async Task SaveJobsAsync(List<Job> jobs)
        {
            await _writeSemaphore.WaitAsync(); 
            try
            {
                // Ensure the directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath));

                // Serialize the combined list back to JSON and write it to the file
                var json = JsonSerializer.Serialize(jobs, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_filePath, json);
            }
            finally
            {
                _writeSemaphore.Release(); 
            }
        }

        public async Task UpdateJobAsync(Job job)
        {
            await _writeSemaphore.WaitAsync(); 
            try
            {
                var jobs = await LoadJobsAsync();

                var index = jobs.FindIndex(j => j.Id == job.Id);

                if (index >= 0)
                {
                    jobs[index] = job;

                    // Serialize the updated list and write it back to the file
                    var json = JsonSerializer.Serialize(jobs, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(_filePath, json);
                }
                else
                {
                    // Optionally handle the case where the job wasn't found
                    throw new Exception($"Job with Id {job.Id} not found.");
                }
            }
            finally
            {
                _writeSemaphore.Release(); 
            }
        }
    }
}
