namespace TimeControlApi.Domain.Tenant.FormBuilder
{
    /// <summary>
    /// Plantillas de formularios generadas por IA
    /// </summary>
    public class FormTemplate
    {
        public Guid TenantId { get; set; }
        public Guid FormTemplateId { get; set; }
        public string Code { get; set; } = default!; // UNIQUE por tenant
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string SchemaJson { get; set; } = default!; // JSON Schema del formulario
        public string? UiSchemaJson { get; set; } // JSON Schema UI (layout, widgets)
        public string? GeneratedPrompt { get; set; } // Prompt original usado para generar
        public string? AiModel { get; set; } // gpt-4o, gpt-4-turbo, claude-3, etc.
        public string Status { get; set; } = "Draft"; // Draft, Published, Archived
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual ICollection<FormField> FormFields { get; set; } = new List<FormField>();
        public virtual ICollection<BusinessRule> BusinessRules { get; set; } = new List<BusinessRule>();
        public virtual ICollection<FormVersion> FormVersions { get; set; } = new List<FormVersion>();
        public virtual ICollection<FormAudit> FormAudits { get; set; } = new List<FormAudit>();
    }
}

