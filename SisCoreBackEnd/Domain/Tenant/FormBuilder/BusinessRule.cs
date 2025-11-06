namespace TimeControlApi.Domain.Tenant.FormBuilder
{
    /// <summary>
    /// Reglas de negocio para formularios (validaciones, cálculos, acciones)
    /// </summary>
    public class BusinessRule
    {
        public Guid TenantId { get; set; }
        public Guid BusinessRuleId { get; set; }
        public Guid FormTemplateId { get; set; }
        public string RuleType { get; set; } = default!; // Validation, Calculation, Action, Visibility
        public string RuleName { get; set; } = default!;
        public string Expression { get; set; } = default!; // JavaScript/JSON Logic expression
        public string? TriggerFields { get; set; } // JSON array de campos que activan
        public string? TargetFields { get; set; } // JSON array de campos afectados
        public string? ErrorMessage { get; set; } // Mensaje de error si falla validación
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public Guid CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual FormTemplate FormTemplate { get; set; } = null!;
    }
}

