using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UCABPagaloTodoMS.Application.Commands;
using UCABPagaloTodoMS.Application.Exceptions;
using UCABPagaloTodoMS.Application.Responses;
using UCABPagaloTodoMS.Core.Database;
using UCABPagaloTodoMS.Core.Entities;
using UCABPagaloTodoMS.Infrastructure.Utils;

namespace UCABPagaloTodoMS.Application.Handlers.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUCABPagaloTodoDbContext _dbContext;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(IUCABPagaloTodoDbContext dbContext, ILogger<LoginCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    /// <summary>
    /// Maneja una solicitud de inicio de sesión. Verifica que la solicitud no sea nula y llama al método HandleAsync para autenticar al usuario.
    /// Lanza una excepción de tipo ArgumentNullException si la solicitud es nula.
    /// </summary>
    /// <param name="request">La solicitud de inicio de sesión.</param>
    /// <param name="cancellationToken">El token de cancelación.</param>
    /// <returns>Un objeto LoginResponse que contiene el tipo de usuario y el ID del usuario autenticado.</returns>
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.Request == null)
            {
                _logger.LogWarning("LoginCommandHandler.Handle: Request nulo.");
                throw new ArgumentNullException(nameof(request));
            }
            return await HandleAsync(request);
        }
        catch (HttpRequestException)
        {
            throw; // Si ya es una excepción de tipo HttpRequestException, simplemente relanzarla
        }
        catch (ArgumentNullException)
        {
            throw; // Si ya es una excepción de tipo ArgumentNullException, simplemente relanzarla
        }
        catch (Exception e)
        {
            throw new CustomException(e);
        }
    }

    /// <summary>
    /// Autentica al usuario y devuelve un objeto LoginResponse que contiene el tipo de usuario y el ID del usuario autenticado.
    /// Lanza una excepción de tipo HttpRequestException con un mensaje de error y un código de estado HTTP si la cuenta del usuario está inactiva o las credenciales son inválidas.
    /// Lanza una excepción de tipo CustomException si se produce algún otro tipo de error y lo registra en el log de la aplicación.
    /// </summary>
    /// <param name="request">La solicitud de inicio de sesión.</param>
    /// <returns>Un objeto LoginResponse que contiene el tipo de usuario y el ID del usuario autenticado.</returns>
    private async Task<LoginResponse> HandleAsync(LoginCommand request)
    {
        try
        {
            // Verificar que se proporcione el nombre de usuario, la contraseña y el tipo de usuario en la solicitud
            if (request.Request.Username == null || request.Request.PasswordHash == null || request.Request.UserType == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var userType = request.Request.UserType.ToUpper();
            UserEntity? user;
            switch (userType)
            {
                case "ADMIN":
                    user = await _dbContext.Admins.SingleOrDefaultAsync(a => a.Username == request.Request.Username);
                    break;
                case "CONSUMER":
                    user = await _dbContext.Consumers.SingleOrDefaultAsync(a => a.Username == request.Request.Username);
                    break;
                case "PROVIDER":
                    user = await _dbContext.Providers.SingleOrDefaultAsync(a => a.Username == request.Request.Username);
                    break;
                default:
                    _logger.LogWarning($"El tipo de usuario {userType} no es válido.");
                    user = null;
                    break;
            }

            if (user == null || !SecurePasswordHasher.Verify(request.Request.PasswordHash, user.PasswordHash))
            {
                throw new HttpRequestException("Las credenciales de inicio de sesión son inválidas.", null, HttpStatusCode.BadRequest);
            }

            if (user.Status == false)
            {
                throw new HttpRequestException($"La cuenta del {userType.ToLower()} está inactiva.", null, HttpStatusCode.Unauthorized);
            }

            _logger.LogInformation($"El {userType.ToLower()} {request.Request.Username} inició sesión con éxito.");

            return new LoginResponse { UserType = userType.ToLower(), Id = user.Id };
        }
        catch (HttpRequestException)
        {
            throw; // Si ya es una excepción de tipo HttpRequestException, simplemente relanzarla
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ocurrió un error al autenticar al usuario {request.Request.Username}.");
            throw new CustomException($"Ocurrió un error al autenticar al usuario: {ex.Message}", ex);
        }
    }

}