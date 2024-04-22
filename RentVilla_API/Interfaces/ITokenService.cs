using RentVilla_API.Entities;

namespace RentVilla_API.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}
