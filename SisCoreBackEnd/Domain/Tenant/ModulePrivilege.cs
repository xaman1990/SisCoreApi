namespace SisCoreBackEnd.Domain.Tenant
{
    public class ModulePrivilege
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public int PermissionId { get; set; }
        public int? SubModuleId { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsDefault { get; set; } = false;

        // Navigation properties
        public virtual Module Module { get; set; } = null!;
        public virtual Permission Permission { get; set; } = null!;
        public virtual SubModule? SubModule { get; set; }
        public virtual ICollection<PermissionAssignment> PermissionAssignments { get; set; } = new List<PermissionAssignment>();
    }
}

