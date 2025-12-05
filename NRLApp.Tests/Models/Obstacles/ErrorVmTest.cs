using NRLApp.Models;
using Xunit;

namespace NRLApp.Tests.Models;

public class ErrorViewModelTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("request-123", true)]
    public void ShowRequestId_ReflectsPresenceOfRequestId(string? requestId, bool expected)
    {
        var model = new ErrorViewModel { RequestId = requestId };

        Assert.Equal(expected, model.ShowRequestId);
    }
}