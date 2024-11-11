using AutoMapper;
using JobScheduler.Dto;
using JobScheduler.Models;
using JobScheduler.Repository;
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
        private readonly IJobRepository _jobRepository;//for demo api usage only

        public JobsController(IMapper mapper, JobSchedulerService jobSchedulerService, IJobRepository jobRepository, ILogger<JobsController> logger)
        {
            _mapper = mapper;
            _jobSchedulerService = jobSchedulerService;
            _jobRepository = jobRepository;
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
        public async Task<ActionResult<string>> Demo()
        {
            Console.WriteLine();
            Console.WriteLine("----------->   Demo Starting <------------");
            Console.WriteLine("Jobs will be scheduled as follows:");
            Console.WriteLine("1. Job 1 will run 3 seconds from now and print a message. It will run 2 times.");
            Console.WriteLine("2. Job 2 will run 8 seconds from now and print a message. It will run 1 time.");
            Console.WriteLine("3. Job 3 will run 11 seconds from now and print a message, then run 5 times.");
            Console.WriteLine("Each job will execute at the scheduled time based on the MaxOccurrences.");
            Console.WriteLine();

            await _jobRepository.DeleteAllJobsAsync();

            var now = DateTime.Now.TimeOfDay;

            var job1 = new Job
            {
                Name = "Demo Job 1",
                ExecutionTime = now.Add(TimeSpan.FromSeconds(3)),
                ScriptCode = "Console.WriteLine(\"[Demo Job 1] Running job scheduled 3 seconds from start time.\");",
                MaxOccurrences = 2
            };

            var job2 = new Job
            {
                Name = "Demo Job 2",
                ExecutionTime = now.Add(TimeSpan.FromSeconds(8)),
                ScriptCode = "Console.WriteLine(\"[Demo Job 2] Running job scheduled 8 seconds from start time.\");",
                MaxOccurrences = 1
            };

            var job3 = new Job
            {
                Name = "Demo Job 3",
                ExecutionTime = now.Add(TimeSpan.FromSeconds(11)),
                ScriptCode = "Console.WriteLine(\"[Demo Job 3] Running job scheduled 11 seconds from start time.\");",
                MaxOccurrences = 5 // Will run twice, demonstrating multiple occurrences
            };

            // Register the jobs in the scheduler
            await _jobSchedulerService.RegisterJob(job1);
            await _jobSchedulerService.RegisterJob(job2);
            await _jobSchedulerService.RegisterJob(job3);

            return Ok("Job scheduler demo started. Check the server console for job outputs.");
        }

    }
}
