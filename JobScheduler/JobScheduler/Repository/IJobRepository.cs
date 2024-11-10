using JobScheduler.Models;

namespace JobScheduler.Repository
{
    public interface IJobRepository
    {
        Task<List<Job>> LoadJobsAsync();
        Task SaveJobsAsync(List<Job> jobs);
        Task UpdateJobAsync(Job job);
    }
}
