namespace SisCoreBackEnd.Domain.Tenant
{
    public class Module
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!; // timesheet, reports, settings, etc.
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? Icon { get; set; } // Nombre del icono
        public int MenuOrder { get; set; } = 0;
        public bool IsEnabled { get; set; } = true;
        public bool IsSystem { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<SubModule> SubModules { get; set; } = new List<SubModule>();
        public virtual ICollection<ModulePrivilege> ModulePrivileges { get; set; } = new List<ModulePrivilege>();
    }
}

