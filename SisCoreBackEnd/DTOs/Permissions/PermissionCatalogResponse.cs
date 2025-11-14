namespace SisCoreBackEnd.DTOs.Permissions
{
    public class PermissionCatalogResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsSystem { get; set; }
        public bool IsDefaultForModule { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class PermissionCatalogFilter
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool IncludeSystem { get; set; } = true;
        public bool? OnlyDefaults { get; set; }
    }
}

