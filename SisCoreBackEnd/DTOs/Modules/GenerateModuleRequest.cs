namespace SisCoreBackEnd.DTOs.Modules
{
    public class GenerateModuleRequest
    {
        public string Prompt { get; set; } = default!;
        public string AiModel { get; set; } = "gpt-4o";
    }
}

