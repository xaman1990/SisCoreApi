namespace SisCoreBackEnd.Tenancy
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        public TenantMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, ITenantResolver resolver)
        {
            var tenant = await resolver.ResolveAsync(context);
            if (tenant != null) context.Items["TENANT"] = tenant;
            await _next(context);
        }
    }

    public static class TenantAppBuilderExtensions
    {
        public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app)
            => app.UseMiddleware<TenantMiddleware>();

        public static TenantContext? GetTenant(this HttpContext http)
            => http.Items.TryGetValue("TENANT", out var t) ? (TenantContext)t! : null;
    }
}
