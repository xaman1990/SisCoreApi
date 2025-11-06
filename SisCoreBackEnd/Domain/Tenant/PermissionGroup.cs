namespace TimeControlApi.Domain.Tenant
{
    /// <summary>
    /// Grupos de permisos para asignación masiva (ej: "Editor Completo", "Solo Lectura")
    /// </summary>
    public class PermissionGroup
    {
        public Guid TenantId { get; set; }
        public Guid PermissionGroupId { get; set; }
        public string Code { get; set; } = default!; // UNIQUE por tenant
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsSystem { get; set; } = false; // Grupos del sistema
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual ICollection<PermissionGroupAssignment> PermissionGroupAssignments { get; set; } = new List<PermissionGroupAssignment>();
    }

    /// <summary>
    /// Relación entre grupos de permisos y privilegios
    /// </summary>
    public class PermissionGroupAssignment
    {
        public Guid TenantId { get; set; }
        public Guid PermissionGroupId { get; set; }
        public Guid ModulePrivilegeId { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid CreatedBy { get; set; }

        // Navigation properties
        public virtual PermissionGroup PermissionGroup { get; set; } = null!;
        public virtual ModulePrivilege ModulePrivilege { get; set; } = null!;
    }
}

