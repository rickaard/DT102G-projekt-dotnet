using AutoMapper;
using CourseProject.Models;

namespace CourseProject.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, SimpleUser>();
            CreateMap<Register, User>();
            // CreateMap<UpdateModel, User>();
        }
    }
}