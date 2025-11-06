namespace TimeControlApi.Domain.Tenant
{
    public class Permission
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public string Code { get; set; } = default!; // create, read, update, delete, approve, etc.
        public string Name { get; set; } = default!;
        public string? Description { get; set; }

        // Navigation properties
        public virtual Module Module { get; set; } = null!;
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

    public class RolePermission
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
        public int? GrantedBy { get; set; }

        // Navigation properties
        public virtual Role Role { get; set; } = null!;
        public virtual Permission Permission { get; set; } = null!;
    }
}

