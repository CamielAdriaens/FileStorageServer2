namespace DTOs
{
    public class UserDto
    {
        public string GoogleId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public List<UserFileDto> UserFiles { get; set; }
    }
}
