using MediatR;
using Microsoft.Extensions.Logging;
using UCABPagaloTodoMS.Application.Commands.Services;
using UCABPagaloTodoMS.Application.Exceptions;
using UCABPagaloTodoMS.Core.Database;

namespace UCABPagaloTodoMS.Application.Handlers.Commands.Services;

public class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, Guid>
{
    private readonly IUCABPagaloTodoDbContext _dbContext;
    private readonly ILogger<DeleteServiceCommandHandler> _logger;

    public DeleteServiceCommandHandler(IUCABPagaloTodoDbContext dbContext, ILogger<DeleteServiceCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Guid> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return await HandleAsync(request);
        }
        catch (Exception e)
        {
            throw new CustomException(e);
        }
    }

    /// <summary>
    /// Handles the deletion of a service from the database.
    /// </summary>
    /// <param name="request">The request containing the ID of the service to delete.</param>
    /// <returns>The ID of the deleted service.</returns>
    private async Task<Guid> HandleAsync(DeleteServiceCommand request)
    {
        var transaccion = _dbContext.BeginTransaction();
        try
        {
            _logger.LogInformation("DeleteServiceCommandHandler.HandleAsync {Request}", request);
            var entityId = request.Id;
            var entity = _dbContext.Services.Find(entityId);
            if (entity is not null)
            {
                _dbContext.Services.Remove(entity);
            }
            else
            {
                throw new Exception($"Servicio {entityId} no se encontro en la base de datos");
            }

            await _dbContext.SaveEfContextChanges("APP");
            transaccion.Commit();
            _logger.LogInformation("DeleteServiceCommandHandler.HandleAsync {Response}", entityId);
            return entityId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error DeleteServiceCommandHandler.HandleAsync. {Mensaje}", ex.Message);
            transaccion.Rollback();
            throw;
        }
    }
}