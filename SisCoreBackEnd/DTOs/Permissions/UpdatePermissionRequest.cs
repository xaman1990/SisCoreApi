namespace SisCoreBackEnd.DTOs.Permissions
{
    public class UpdatePermissionRequest
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool? IsSystem { get; set; }
        public bool? IsDefaultForModule { get; set; }
    }
}

