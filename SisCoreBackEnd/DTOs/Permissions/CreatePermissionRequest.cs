namespace SisCoreBackEnd.DTOs.Permissions
{
    public class CreatePermissionRequest
    {
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsSystem { get; set; } = false;
        public bool IsDefaultForModule { get; set; } = true;
    }
}

