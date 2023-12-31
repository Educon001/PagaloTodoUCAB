using System.Diagnostics.CodeAnalysis;
using System.Net;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;
using UCABPagaloTodoMS.Infrastructure.Services;
using Xunit;

namespace UCABPagaloTodoMS.Tests.UnitTestsInfrastructure.Services;
[ExcludeFromCodeCoverage]
public class ForgotPasswordEmailSenderTest
{
    private readonly ForgotPasswordEmailSender _sender;
    private readonly Mock<ISendGridClient> _mockClient;

    public ForgotPasswordEmailSenderTest()
    {
        _mockClient = new Mock<ISendGridClient>();
        _sender = new ForgotPasswordEmailSender(_mockClient.Object, "test@email.com", "Test");
    }

    [Fact]
    public async void SendEmailAsync_Test()
    {
        var email = "prueba@email.com";
        var url = "https://www.test.com";
        var expectedResponse =
            new Response(HttpStatusCode.Accepted, null,null);
        _mockClient.Setup(m => m.SendEmailAsync(It.IsAny<SendGridMessage>(), CancellationToken.None))
            .ReturnsAsync(expectedResponse);
        var response = await _sender.SendEmailAsync(email,url);
        Assert.IsType<HttpStatusCode>(response);
        Assert.Equal(expectedResponse.StatusCode,response);
    }
}