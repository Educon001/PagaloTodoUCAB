using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Moq;
using UCABPagaloTodoMS.Application.Exceptions;
using UCABPagaloTodoMS.Application.Handlers.Queries.Payments;
using UCABPagaloTodoMS.Application.Mappers;
using UCABPagaloTodoMS.Application.Queries.Payments;
using UCABPagaloTodoMS.Application.Responses;
using UCABPagaloTodoMS.Core.Database;
using Xunit;

namespace UCABPagaloTodoMS.Tests.UnitTestsApplication.Handlers.Queries.Payments;

[ExcludeFromCodeCoverage]
public class GetPaymentsQueryHandlerTest
{
    private readonly GetPaymentsQueryHandler _handler;
    private readonly Mock<ILogger<GetPaymentsQueryHandler>> _loggerMock;
    private readonly Mock<IUCABPagaloTodoDbContext> _mockContext;

    public GetPaymentsQueryHandlerTest()
    {
        _loggerMock = new Mock<ILogger<GetPaymentsQueryHandler>>();
        _mockContext = new Mock<IUCABPagaloTodoDbContext>();
        _handler = new GetPaymentsQueryHandler(_mockContext.Object, _loggerMock.Object);
        DataSeed.DataSeed.SetupDbContextData(_mockContext);
    }

    /// <summary>
    ///     Prueba de handler con respuesta Ok
    /// </summary>
    [Fact]
    public async void GetPaymentsQueryHandle_Returns_List()
    {
        var expectedResponse = _mockContext.Object.Payments
            .Select(p => PaymentMapper.MapEntityToResponse(p, false, false)).ToList();
        var query = new GetPaymentsQuery(null, null);
        var response = await _handler.Handle(query, default);
        Assert.IsType<List<PaymentResponse>>(response);
        Assert.Equal(expectedResponse.ToString(), response.ToString());
    }


    /// <summary>
    ///     Prueba de handler con excepción en HandleAsync
    /// </summary>
    [Fact]
    public async void GetPaymentsQueryHandle_HandleAsyncException()
    {
        var expectedException = new Exception("Test Exception");
        _mockContext.Setup(c => c.Payments).Throws(expectedException);
        var query = new GetPaymentsQuery(null, null);
        var result = await Assert.ThrowsAnyAsync<Exception>(() => _handler.Handle(query, default));
        Assert.Equal(expectedException.Message, result.Message);
    }

    /// <summary>
    ///     Prueba de handler con request nulo
    /// </summary>
    [Fact]
    public async void GetPaymentsQueryHandle_ArgumentNullException()
    {
        var result = await Assert.ThrowsAsync<CustomException>(() => _handler.Handle(null, default));
        Assert.IsType<ArgumentNullException>(result.InnerException);
    }
}