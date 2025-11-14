namespace SisCoreBackEnd.Domain.Tenant
{
    public class Permission
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsSystem { get; set; } = false;
        public bool IsDefaultForModule { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        // Navigation properties
        public virtual ICollection<ModulePrivilege> ModulePrivileges { get; set; } = new List<ModulePrivilege>();
    }
}

