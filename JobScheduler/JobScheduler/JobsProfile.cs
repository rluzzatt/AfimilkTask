using AutoMapper;
using JobScheduler.Dto;
using JobScheduler.Models;

namespace JobScheduler
{
    public class JobsProfile : Profile
    {
        public JobsProfile()
        {
            CreateMap<RegisterJobDto, Job>();
        }
    }
}
