namespace SisCoreBackEnd.Domain.Tenant
{
    /// <summary>
    /// Asignación de permisos a roles o usuarios (permisos directos)
    /// </summary>
    public class PermissionAssignment
    {
        public int Id { get; set; }
        public int ModulePrivilegeId { get; set; }
        public int? RoleId { get; set; } // Opcional, si es null es directo a usuario
        public int? UserId { get; set; } // Opcional, si es null es a rol
        public int? GrantedBy { get; set; }
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsInherited { get; set; } = false; // Si viene de herencia
        public int? OverrideParentId { get; set; } // Override de asignación padre
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? UpdatedAt { get; set; } 
        public int? UpdatedBy { get; set; }

        // Navigation properties
        public virtual ModulePrivilege ModulePrivilege { get; set; } = null!;
        public virtual Role? Role { get; set; }
        public virtual User? User { get; set; }
        public virtual PermissionAssignment? OverrideParent { get; set; }
    }
}

