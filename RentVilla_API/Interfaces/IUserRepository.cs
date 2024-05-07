using RentVilla_API.DTOs;
using RentVilla_API.Entities;
using RentVilla_API.Helpers;
using System.Text.Json;

namespace RentVilla_API.Interfaces
{
    public interface IUserRepository
    {
        void update (AppUser user);

        Task<bool> SaveAllAsync();

        Task<IEnumerable<AppUserDTO>> GetAllAppUsersDTOForSearchAsync(string searchByUserNameOrEmail);

        Task<Pagination<AppUserDTO>> GetPagedAppUsersDTOAsync(int pageSize, int pageNumber);

        Task<AppUser> GetUserByIdAsync (int id);


        Task<AppUser> UpdateUserAsync(AppUser entity);

        //PARTIAL UPDATE
        Task<AppUserAddress> UpdateUserAddressAsync(AppUserAddress appUserAddress);

        Task<AppUser> RemoveUserAsync(AppUser entity);

    }
}
