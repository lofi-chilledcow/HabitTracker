using AuthService.Controllers;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace AuthService.Tests.Controllers;

public class AdminUsersControllerTests
{
    [Fact]
    public void Controller_RequiresAdminOnlyPolicy()
    {
        var attribute = typeof(AdminUsersController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .Single();

        Assert.Equal("AdminOnly", attribute.Policy);
    }
}
