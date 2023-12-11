namespace PersistentRegister.Dtos.User
{
    public class UpdateUserDto
    {
        public Guid ID { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}