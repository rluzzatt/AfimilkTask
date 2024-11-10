using JobScheduler.Models;
using JobScheduler.Repository;
using Microsoft.AspNetCore.Mvc;

namespace JobScheduler.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        public JobsController()
        {
        }
        [HttpPost]
        public async Task<IActionResult> RegisterJob([FromBody] Job job)
        {
            return Ok("Job registered successfully.");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobs()
        {

            return Ok();
        }
    }
}
