namespace TimeControlApi.Domain.Tenant
{
    public class Timesheet
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public int ActivityId { get; set; }
        public DateOnly Date { get; set; } // Fecha del trabajo realizado
        public decimal Hours { get; set; } // Horas trabajadas (mínimo 0.25)
        public string? Note { get; set; }
        public decimal? LocationLatitude { get; set; } // Latitud GPS
        public decimal? LocationLongitude { get; set; } // Longitud GPS
        public string? LocationAddress { get; set; } // Dirección derivada de GPS
        public byte Status { get; set; } = 0; // 0=Pendiente, 1=Aprobado, 2=Rechazado
        public int? ApprovedBy { get; set; } // Supervisor que aprobó
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int CreatedBy { get; set; } // Usuario que registró
        public int? UpdatedBy { get; set; } // Usuario que modificó

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Project Project { get; set; } = null!;
        public virtual Activity Activity { get; set; } = null!;
    }
}
