namespace TimeControlApi.DTOs.Auth
{
    public class RegisterUserRequest
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Password { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string? EmployeeNumber { get; set; }
        public List<int> RoleIds { get; set; } = new();
    }
}

