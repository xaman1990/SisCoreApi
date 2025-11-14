namespace SisCoreBackEnd.DTOs.Permissions
{
    public class ModulePrivilegeResponse
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public string ModuleCode { get; set; } = default!;
        public string ModuleName { get; set; } = default!;
        public int PermissionId { get; set; }
        public string PermissionCode { get; set; } = default!;
        public string PermissionName { get; set; } = default!;
        public int? SubModuleId { get; set; }
        public string? SubModuleCode { get; set; }
        public string? SubModuleName { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsDefault { get; set; }
        public bool HasAssignments { get; set; }
    }

    public class ModulePrivilegeFilter
    {
        public int? ModuleId { get; set; }
        public int? SubModuleId { get; set; }
        public int? PermissionId { get; set; }
        public string? PermissionCode { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }
}

