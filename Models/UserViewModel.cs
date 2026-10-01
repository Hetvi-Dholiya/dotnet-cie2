namespace _091.Models
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public CompanyDto Company { get; set; } = new();
    }

    public class CompanyDto
    {
        public string Name { get; set; } = string.Empty;
    }
}