namespace JobScheduler.Repository
{
    using JobScheduler.Models;
    using System.Text.Json;

    public class JobRepository : IJobRepository
    {
        private readonly string _filePath = "c:\\temp\\jobs.json";

        public async Task<List<Job>> LoadJobsAsync()
        {
            if (!File.Exists(_filePath))
                return new List<Job>();

            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<Job>>(json) ?? new List<Job>();
        }

        public async Task SaveJobsAsync(List<Job> jobs)
        {
            // Ensure the directory exists
            Directory.CreateDirectory(path: Path.GetDirectoryName(_filePath));

            var existingJobs = new List<Job>();

            // Check if the file exists and has content
            if (File.Exists(_filePath) && new FileInfo(_filePath).Length > 0)
            {
                // Read the existing content and deserialize it into a list of jobs
                var existingJson = await File.ReadAllTextAsync(_filePath);
                existingJobs = JsonSerializer.Deserialize<List<Job>>(existingJson) ?? new List<Job>();
            }

            // Add the new jobs to the existing list
            existingJobs.AddRange(jobs);

            // Serialize the combined list back to JSON and write it to the file
            var json = JsonSerializer.Serialize(existingJobs, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }

        public async Task UpdateJobAsync(Job job)
        {
            var jobs = await LoadJobsAsync();
            var index = jobs.FindIndex(j => j.Id == job.Id);

            if (index >= 0)
                jobs[index] = job;

            await SaveJobsAsync(jobs);
        }
    }

}
