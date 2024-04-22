using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace RentVilla_API.Entities
{
    public class AppUser
    {
       
        [Required]
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; }

        
        [Required]
        public string Email { get; set; }

        
        [Required]
        public byte[] PasswordHash { get; set; }

        
        [Required]
        public byte[] PasswordSalt { get; set; }

        [Required]
        public DateTime Created { get; set; }

        public ICollection<AppUserAddress> UserAddresses { get; set; } = new List<AppUserAddress>();

    }
}
