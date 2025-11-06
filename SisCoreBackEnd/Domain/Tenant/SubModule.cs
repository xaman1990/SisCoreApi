namespace TimeControlApi.Domain.Tenant
{
    /// <summary>
    /// Submódulos dentro de un módulo (opcional, para organización jerárquica)
    /// </summary>
    public class SubModule
    {
        public Guid TenantId { get; set; }
        public Guid SubModuleId { get; set; }
        public Guid ModuleId { get; set; }
        public string Code { get; set; } = default!; // UNIQUE por módulo
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int MenuOrder { get; set; } = 0;
        public bool IsEnabled { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual Module Module { get; set; } = null!;
        public virtual ICollection<ModulePrivilege> ModulePrivileges { get; set; } = new List<ModulePrivilege>();
    }
}

