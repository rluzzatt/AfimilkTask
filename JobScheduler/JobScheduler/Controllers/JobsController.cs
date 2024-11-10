using JobScheduler.Models;
using JobScheduler.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JobScheduler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IJobRepository _jobRepository;
        public JobsController(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }
        [HttpPost]
        public async Task<IActionResult> RegisterJob([FromBody] Job job)
        {
            await _jobRepository.SaveJobsAsync(new List<Job> { job });

            return Ok("Job registered successfully.");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobs()
        {
            var jobs = await _jobRepository.LoadJobsAsync();

            return Ok(jobs);
        }
    }
}
