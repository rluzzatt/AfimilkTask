using JobScheduler.Models;

namespace JobScheduler.Dto
{
    public class RegisterJobDto
    {
        public string Name { get; set; }
        public DateTime ExecutionTime { get; set; }
        public int? MaxOccurrences { get; set; }
        public string ScriptCode { get; set; }
    }
}
