namespace JobScheduler.Models
{
    public class Job
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public DateTime ExecutionTime { get; set; }
        public int? MaxOccurrences { get; set; }
        public int OccurrencesExecuted { get; set; } = 1;
        public bool IsCompleted => MaxOccurrences.HasValue && OccurrencesExecuted >= MaxOccurrences;
        public string ScriptCode { get; set; }
        public JobType JobType => JobType.CSharpScript;//currentlly not editable
    }
}
