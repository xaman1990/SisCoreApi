namespace SisCoreBackEnd.Tenancy
{
    public class TenantContext
    {
        public int CompanyId { get; set; }
        public string ConnectionString { get; set; } = default!;
        public string Subdomain { get; set; } = default!;
    }
}
