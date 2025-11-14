namespace SisCoreBackEnd.DTOs.Permissions
{
    public class CreateModulePrivilegeRequest
    {
        public int PermissionId { get; set; }
        public int? SubModuleId { get; set; }
        public string? Description { get; set; }
        public string? NameOverride { get; set; }
        public bool? IsDefault { get; set; }
    }
}

