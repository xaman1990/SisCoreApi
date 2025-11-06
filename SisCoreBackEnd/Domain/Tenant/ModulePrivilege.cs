namespace TimeControlApi.Domain.Tenant
{
    /// <summary>
    /// Privilegios o acciones dentro de un módulo (Create, Read, Update, Delete, Execute, Publish, etc.)
    /// </summary>
    public class ModulePrivilege
    {
        public Guid TenantId { get; set; }
        public Guid ModulePrivilegeId { get; set; }
        public Guid ModuleId { get; set; }
        public Guid? SubModuleId { get; set; }
        public string Action { get; set; } = default!; // CRUD/Execute/Publish/Approve/Export
        public string Scope { get; set; } = default!; // OwnTenant/SubTenant/System
        public string? Description { get; set; }
        public Guid? GrantedBy { get; set; }
        public DateTimeOffset ValidFrom { get; set; }
        public DateTimeOffset? ValidTo { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual Module Module { get; set; } = null!;
        public virtual SubModule? SubModule { get; set; }
        public virtual ICollection<PermissionAssignment> PermissionAssignments { get; set; } = new List<PermissionAssignment>();
    }
}

