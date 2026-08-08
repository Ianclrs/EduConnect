using Microsoft.AspNetCore.Authorization;
using EduGestor.Infrastructure.Tenancy;

namespace EduGestor.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        var endpoint = context.GetEndpoint();
        var requireAuth = endpoint?.Metadata
            .GetMetadata<AuthorizeAttribute>() != null;

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("tenant_id");
            if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            {
                if (requireAuth)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(
                        "{\"error\":\"tenant_not_resolved\",\"message\":\"Tenant context could not be resolved from the current request.\"}");
                    return;
                }
            }
            else
            {
                tenantContext.SetTenant(tenantId);
            }
        }

        await _next(context);
    }
}
