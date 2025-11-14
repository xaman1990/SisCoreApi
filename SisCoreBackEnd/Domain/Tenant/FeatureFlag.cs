namespace SisCoreBackEnd.Domain.Tenant
{
    /// <summary>
    /// Feature flags por tenant para control de funcionalidades
    /// </summary>
    public class FeatureFlag
    {
        public Guid TenantId { get; set; }
        public Guid FeatureFlagId { get; set; }
        public string Code { get; set; } = default!; // UNIQUE por tenant
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsEnabled { get; set; } = false;
        public int RolloutPercentage { get; set; } = 0; // 0-100 para rollout gradual
        public DateTimeOffset ValidFrom { get; set; }
        public DateTimeOffset? ValidTo { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}

