namespace SisCoreBackEnd.Domain.Tenant
{
    /// <summary>
    /// Submódulos dentro de un módulo (opcional, para organización jerárquica)
    /// </summary>
    public class SubModule
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public string Code { get; set; } = default!; // UNIQUE por módulo
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int MenuOrder { get; set; } = 0;
        public bool IsEnabled { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual Module Module { get; set; } = null!;
        public virtual ICollection<ModulePrivilege> ModulePrivileges { get; set; } = new List<ModulePrivilege>();
    }
}

