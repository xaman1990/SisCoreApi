namespace TimeControlApi.Domain.Tenant.FormBuilder
{
    /// <summary>
    /// Versiones de formularios para control de cambios y rollback
    /// </summary>
    public class FormVersion
    {
        public Guid TenantId { get; set; }
        public Guid FormVersionId { get; set; }
        public Guid FormTemplateId { get; set; }
        public string VersionNumber { get; set; } = default!; // Semántico (1.0.0, 1.1.0, etc.)
        public string SchemaJson { get; set; } = default!; // JSON Schema de esta versión
        public string? UiSchemaJson { get; set; }
        public string? ChangeNotes { get; set; } // Notas de cambios
        public bool IsPublished { get; set; } = false;
        public DateTimeOffset? PublishedAt { get; set; }
        public Guid? PublishedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid CreatedBy { get; set; }

        // Navigation properties
        public virtual FormTemplate FormTemplate { get; set; } = null!;
        public virtual ICollection<FormPublication> FormPublications { get; set; } = new List<FormPublication>();
    }
}

