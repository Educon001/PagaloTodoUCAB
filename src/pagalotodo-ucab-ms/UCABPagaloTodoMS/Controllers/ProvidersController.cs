﻿using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UCABPagaloTodoMS.Application.Commands;
using UCABPagaloTodoMS.Application.Exceptions;
using UCABPagaloTodoMS.Application.Queries.Providers;
using UCABPagaloTodoMS.Application.Requests;
using UCABPagaloTodoMS.Application.Responses;
using UCABPagaloTodoMS.Authorization;
using UCABPagaloTodoMS.Base;

namespace UCABPagaloTodoMS.Controllers;

[ApiController]
[Route("[controller]")]
public class ProvidersController : BaseController<ProvidersController>
    {
        private readonly IMediator _mediator;

        public ProvidersController(ILogger<ProvidersController> logger, IMediator mediator) : base(logger)
        {
            _mediator = mediator;
        }

        /// <summary>
        ///     Endpoint para la consulta de prestadores
        /// </summary>
        /// <remarks>
        ///     ## Description
        ///     ### Get prestadores
        ///     ## Url
        ///     GET /providers
        /// </remarks>
        /// <response code="200">
        ///     Accepted:
        ///     - Operation successful.
        /// </response>
        /// <returns>Retorna la lista de prestadores.</returns>
        [Authorize(Policy = AuthorizationPolicies.AdminOrConsumerPolicy )]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<ProviderResponse>>> GetProviders()
        {
            _logger.LogInformation("Entrando al método que consulta los prestadores");
            try
            {
                var query = new GetProvidersQuery();
                var response = await _mediator.Send(query);
                return Ok(response);
            }
            catch (CustomException ex)
            {
                _logger.LogError("Ocurrio un error en la consulta de los prestadores. Exception: " + ex);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Ocurrio un error en la consulta de los prestadores. Exception: " + ex);
                return BadRequest(ex.Message + ex.InnerException?.Message);
            }
        }
        
        /// <summary>
        ///     Endpoint para la consulta de prestadores
        /// </summary>
        /// <remarks>
        ///     ## Description
        ///     ### Get prestadores
        ///     ## Url
        ///     GET /providers
        /// </remarks>
        /// <response code="200">
        ///     Accepted:
        ///     - Operation successful.
        /// </response>
        /// <returns>Retorna la lista de prestadores.</returns>
        [Authorize(Policy = AuthorizationPolicies.AdminOrProviderPolicy)]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProviderResponse>> GetProviderById(Guid id)
        {
            _logger.LogInformation("Entrando al método que consulta los prestadores");
            try
            {
                var query = new GetProviderByIdQuery(id);
                var response = await _mediator.Send(query);
                return Ok(response);
            }
            catch (CustomException ex)
            {
                _logger.LogError("Ocurrio un error en la consulta de los prestadores. Exception: " + ex);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Ocurrio un error en la consulta de los prestadores. Exception: " + ex);
                return BadRequest(ex.Message + ex.InnerException?.Message);
            }
        }

        /// <summary>
        ///     Endpoint para la consulta de prestadores con sus servicios
        /// </summary>
        /// <remarks>
        ///     ## Description
        ///     ### Get prestadores con servicios
        ///     ## Url
        ///     GET /providers/services
        /// </remarks>
        /// <response code="200">
        ///     Accepted:
        ///     - Operation successful.
        /// </response>
        /// <returns>Retorna la lista de prestadores con sus servicios.</returns>
        [Authorize(Policy = AuthorizationPolicies.AdminOrConsumerPolicy)]
        [HttpGet("services")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<ProviderResponse>>> GetProvidersWithServices()
        {
            _logger.LogInformation("Entrando al método que consulta los prestadores con sus servicios");
            try
            {
                var query = new GetProvidersWithServicesQuery();
                var response = await _mediator.Send(query);
                return Ok(response);
            }
            catch (CustomException ex)
            {
                _logger.LogError("Ocurrio un error en la consulta de los prestadores con sus servicios. Exception: " + ex);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Ocurrio un error en la consulta de los prestadores con sus servicios. Exception: " + ex);
                return BadRequest(ex.Message + ex.InnerException?.Message);
            }
        }

        /// <summary>
        ///     Endpoint que registra un prestador.
        /// </summary>
        /// <remarks>
        ///     ## Description
        ///     ### Post prestador.
        ///     ## Url
        ///     POST /providers
        /// </remarks>
        /// <response code="200">
        ///     Accepted:
        ///     - Operation successful.
        /// </response>
        /// <returns>Retorna el id del nuevo registro.</returns>
        [Authorize(Policy = AuthorizationPolicies.AdminPolicy )]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> PostProvider(ProviderRequest provider)
        {
            _logger.LogInformation("Entrando al método que registra los prestadores");
            try
            {
                var query = new CreateProviderCommand(provider);
                var response = await _mediator.Send(query);
                return Ok(response);
            }
            catch (CustomException ex)
            {
                _logger.LogError("Ocurrio un error al intentar registrar un prestador. Exception: " + ex);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Ocurrio un error al intentar registrar un prestador. Exception: " + ex);
                return BadRequest(ex.Message + ex.InnerException?.Message);
            }
        }
        
        /// <summary>
        ///     Endpoint que elimina un prestador.
        /// </summary>
        /// <remarks>
        ///     ## Description
        ///     ### Eliminar prestador.
        ///     ## Url
        ///     DELETE /providers
        /// </remarks>
        /// <response code="200">
        ///     Accepted:
        ///     - Operation successful.
        /// </response>
        /// <returns>Retorna el id del objeto eliminado</returns>
        [Authorize(Policy = AuthorizationPolicies.AdminPolicy )]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> DeleteProvider(Guid id)
        {
            _logger.LogInformation("Entrando al método que elimina un prestador");
            try
            {
                var query = new DeleteProviderCommand(id);
                var response = await _mediator.Send(query);
                return Ok(response);
            }
            catch (CustomException ex)
            {
                _logger.LogError("Ocurrio un error al intentar registrar un prestador. Exception: " + ex);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Ocurrio un error al intentar registrar un prestador. Exception: " + ex);
                return BadRequest(ex.Message + ex.InnerException?.Message);
            }
        }
        
        /// <summary>
        ///     Endpoint que actualiza un prestador.
        /// </summary>
        /// <remarks>
        ///     ## Description
        ///     ### Actualizar prestador.
        ///     ## Url
        ///     PUT /providers
        /// </remarks>
        /// <response code="200">
        ///     Accepted:
        ///     - Operation successful.
        /// </response>
        /// <returns>Retorna el objeto actualizado</returns>
        [Authorize(Policy = AuthorizationPolicies.AdminPolicy )]
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProviderResponse>> UpdateProvider(Guid id, [FromBody]ProviderRequest provider)
        {
            _logger.LogInformation("Entrando al método que registra los prestadores");
            try
            {
                var query = new UpdateProviderCommand(provider,id);
                var response = await _mediator.Send(query);
                return Ok(response);
            }
            catch (CustomException ex)
            {
                _logger.LogError("Ocurrio un error al intentar registrar un prestador. Exception: " + ex);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Ocurrio un error al intentar registrar un prestador. Exception: " + ex);
                return BadRequest(ex.Message + ex.InnerException?.Message);
            }
        }
        
        ///<summary>
        ///     Endpoint que actualiza la contraseña del prestador
        /// </summary>
        /// <remarks>
        ///     ## Description
        ///     ### actualizar contraseña
        ///     ## Url
        ///     PUT /{id}/password
        /// </remarks>
        /// <response code="200">
        ///     Accepted:
        ///     - Operation successful.
        /// </response>
        /// <returns>Retorna un mensaje de exito</returns>
        [Authorize(Policy = "ProviderPolicy" )]
        [HttpPut("{id:guid}/password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UpdatePasswordResponse>> UpdatePassword(Guid id, [FromBody]UpdatePasswordRequest request)
        {
            _logger.LogInformation("Entrando al método que cambia clave a provider");
            try
            {
                request.UserType = "provider";
                var query = new UpdatePasswordCommand(request,id);
                var response = await _mediator.Send(query);
                return Ok(response);
            }
            catch (CustomException ex)
            {
                _logger.LogError("Ocurrio un error al cambiar la clave del consumer. Exception: " + ex);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Ocurrio un error al cambiar la clave del consumer. Exception: " + ex);
                return BadRequest("Error al cambiar la clave del consumer.");
            }
        }
        
        /// <summary>
        ///     Endpoint que carga el archivo de resultado de conciliación
        /// </summary>
        /// <remarks>
        ///     ## Description
        ///     ### cargar resultado de conciliación
        ///     ## Url
        ///     POST /uploadConciliation
        /// </remarks>
        /// <response code="200">
        ///     Accepted:
        ///     - Operation successful.
        /// </response>
        /// <returns>Retorna la cantidad de archivos cargados</returns>
        //[Authorize(Policy = AuthorizationPolicies.ProviderPolicy)]
        [HttpPost("uploadConciliation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> UploadConciliationResponse(List<IFormFile> files)
        {
            _logger.LogInformation("Entrando al método que recibe la respuesta de conciliacion");
            try
            {
                var success = 0;
                var errors = 0;
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        using var stream = new MemoryStream();
                        await file.CopyToAsync(stream);
                        var data = stream.ToArray();
                        var request = new UploadConciliationResponseRequest(data);
                        var response = await _mediator.Send(request);
                        var i = response ? success++ : errors++;
                    }
                }
                return Ok($"Se cargaron {success} archivos a la cola. Errores: {errors}");
            }
            catch (CustomException e)
            {
                _logger.LogError("Ocurrio un error inesperado. Exception: " + e);
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError("Ocurrio un error inesperado. Exception: " + e);
                return BadRequest(e.Message + e.InnerException?.Message);
            }
        }
    }