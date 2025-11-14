namespace SisCoreBackEnd.Domain.Tenant
{
    public class Project
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!; // Código único del proyecto/OV
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int? ClientId { get; set; } // FK a tabla Clients si existe
        public int? CostCenterId { get; set; } // FK a tabla CostCenters si existe
        public decimal DefaultHoursPerDay { get; set; } = 9.00m; // Horas por defecto configurable por proyecto
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public byte Status { get; set; } = 1; // 1=Activo, 0=Inactivo
        public int? SupervisorId { get; set; } // Supervisor asignado
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedBy { get; set; }

        // Navigation properties
        public virtual User? Supervisor { get; set; }
        public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
        public virtual ICollection<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();
        public virtual ICollection<Timesheet> Timesheets { get; set; } = new List<Timesheet>();
    }

    public class ProjectUser
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public int? AssignedBy { get; set; }

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}

