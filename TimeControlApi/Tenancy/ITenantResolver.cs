namespace TimeControlApi.Tenancy
{
    public interface ITenantResolver
    {
        Task<TenantContext?> ResolveAsync(HttpContext http);
    }
}
