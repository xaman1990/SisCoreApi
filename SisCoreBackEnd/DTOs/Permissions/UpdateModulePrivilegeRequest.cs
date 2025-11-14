namespace SisCoreBackEnd.DTOs.Permissions
{
    public class UpdateModulePrivilegeRequest
    {
        public int? PermissionId { get; set; }
        public int? SubModuleId { get; set; }
        public bool RemoveSubModule { get; set; } = false;
        public string? NameOverride { get; set; }
        public string? Description { get; set; }
        public bool? IsDefault { get; set; }
    }
}

