using RentVilla_API.DTOs;
using RentVilla_API.Entities;
using System.Text.Json;

namespace RentVilla_API.Interfaces
{
    public interface IUserRepository
    {
        void update (AppUser user);

        Task<bool> SaveAllAsnync();

        //Task<IEnumerable<AppUser>> GetUsersAsync();

        Task<IEnumerable<AppUserDTO>> GetAppUsersDTOAsync();

        Task<AppUser> GetUserByIdAsync (int id);


        Task<AppUser> UpdateAsync(AppUser entity);

        Task<AppUser> RemoveUserAsync(AppUser entity);

    }
}
