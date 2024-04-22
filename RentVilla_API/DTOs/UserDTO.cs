using System.ComponentModel.DataAnnotations;

namespace RentVilla_API.DTOs
{
    public class UserDTO
    {
        public string Email { get; set; }

        public string Token { get; set; }
    }
}
