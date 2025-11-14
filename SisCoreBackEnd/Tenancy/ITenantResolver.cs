namespace SisCoreBackEnd.Tenancy
{
    public interface ITenantResolver
    {
        Task<TenantContext?> ResolveAsync(HttpContext http);
    }
}
