using EduGestor.Infrastructure.Tenancy;

namespace EduGestor.Api.Tests;

public class TenantNotResolvedExceptionTests
{
    [Fact]
    public void DefaultConstructor_HasExpectedMessage()
    {
        var ex = new TenantNotResolvedException();

        Assert.Equal("No tenant context resolved for the current request.", ex.Message);
    }

    [Fact]
    public void MessageConstructor_PreservesMessage()
    {
        var customMessage = "Custom error message";
        var ex = new TenantNotResolvedException(customMessage);

        Assert.Equal(customMessage, ex.Message);
    }

    [Fact]
    public void InheritsFromInvalidOperationException()
    {
        var ex = new TenantNotResolvedException();

        Assert.IsAssignableFrom<InvalidOperationException>(ex);
    }
}
