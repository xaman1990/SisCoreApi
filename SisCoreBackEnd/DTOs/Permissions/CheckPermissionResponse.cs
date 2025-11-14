namespace SisCoreBackEnd.DTOs.Permissions
{
    public class CheckPermissionResponse
    {
        public bool HasPermission { get; set; }
        public string? Source { get; set; } // "Role", "Direct", "Inherited"
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public string? Message { get; set; }
    }
}

