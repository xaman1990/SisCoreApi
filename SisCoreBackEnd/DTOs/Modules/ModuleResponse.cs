namespace SisCoreBackEnd.DTOs.Modules
{
    public class ModuleResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int MenuOrder { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsSystem { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ModulePrivilegeInfo>? Privileges { get; set; }
        public List<SubModuleInfo>? SubModules { get; set; }
    }

    public class ModulePrivilegeInfo
    {
        public int Id { get; set; }
        public int PermissionId { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
    }

    public class SubModuleInfo
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int MenuOrder { get; set; }
        public bool IsEnabled { get; set; }
    }
}

