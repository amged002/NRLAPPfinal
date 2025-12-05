using Microsoft.AspNetCore.Mvc;
using NRLApp.Controllers;
using Xunit;

namespace NRLApp.Tests.Controllers;

public class SimpleControllersTests
{
    [Fact]
    public void Home_Index_ReturnsView()
    {
        var controller = new HomeController();

        var result = controller.Index();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Contact_Index_ReturnsView()
    {
        var controller = new ContactController();

        var result = controller.Index();

        Assert.IsType<ViewResult>(result);
    }
}