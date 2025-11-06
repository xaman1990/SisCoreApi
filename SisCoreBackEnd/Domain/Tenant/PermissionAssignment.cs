namespace TimeControlApi.Domain.Tenant
{
    /// <summary>
    /// Asignación de permisos a roles o usuarios (permisos directos)
    /// </summary>
    public class PermissionAssignment
    {
        public Guid TenantId { get; set; }
        public Guid PermissionAssignmentId { get; set; }
        public Guid ModulePrivilegeId { get; set; }
        public Guid? RoleId { get; set; } // Opcional, si es null es directo a usuario
        public Guid? UserId { get; set; } // Opcional, si es null es a rol
        public Guid GrantedBy { get; set; }
        public DateTimeOffset GrantedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset ValidFrom { get; set; }
        public DateTimeOffset? ValidTo { get; set; }
        public bool IsInherited { get; set; } = false; // Si viene de herencia
        public Guid? OverrideParentId { get; set; } // Override de asignación padre
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid CreatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual ModulePrivilege ModulePrivilege { get; set; } = null!;
        public virtual Role? Role { get; set; }
        public virtual User? User { get; set; }
        public virtual PermissionAssignment? OverrideParent { get; set; }
    }
}

