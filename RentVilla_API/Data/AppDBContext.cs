using Microsoft.EntityFrameworkCore;
using RentVilla_API.Entities;

namespace RentVilla_API.Data
{
    public class AppDBContext:DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options): base(options) 
        { 

        }

        //First migration:: add-migration AddAppUserTable -o Data/Migrations  // update-database
        public DbSet <AppUser> Users { get; set; }

        public DbSet<AppUserAddress> UsersAddress { get; set; }

    }
}
