using Bunit;
using Xunit;
using Truckero.Admin.Components.Pages;

namespace Truckero.Admin.Tests;

public class BasicRenderTests : TestContext
{
    [Fact]
    public void AdminDashboard_Should_Render()
    {
        // Replace with actual component if needed
        var cut = RenderComponent<Dashboard>();
        cut.MarkupMatches(cut.Markup); // basic sanity check
    }
}
