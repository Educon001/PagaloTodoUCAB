using Microsoft.Extensions.Logging;
using Moq;
using UCABPagaloTodoMS.Application.Handlers.Commands;
using UCABPagaloTodoMS.Core.Database;
using UCABPagaloTodoMS.Application.Commands;
using UCABPagaloTodoMS.Application.Requests;
using UCABPagaloTodoMS.Application.Responses;
using Xunit;

namespace UCABPagaloTodoMS.Tests.UnitTestsApplication.Handlers.Commands.Login;

public class LoginCommandHandlerTest
{
    private readonly LoginCommandHandler _handler;
    private readonly Mock<ILogger<LoginCommandHandler>> _loggerMock;
    private readonly Mock<IUCABPagaloTodoDbContext> _mockContext;

    public LoginCommandHandlerTest()
    {
        _loggerMock = new Mock<ILogger<LoginCommandHandler>>();
        _mockContext = new Mock<IUCABPagaloTodoDbContext>();
        _handler = new LoginCommandHandler(_mockContext.Object, _loggerMock.Object);
        DataSeed.DataSeed.SetupDbContextData(_mockContext);
    }

    [Fact]
    public async Task Handle_ValidConsumerLogin_ReturnsLoginResponse()
    {
        var entity = _mockContext.Object.Consumers.First();
        var expectedResponse = new LoginResponse { Id = entity.Id, UserType = "consumer", Token = "some.jwt.token" };
        var request = new LoginRequest { Username = entity.Username, PasswordHash = "Password.", UserType = "consumer" };
        var command = new LoginCommand(request);

        // Act
        var response = await _handler.Handle(command, default);

        // Assert
        Assert.IsType<LoginResponse>(response);
        Assert.Equal(expectedResponse.ToString(), response.ToString());
    }
    
    [Fact]
    public async Task Handle_ValidProviderLogin_ReturnsLoginResponse()
    {
        var entity = _mockContext.Object.Providers.First();
        var expectedResponse = new LoginResponse { Id = entity.Id, UserType = "provider", Token = "some.jwt.token" };
        var request = new LoginRequest { Username = entity.Username, PasswordHash = "Password.", UserType = "provider" };
        var command = new LoginCommand(request);

        // Act
        var response = await _handler.Handle(command, default);

        // Assert
        Assert.IsType<LoginResponse>(response);
        Assert.Equal(expectedResponse.ToString(), response.ToString());
    }
    
    [Fact]
    public async Task Handle_ValidAdminLogin_ReturnsLoginResponse()
    {
        var entity = _mockContext.Object.Admins.First();
        var expectedResponse = new LoginResponse { Id = entity.Id, UserType = "admin", Token = "some.jwt.token" };
        var request = new LoginRequest { Username = entity.Username, PasswordHash = "Password.", UserType = "admin" };
        var command = new LoginCommand(request);

        // Act
        var response = await _handler.Handle(command, default);

        // Assert
        Assert.IsType<LoginResponse>(response);
        Assert.Equal(expectedResponse.ToString(), response.ToString());
    }
    
    [Fact]
    public async Task Handle_ConsumerInvalidCredentials_ReturnsNull()
    {
        // Arrange
        var entity = _mockContext.Object.Consumers.First();
        var request = new LoginRequest { Username = entity.Username, PasswordHash = "invalid_password", UserType = "consumer" };
        var command = new LoginCommand(request);

        // Act
        var response = await _handler.Handle(command, default);

        // Assert
        Assert.Null(response);
    }
    
    [Fact]
    public async Task Handle_ProviderInvalidCredentials_ReturnsNull()
    {
        // Arrange
        var entity = _mockContext.Object.Providers.First();
        var request = new LoginRequest { Username = entity.Username, PasswordHash = "invalid_password", UserType = "provider" };
        var command = new LoginCommand(request);

        // Act
        var response = await _handler.Handle(command, default);

        // Assert
        Assert.Null(response);
    }
    
    [Fact]
    public async Task Handle_AdminInvalidCredentials_ReturnsNull()
    {
        // Arrange
        var entity = _mockContext.Object.Admins.First();
        var request = new LoginRequest { Username = entity.Username, PasswordHash = "invalid_password", UserType = "admin" };
        var command = new LoginCommand(request);

        // Act
        var response = await _handler.Handle(command, default);

        // Assert
        Assert.Null(response);
    }
    
    [Fact]
    public async Task Handle_InvalidUserType()
    {
        // Arrange
        var request = new LoginRequest { Username = "invalid_username", PasswordHash = "invalid_password", UserType = "invalid_usertype" };
        var command = new LoginCommand(request);

        // Act
        var response = await _handler.Handle(command, default);

        // Assert
        Assert.Null(response);
    }

    [Fact]
    public async Task Handle_NullUsernamePasswordOrUserType_ThrowsArgumentNullException()
    {
        // Arrange
        var request = new LoginRequest { Username = null, PasswordHash = "password", UserType = "consumer" };
        var command = new LoginCommand(request);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(command, default));

        // Assert
        Assert.Equal("Value cannot be null. (Parameter 'request')", ex.Message);
    }
 
}