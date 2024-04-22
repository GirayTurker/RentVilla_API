using AutoMapper;
using RentVilla_API.DTOs;
using RentVilla_API.Entities;

namespace RentVilla_API.Helpers
{
    public class AutoMapperProfiles:Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, AppUserDTO>().ReverseMap();
            CreateMap<RegisterDTO, AppUser>().ReverseMap();
            CreateMap<AppUserAddress, AppUserAddressDTO>().ReverseMap();
        }
    }
}
