using System.ComponentModel.DataAnnotations;

namespace RentVilla_API.DTOs
{
    public class RegisterDTO
    {
        [Required]
        [MinLength(6)]
        public string Email {  get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }

    }
}
