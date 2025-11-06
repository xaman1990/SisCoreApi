namespace TimeControlApi.Domain.Tenant.FormBuilder
{
    /// <summary>
    /// Publicación de formularios (control de acceso y rollout)
    /// </summary>
    public class FormPublication
    {
        public Guid TenantId { get; set; }
        public Guid FormPublicationId { get; set; }
        public Guid FormVersionId { get; set; }
        public string PublicationType { get; set; } = default!; // Public, Private, RoleBased, UserBased
        public string? TargetRoleIds { get; set; } // JSON array de RoleIds (si RoleBased)
        public string? TargetUserIds { get; set; } // JSON array de UserIds (si UserBased)
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual FormVersion FormVersion { get; set; } = null!;
    }
}

