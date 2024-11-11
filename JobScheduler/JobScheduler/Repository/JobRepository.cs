namespace JobScheduler.Repository
{
    using JobScheduler.Models;
    using System.Text.Json;
    using System.Threading;

    public class JobRepository : IJobRepository
    {
        private readonly string _filePath = "c:\\temp\\jobs.json";
        private readonly ILogger<JobRepository> _logger;

        private readonly SemaphoreSlim _readSemaphore = new SemaphoreSlim(1, 1); // Semaphore for read operations
        private readonly SemaphoreSlim _writeSemaphore = new SemaphoreSlim(1, 1); // Semaphore for write operations

        public JobRepository(ILogger<JobRepository> logger)
        {
            _logger = logger;
        }

        public async Task DeleteAllJobsAsync()
        {
            await _writeSemaphore.WaitAsync(); // Lock for writing
            try
            {
                if (File.Exists(_filePath))
                {
                    File.Delete(_filePath);
                }
            }
            finally
            {
                _writeSemaphore.Release(); // Release the write semaphore
            }
        }

        public async Task<List<Job>> LoadJobsAsync()
        {
            await _readSemaphore.WaitAsync(); // Lock for reading
            try
            {
                if (!File.Exists(_filePath))
                {
                    _logger.LogInformation("No jobs file found at {FilePath}, returning empty list.", _filePath);
                    return new List<Job>(); // Return empty list if the file does not exist
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
                    return new List<Job>(); // Return empty list if deserialization fails
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred while loading jobs from {FilePath}. Returning empty list.", _filePath);
                    return new List<Job>(); // Return empty list for unexpected errors
                }
            }
            finally
            {
                _readSemaphore.Release(); // Release the read semaphore
            }
        }

        public async Task SaveJobsAsync(List<Job> jobs)
        {
            await _writeSemaphore.WaitAsync(); // Lock for writing
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
                _writeSemaphore.Release(); // Release the write semaphore
            }
        }

        public async Task UpdateJobAsync(Job job)
        {
            await _writeSemaphore.WaitAsync(); // Lock for writing
            try
            {
                // Load jobs for updating, outside of the write lock
                var jobs = await LoadJobsAsync();

                // Find the index of the job to update
                var index = jobs.FindIndex(j => j.Id == job.Id);

                // If the job exists, replace the old job with the updated one
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
                _writeSemaphore.Release(); // Release the write semaphore
            }
        }
    }
}
