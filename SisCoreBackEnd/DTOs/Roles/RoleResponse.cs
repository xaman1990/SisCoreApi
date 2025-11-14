namespace SisCoreBackEnd.DTOs.Roles
{
    public class RoleResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsSystem { get; set; }
        public byte Status { get; set; }
        public List<PermissionInfo> Permissions { get; set; } = new();
    }

    public class PermissionInfo
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string ModuleCode { get; set; } = default!;
    }
}

