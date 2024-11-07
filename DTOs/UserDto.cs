namespace DTOs
{
    public class UserDTO

    {
        public string GoogleId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public List<UserFileDTO> UserFiles { get; set; }
    }
}
