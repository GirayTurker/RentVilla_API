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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure one-to-many relationship between AppUser and AppUserAddress
            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.UserAddresses) // One AppUser has many AppUserAddresses
                .WithOne(a => a.AppUser) // Each AppUserAddress belongs to one AppUser
                .HasForeignKey(a => a.AppuserID) // Foreign key property
                .IsRequired(false); // AppUserAddress can be optional

            // Configure default value for AppuserID in AppUserAddress
            modelBuilder.Entity<AppUserAddress>()
                .Property(a => a.AppuserID)
                .HasDefaultValue(0); // Or whatever default value you prefer
        }

    }
}
