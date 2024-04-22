using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using RentVilla_API.DTOs;
using RentVilla_API.Entities;
using RentVilla_API.Interfaces;
using RentVilla_API.Logger;
using RentVilla_API.Logger.LoogerInterfaces;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace RentVilla_API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDBContext _appdbContext;
        private readonly IMapper _mapper;
        private readonly ILoggerDev _logger;
        public UserRepository(AppDBContext appDBContext, IMapper mapper, ILoggerDev logger)
        {
            _appdbContext = appDBContext;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            _logger.Log("User Repository:  Retrieve AppUser by id: " + id + " from DataBase", "info");
            return await _appdbContext.Users.
                Where(user => user.Id == id).SingleOrDefaultAsync();
        }

        public async Task<AppUserDTO> GetAppUserDTOByIdAsync(int id)
        { 
            return await _appdbContext.Users
                .Where(user=> user.Id == id)
                .ProjectTo<AppUserDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
               
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            _logger.Log("User Repository: Retrive all AppUser from DataBase", "info");
            return await _appdbContext.Users.ToListAsync();
        }

        public async Task<IEnumerable<AppUserDTO>> GetAppUsersDTOAsync()
        {
            _logger.Log("User Repository:  Check if All AppUserDTO retrive from DataBase", "info");
           return await _appdbContext.Users
                .ProjectTo<AppUserDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<bool> SaveAllAsnync()
        {
            _logger.Log("User Repository:  Check if Changes are saved successfully!!", "info");
            return await _appdbContext.SaveChangesAsync() > 0;
        }

        public void update(AppUser user)
        {
            _logger.Log("User Repository: Check if AppUser", "info");
            _appdbContext.Entry(user).State = EntityState.Modified;
        }

        public async Task<AppUser> UpdateAsync(AppUser entity)
        {
            _logger.Log("User Repository: Update AppUser", "info");
            _appdbContext.Users.Update(entity);
            await _appdbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<AppUser> RemoveUserAsync(AppUser entity)
        {
            _logger.Log("User Repository: Remove AppUser", "info");
            _appdbContext.Remove(entity);

            await _appdbContext.SaveChangesAsync();

            return entity;
        }

    }
}
