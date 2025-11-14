namespace SisCoreBackEnd.DTOs.Modules
{
    public class UpdateModuleRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public int? MenuOrder { get; set; }
        public bool? IsEnabled { get; set; }
    }
}

