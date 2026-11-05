using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SICPAI.Security;
using Xunit;

namespace SICPAI.Tests;

public class PermissionHandlerTests
{
    [Fact]
    public async Task GrantsAccessWhenPermissionClaimExists()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("permission", "reports.export") }, "test"));
        var requirement = new PermissionRequirement("reports.export");
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);
        await new PermissionHandler().HandleAsync(context);
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task DeniesAccessWhenPermissionClaimIsMissing()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Consultor") }, "test"));
        var requirement = new PermissionRequirement("reports.export");
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);
        await new PermissionHandler().HandleAsync(context);
        Assert.False(context.HasSucceeded);
    }
}
