namespace SisCoreBackEnd.Domain.Tenant.FormBuilder
{
    /// <summary>
    /// Campos de un formulario (metadatos detallados)
    /// </summary>
    public class FormField
    {
        public Guid TenantId { get; set; }
        public Guid FormFieldId { get; set; }
        public Guid FormTemplateId { get; set; }
        public string FieldName { get; set; } = default!; // Nombre técnico
        public string FieldType { get; set; } = default!; // text, number, date, select, etc.
        public string Label { get; set; } = default!; // Etiqueta visible
        public string? Placeholder { get; set; }
        public bool IsRequired { get; set; } = false;
        public string? DefaultValue { get; set; } // JSON
        public string? ValidationRules { get; set; } // JSON con reglas de validación
        public int DisplayOrder { get; set; } = 0;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual FormTemplate FormTemplate { get; set; } = null!;
    }
}

