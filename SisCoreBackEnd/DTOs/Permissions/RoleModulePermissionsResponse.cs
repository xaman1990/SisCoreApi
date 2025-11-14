namespace SisCoreBackEnd.DTOs.Permissions
{
    public class RoleModulePermissionsResponse
    {
        public int RoleId { get; set; }
        public int ModuleId { get; set; }
        public string ModuleCode { get; set; } = default!;
        public string ModuleName { get; set; } = default!;
        public List<RoleModulePrivilegeStatus> Privileges { get; set; } = new();
    }

    public class RoleModulePrivilegeStatus
    {
        public int ModulePrivilegeId { get; set; }
        public int PermissionId { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsGranted { get; set; }
        public bool IsDefault { get; set; }
    }

    public class UpdateRoleModulePermissionsRequest
    {
        public List<RoleModulePermissionsUpdate> Modules { get; set; } = new();
    }

    public class RoleModulePermissionsUpdate
    {
        public int ModuleId { get; set; }
        public List<RoleModulePrivilegeToggle> Privileges { get; set; } = new();
    }

    public class RoleModulePrivilegeToggle
    {
        public int ModulePrivilegeId { get; set; }
        public bool IsGranted { get; set; }
    }
}

