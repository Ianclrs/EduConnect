namespace EduGestor.Infrastructure.Tenancy;

public class TenantNotResolvedException : InvalidOperationException
{
    public TenantNotResolvedException()
        : base("No tenant context resolved for the current request.") { }

    public TenantNotResolvedException(string message)
        : base(message) { }
}
