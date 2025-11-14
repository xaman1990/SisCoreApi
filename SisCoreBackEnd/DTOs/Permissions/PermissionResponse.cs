namespace SisCoreBackEnd.DTOs.Permissions
{
    public class PermissionResponse
    {
        public int UserId { get; set; }
        public List<ModulePermission> Permissions { get; set; } = new();
    }

    public class ModulePermission
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public List<PermissionPrivilege> Privileges { get; set; } = new();
    }

    public class PermissionPrivilege
    {
        public int ModulePrivilegeId { get; set; }
        public int PermissionId { get; set; }
        public int? PermissionAssignmentId { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public bool HasPermission { get; set; }
    }
}

