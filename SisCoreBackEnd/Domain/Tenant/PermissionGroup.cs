namespace SisCoreBackEnd.Domain.Tenant
{
    /// <summary>
    /// Grupos de permisos para asignación masiva (ej: "Editor Completo", "Solo Lectura")
    /// </summary>
    public class PermissionGroup
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!; // UNIQUE por tenant
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsSystem { get; set; } = false; // Grupos del sistema
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual ICollection<PermissionGroupAssignment> PermissionGroupAssignments { get; set; } = new List<PermissionGroupAssignment>();
    }

    /// <summary>
    /// Relación entre grupos de permisos y privilegios
    /// </summary>
    public class PermissionGroupAssignment
    {
        public int PermissionGroupId { get; set; }
        public int ModulePrivilegeId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }

        // Navigation properties
        public virtual PermissionGroup PermissionGroup { get; set; } = null!;
        public virtual ModulePrivilege ModulePrivilege { get; set; } = null!;
    }
}

