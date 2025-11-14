namespace SisCoreBackEnd.Domain.Tenant.FormBuilder
{
    /// <summary>
    /// Auditoría de uso de formularios (submisiones, visualizaciones)
    /// </summary>
    public class FormAudit
    {
        public long FormAuditId { get; set; } // IDENTITY
        public Guid TenantId { get; set; }
        public Guid FormTemplateId { get; set; }
        public Guid? FormVersionId { get; set; }
        public Guid UserId { get; set; }
        public string Action { get; set; } = default!; // View, Submit, Edit, Delete
        public string? FormData { get; set; } // JSON con datos del formulario (redactado)
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation properties
        public virtual FormTemplate FormTemplate { get; set; } = null!;
        public virtual FormVersion? FormVersion { get; set; }
    }
}

