using UCABPagaloTodoMS.Core.Database;
using MediatR;
using Microsoft.Extensions.Logging;
using UCABPagaloTodoMS.Application.Queries;
using UCABPagaloTodoMS.Application.Responses;
using Microsoft.EntityFrameworkCore;
using UCABPagaloTodoMS.Application.Exceptions;
using UCABPagaloTodoMS.Application.Mappers;

namespace UCABPagaloTodoMS.Application.Handlers.Queries;

public class ConsumersQueryHandler : IRequestHandler<ConsumersQuery, List<ConsumerResponse>>
{
    private readonly IUCABPagaloTodoDbContext _dbContext;
    private readonly ILogger<ConsumersQueryHandler> _logger;

    public ConsumersQueryHandler(IUCABPagaloTodoDbContext dbContext,
        ILogger<ConsumersQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public Task<List<ConsumerResponse>> Handle(ConsumersQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogWarning("ConsumersQueryHandler.Handle: Request nulo.");
                throw new ArgumentNullException(nameof(request));
            }
            return HandleAsync();
        }
        catch (Exception e)
        {
            throw new CustomException(e);
        }
    }

    /// <summary>
    /// Handles the retrieval of all consumers from the database.
    /// </summary>
    /// <returns>A list of all consumers retrieved from the database.</returns>
    private async Task<List<ConsumerResponse>> HandleAsync()
    {
        try
        {
            _logger.LogInformation("ConsumersQueryHandler.HandleAsync");

            var result = _dbContext.Consumers.Where(c=>c.IsDeleted==false).Select(c => ConsumerMapper.MapEntityToResponse(c));

            return await result.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ConsumersQueryHandler.HandleAsync. {Mensaje}", ex.Message);
            throw new CustomException(ex);;
        }
    }
}