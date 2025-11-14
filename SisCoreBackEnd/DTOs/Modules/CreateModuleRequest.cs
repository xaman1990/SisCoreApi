namespace SisCoreBackEnd.DTOs.Modules
{
    public class CreateModuleRequest
    {
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int MenuOrder { get; set; } = 0;
        public int? ParentModuleId { get; set; }
    }
}

