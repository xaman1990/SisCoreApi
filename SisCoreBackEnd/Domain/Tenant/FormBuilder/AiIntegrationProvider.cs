namespace SisCoreBackEnd.Domain.Tenant.FormBuilder
{
    /// <summary>
    /// Configuración de proveedores de IA (OpenAI, Azure OpenAI, etc.)
    /// </summary>
    public class AiIntegrationProvider
    {
        public Guid TenantId { get; set; }
        public Guid AiProviderId { get; set; }
        public string ProviderName { get; set; } = default!; // OpenAI, AzureOpenAI, Anthropic, etc.
        public string ApiKey { get; set; } = default!; // Encriptado
        public string? BaseUrl { get; set; }
        public string DefaultModel { get; set; } = default!; // gpt-4o, gpt-4-turbo, etc.
        public int MaxTokens { get; set; } = 2000; // Límite de tokens por request
        public int RateLimitPerMinute { get; set; } = 60; // Límite de requests por minuto
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual ICollection<PromptHistory> PromptHistories { get; set; } = new List<PromptHistory>();
    }

    /// <summary>
    /// Historial de prompts enviados a IA para trazabilidad y cache
    /// </summary>
    public class PromptHistory
    {
        public long PromptHistoryId { get; set; } // IDENTITY
        public Guid TenantId { get; set; }
        public Guid UserId { get; set; }
        public string PromptText { get; set; } = default!; // Prompt original
        public string PromptHash { get; set; } = default!; // SHA-256 para búsqueda de similares
        public string? AiProvider { get; set; }
        public string? AiModel { get; set; }
        public string? ResponseJson { get; set; } // Respuesta de IA
        public int? TokensUsed { get; set; }
        public decimal? CostUsd { get; set; } // Costo estimado
        public int? DurationMs { get; set; } // Tiempo de respuesta
        public Guid? FormTemplateId { get; set; } // Si generó formulario
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation properties
        public virtual AiIntegrationProvider? Provider { get; set; }
        public virtual FormTemplate? FormTemplate { get; set; }
    }
}

