namespace SisCoreBackEnd.DTOs.Permissions
{
    public class CheckPermissionRequest
    {
        public int? UserId { get; set; }
        public int ModuleId { get; set; }
        public string PermissionCode { get; set; } = default!;
    }
}

