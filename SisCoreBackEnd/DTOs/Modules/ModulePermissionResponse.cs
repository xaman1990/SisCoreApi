namespace SisCoreBackEnd.DTOs.Modules
{
    public class ModulePermissionResponse
    {
        public int ModuleId { get; set; }
        public string ModuleCode { get; set; } = default!;
        public string ModuleName { get; set; } = default!;
        public List<PermissionDetail> Permissions { get; set; } = new();
    }

    public class PermissionDetail
    {
        public int ModulePrivilegeId { get; set; }
        public int PermissionId { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool HasPermission { get; set; }
        public string Source { get; set; } = default!;
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
    }
}

