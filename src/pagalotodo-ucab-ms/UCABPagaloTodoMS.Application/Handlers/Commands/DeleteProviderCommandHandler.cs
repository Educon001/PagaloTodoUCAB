﻿using MediatR;
using Microsoft.Extensions.Logging;
using UCABPagaloTodoMS.Application.Commands;
using UCABPagaloTodoMS.Application.Exceptions;
using UCABPagaloTodoMS.Core.Database;

namespace UCABPagaloTodoMS.Application.Handlers.Commands;

public class DeleteProviderCommandHandler : IRequestHandler<DeleteProviderCommand, Guid>
{
    private readonly IUCABPagaloTodoDbContext _dbContext;
    private readonly ILogger<DeleteProviderCommandHandler> _logger;

    public DeleteProviderCommandHandler(IUCABPagaloTodoDbContext dbContext,
        ILogger<DeleteProviderCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Guid> Handle(DeleteProviderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.Request == Guid.Empty)
            {
                _logger.LogWarning("DeleteProviderCommandHandler.Handle: Request nulo.");
                throw new ArgumentNullException(nameof(request));
            }
            return await HandleAsync(request);
        }
        catch (Exception e)
        {
            throw new CustomException(e);
        }
    }

    /// <summary>
    /// Handles the deletion of a provider from the database.
    /// </summary>
    /// <param name="request">The request containing the ID of the provider to delete.</param>
    /// <returns>The ID of the deleted provider.</returns>
    private async Task<Guid> HandleAsync(DeleteProviderCommand request)
    {
        var transaccion = _dbContext.BeginTransaction();
        try
        {
            _logger.LogInformation("DeleteProviderCommandHandler.HandleAsync {Request}", request);
            var providerId = request.Request;
            var entity = _dbContext.Providers.Find(providerId);
            if (entity!=null)
            {
                _dbContext.Providers.Remove(entity);
            }
            else
            {
                throw new KeyNotFoundException($"Object with key {providerId} not found");
            }
            await _dbContext.SaveEfContextChanges("APP");
            transaccion.Commit();
            _logger.LogInformation("DeleteProviderCommandHandler.HandleAsync {Response}", providerId);
            return providerId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error DeleteProviderCommandHandler.HandleAsync. {Mensaje}", ex.Message);
            transaccion.Rollback();
            throw;
        }
    }
}