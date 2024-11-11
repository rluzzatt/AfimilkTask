namespace JobScheduler.Repository
{
    using JobScheduler.Models;
    using System.Text.Json;

    public class JobRepository : IJobRepository
    {
        private readonly string _filePath = "c:\\temp\\jobs.json";
        private readonly ILogger<JobRepository> _logger;

        public JobRepository(ILogger<JobRepository> logger)
        {
            _logger = logger;
        }

        public async Task DeleteAllJobsAsync()
        {
            if (File.Exists(_filePath))
            {
                await Task.Run(() => File.Delete(_filePath)); 
            }
        }

        public async Task<List<Job>> LoadJobsAsync()
        {
            if (!File.Exists(_filePath))
            {
                _logger.LogInformation("No jobs file found at {FilePath}, returning empty list.", _filePath);
                return new List<Job>(); // Return empty list if the file does not exist
            }

            try
            {
                var json = await File.ReadAllTextAsync(_filePath);
                if(string.IsNullOrEmpty(json)) return new List<Job>();  

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


        public async Task SaveJobsAsync(List<Job> jobs)
        {
            // Ensure the directory exists
            Directory.CreateDirectory(path: Path.GetDirectoryName(_filePath));

            // Serialize the combined list back to JSON and write it to the file
            var json = JsonSerializer.Serialize(jobs, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task UpdateJobAsync(Job job)
        {
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
    }

}
