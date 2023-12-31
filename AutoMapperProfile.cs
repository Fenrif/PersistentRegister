using AutoMapper;
using PersistentRegister.Dtos.User;
using PersistentRegister.Models;

namespace PersistentRegister
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, GetUserDto>();
            CreateMap<InsertUserDto, User>();
            CreateMap<UpdateUserDto, User>();
            CreateMap<User, UserRegisterInfo>().ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.ID));
        }
    }
}