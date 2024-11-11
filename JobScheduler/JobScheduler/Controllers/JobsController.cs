using AutoMapper;
using JobScheduler.Dto;
using JobScheduler.Models;
using JobScheduler.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobScheduler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly JobSchedulerService _jobSchedulerService;
        private readonly ILogger<JobsController> _logger;
        private readonly IMapper _mapper;

        public JobsController(IMapper mapper, JobSchedulerService jobSchedulerService, ILogger<JobsController> logger)
        {
            _mapper = mapper;
            _jobSchedulerService = jobSchedulerService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterJob([FromBody] RegisterJobDto registerJobDto)
        {
            try
            {
                var job = _mapper.Map<Job>(registerJobDto);

                await _jobSchedulerService.RegisterJob(job);
                return Ok("Job registered successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering the job.");
                return StatusCode(500, "An error occurred while registering the job.");
            }
        }

        [HttpGet("all")]
        public ActionResult<List<Job>> GetJobs()
        {
            try
            {
                var jobs = _jobSchedulerService.GetJobs();

                if (jobs == null || !jobs.Any())
                {
                    return NoContent(); // Returns 204 if no jobs are available
                }

                return Ok(jobs);
            }
            catch (Exception ex)
            {
                // Log the exception and return a 500 error
                _logger.LogError(ex, "Error retrieving jobs.");
                return StatusCode(500, "An error occurred while retrieving jobs.");
            }
        }

        [HttpGet("run-demo")]
        public ActionResult<string> Demo()
        {
            return Ok("Job scheduler demo started, open the server console to see jobs executions");

        }

    }
}
