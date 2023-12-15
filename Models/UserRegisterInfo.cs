namespace PersistentRegister.Models
{
    public class UserRegisterInfo
    {
        public Guid ID { get; set; }
        public Guid UserID { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime RegistrationDate { get; set; }
        public TimeSpan TotalPersistTime { get; set; }
    }
}