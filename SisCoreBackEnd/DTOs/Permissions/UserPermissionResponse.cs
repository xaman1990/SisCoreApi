namespace SisCoreBackEnd.DTOs.Permissions
{
    public class UserPermissionResponse
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = default!;
        public List<ModulePermission> Permissions { get; set; } = new();
        public List<RoleInfo> Roles { get; set; } = new();
    }

    public class RoleInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }
}

