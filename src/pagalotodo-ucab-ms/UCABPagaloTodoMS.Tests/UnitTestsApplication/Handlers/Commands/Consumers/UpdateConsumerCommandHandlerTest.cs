﻿using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using UCABPagaloTodoMS.Application.Commands;
using UCABPagaloTodoMS.Application.Exceptions;
using UCABPagaloTodoMS.Application.Handlers.Commands;
using UCABPagaloTodoMS.Application.Mappers;
using UCABPagaloTodoMS.Application.Requests;
using UCABPagaloTodoMS.Application.Responses;
using UCABPagaloTodoMS.Core.Database;
using Xunit;

namespace UCABPagaloTodoMS.Tests.UnitTestsApplication.Handlers.Commands.Consumers;

[ExcludeFromCodeCoverage]
public class UpdateConsumerCommandHandlerTest
{
    private readonly UpdateConsumerCommandHandler _handler;
    private readonly Mock<ILogger<UpdateConsumerCommandHandler>> _loggerMock;
    private readonly Mock<IUCABPagaloTodoDbContext> _mockContext;
    private readonly Mock<IDbContextTransactionProxy> _mockTransaction;

    public UpdateConsumerCommandHandlerTest()
    {
        _loggerMock = new Mock<ILogger<UpdateConsumerCommandHandler>>();
        _mockContext = new Mock<IUCABPagaloTodoDbContext>();
        _mockTransaction = new Mock<IDbContextTransactionProxy>();
        _handler = new UpdateConsumerCommandHandler(_mockContext.Object, _loggerMock.Object);
        DataSeed.DataSeed.SetupDbContextData(_mockContext);
        _mockContext.Setup(c => c.BeginTransaction()).Returns(_mockTransaction.Object);
    }

    [Fact]
    public async void UpdateConsumerCommandHandler_Ok_PasswordNull()
    {
        var entity = _mockContext.Object.Consumers.First();
        var expectedResponse = ConsumerMapper.MapEntityToResponse(entity);
        expectedResponse.Name = "New";
        var request = new ConsumerRequest()
        {
            Username = entity.Username,
            Email = entity.Email,
            Name = "New",
            ConsumerId = entity.ConsumerId,
            Status = entity.Status,
            LastName = "Name",
            PasswordHash = null
        };
        _mockContext.Setup(m => m.Consumers.Find(entity.Id)).Returns(entity);
        var command = new UpdateConsumerCommand(request,entity.Id);
        var response = await _handler.Handle(command, default);
        Assert.IsType<ConsumerResponse>(response);
        Assert.Equal(expectedResponse.Name, response.Name);
    }

    [Fact]
    public async void UpdateConsumerCommandHandler_Ok_PasswordNotNull()
    {
        var entity = _mockContext.Object.Consumers.First();
        var expectedResponse = ConsumerMapper.MapEntityToResponse(entity);
        expectedResponse.Name = "Name";
        var request = new ConsumerRequest()
        {
            Username = entity.Username,
            Email = entity.Email,
            Name = "Name",
            ConsumerId = entity.ConsumerId,
            Status = entity.Status,
            LastName = "Name",
            PasswordHash = entity.PasswordHash
        };
        _mockContext.Setup(m => m.Consumers.Find(entity.Id)).Returns(entity);
        var command = new UpdateConsumerCommand(request,entity.Id);
        var response = await _handler.Handle(command, default);
        Assert.IsType<ConsumerResponse>(response);
        Assert.Equal(expectedResponse.Name, response.Name);
    }
    
    [Fact]
    public async void UpdateConsumerCommandHandler_ValidationException()
    {
        var request = new ConsumerRequest()
        {
            Username = "HandlerTest"
        };
        var command = new UpdateConsumerCommand(request,new Guid());
        var result = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, default));
        Assert.IsType<ValidationException>(result.InnerException);
    }
    
    [Fact]
    public async void UpdateConsumerCommandHandle_ArgumentNullException()
    {
        var command = new UpdateConsumerCommand(null,new Guid());
        var result = await Assert.ThrowsAsync<CustomException>(() =>_handler.Handle(command, default));
        Assert.IsType<ArgumentNullException>(result.InnerException);
    }
    
    [Fact]
    public async void UpdateConsumerCommandHandler_HandleAsyncException()
    {
        var entity = _mockContext.Object.Consumers.First();
        var request = new ConsumerRequest()
        {
            Username = entity.Username,
            Email = entity.Email,
            Name = entity.Name,
            ConsumerId = entity.ConsumerId,
            Status = entity.Status,
            LastName = entity.LastName,
            PasswordHash = entity.PasswordHash
        };
        var command = new UpdateConsumerCommand(request, entity.Id);
        var result = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(command, default));
        Assert.IsType<CustomException>(result.InnerException);
        Assert.Contains($"Object with key {entity.Id} not found", result.InnerException.Message);
    }
}