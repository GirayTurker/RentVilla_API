namespace RentVilla_API.Entities
{
    public class AppUserAddress
    {
        // 1 to Many (1 User can have many addresses)
        public int Id { get; set; }

        public int AppuserID { get; set; }

        public AppUser AppUser { get; set; }
        public string Email { get; set; }

        public string PhoneNumber { get; set; }
        public DateTime? Created { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }

        public short? ZipCode { get; set; }

        public string State { get; set; }

        public string City { get; set; }
    }
}