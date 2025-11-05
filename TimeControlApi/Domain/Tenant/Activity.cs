namespace TimeControlApi.Domain.Tenant
{
    public class Activity
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public byte Status { get; set; } = 1; // 1=Activo, 0=Inactivo
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual ICollection<Timesheet> Timesheets { get; set; } = new List<Timesheet>();
    }
}

