using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UCABPagaloTodoMS.Application.Commands.Services;
using UCABPagaloTodoMS.Application.Exceptions;
using UCABPagaloTodoMS.Application.Queries;
using UCABPagaloTodoMS.Application.Queries.Debtors;
using UCABPagaloTodoMS.Application.Queries.Services;
using UCABPagaloTodoMS.Application.Requests;
using UCABPagaloTodoMS.Application.Responses;
using UCABPagaloTodoMS.Application.Validators;
using UCABPagaloTodoMS.Authorization;
using UCABPagaloTodoMS.Base;

namespace UCABPagaloTodoMS.Controllers;

[ApiController]
[Route("[controller]")]
public class ServicesController : BaseController<ServicesController>
{
    private readonly IMediator _mediator;

    public ServicesController(ILogger<ServicesController> logger, IMediator mediator) : base(logger)
    {
        _mediator = mediator;
    }

    /// <summary>
    ///     Endpoint para la consulta de servicios
    /// </summary>
    /// <remarks>
    ///     ## Description
    ///     ### Get servicios
    ///     ## Url
    ///     GET /services
    /// </remarks>
    /// <response code="200">
    ///     Accepted:
    ///     - Operation successful.
    /// </response>
    /// <returns>Retorna la lista de servicios.</returns>
    [Authorize(Policy = AuthorizationPolicies.AllPolicies )]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<ServiceResponse>>> GetServices()
    {
        _logger.LogInformation("Entrando al método que consulta los servicios");
        try
        {
            var query = new GetServicesQuery();
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (CustomException ex)
        {
            _logger.LogError("Ocurrio un error en la consulta de los servicios. Exception: " + ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Ocurrio un error en la consulta de los servicios. Exception: " + ex.Message);
            return BadRequest(ex.Message + ex.InnerException?.Message);
        }
    }

    /// <summary>
    ///     Endpoint para la consulta de servicios por id
    /// </summary>
    /// <remarks>
    ///     ## Description
    ///     ### Get servicios
    ///     ## Url
    ///     GET /services/{id}
    /// </remarks>
    /// <response code="200">
    ///     Accepted:
    ///     - Operation successful.
    /// </response>
    /// <returns>Retorna el servicio con el id que se paso.</returns>
    [Authorize(Policy = AuthorizationPolicies.AllPolicies )]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<ServiceResponse>>> GetServiceById(Guid id)
    {
        _logger.LogInformation("Entrando al método que consulta los servicios dado el id");
        try
        {
            var query = new GetServiceByIdQuery(id);
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (CustomException ex)
        {
            _logger.LogError("Ocurrio un error en la consulta de los servicios dado el id. Exception: " + ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Ocurrio un error en la consulta de los servicios dado el id. Exception: " + ex.Message);
            return BadRequest(ex.Message + ex.InnerException?.Message);
        }
    }

    /// <summary>
    ///     Endpoint para la consulta de servicios por prestador
    /// </summary>
    /// <remarks>
    ///     ## Description
    ///     ### Get servicios por prestador
    ///     ## Url
    ///     GET /services/provider/{id}
    /// </remarks>
    /// <response code="200">
    ///     Accepted:
    ///     - Operation successful.
    /// </response>
    /// <returns>Retorna los servicios del prestador.</returns>
    [Authorize(Policy = AuthorizationPolicies.AllPolicies )]
    [HttpGet("Provider/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<ServiceResponse>>> GetServiceByProviderId(Guid id)
    {
        _logger.LogInformation("Entrando al método que consulta los servicios dado el id de un prestador");
        try
        {
            var query = new GetServicesByProviderIdQuery(id);
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (CustomException ex)
        {
            _logger.LogError("Ocurrio un error en la consulta de los servicios dado el id. Exception: " + ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Ocurrio un error en la consulta de los servicios dado el id. Exception: " + ex.Message);
            return BadRequest(ex.Message + ex.InnerException?.Message);
        }
    }

    /// <summary>
    ///     Endpoint que registra un servicio.
    /// </summary>
    /// <remarks>
    ///     ## Description
    ///     ### Post registra servicio.
    ///     ## Url
    ///     POST /services
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
    public async Task<ActionResult<Guid>> CreateService(ServiceRequest request)
    {
        _logger.LogInformation("Entrando al método que registra los servicios");
        try
        {
            AddServiceValidator validator = new AddServiceValidator();
            validator.ValidateAndThrow(request);
            var query = new CreateServiceCommand(request);
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (CustomException ex)
        {
            _logger.LogError("Ocurrio un error al intentar registrar un servicio. Exception: " + ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Ocurrio un error al intentar registrar un servicio. Exception: " + ex.Message);
            return BadRequest(ex.Message + ex.InnerException?.Message);
        }
    }


    /// <summary>
    ///     Endpoint que actualiza un servicio.
    /// </summary>
    /// <remarks>
    ///     ## Description
    ///     ### Put actualiza un servicio.
    ///     ## Url
    ///     PUT /services/{id}
    /// </remarks>
    /// <response code="200">
    ///     Accepted:
    ///     - Operation successful.
    /// </response>
    /// <returns>Retorna el regisrto modificado.</returns>
    [Authorize(Policy = AuthorizationPolicies.AdminPolicy )]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ServiceResponse>> UpdateService([FromBody] ServiceRequest request, Guid id)
    {
        _logger.LogInformation("Entrando al método que actualiza un servicio");
        try
        {
            AddServiceValidator validator = new AddServiceValidator();
            validator.ValidateAndThrow(request);
            var query = new UpdateServiceCommand(request, id);
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (CustomException ex)
        {
            _logger.LogError("Ocurrio un error al intentar actualizar un servicio. Exception: " + ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Ocurrio un error al intentar actualizar un servicio. Exception: " + ex.Message);
            return BadRequest(ex.Message + ex.InnerException?.Message);
        }
    }

    /// <summary>
    ///     Endpoint que elimina un servicio.
    /// </summary>
    /// <remarks>
    ///     ## Description
    ///     ### Delete elimina un servicio.
    ///     ## Url
    ///     DELETE /services/{id}
    /// </remarks>
    /// <response code="200">
    ///     Accepted:
    ///     - Operation successful.
    /// </response>
    /// <returns>Retorna el id del registro modificado.</returns>
    [Authorize(Policy = AuthorizationPolicies.AdminPolicy )]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> DeleteService(Guid id)
    {
        _logger.LogInformation("Entrando al método que elimina un servicio");
        try
        {
            var query = new DeleteServiceCommand(id);
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (CustomException ex)
        {
            _logger.LogError("Ocurrio un error al intentar eliminar un servicio. Exception: " + ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Ocurrio un error al intentar eliminar un servicio. Exception: " + ex.Message);
            return BadRequest(ex.Message + ex.InnerException?.Message);
        }
    }

    /// <summary>
    ///     Endpoint que actualiza los campos de un servicio.
    /// </summary>
    /// <remarks>
    ///     ## Description
    ///     ### Put actualiza los campos de un servicio.
    ///     ## Url
    ///     PUT /services/{id}/fields
    /// </remarks>
    /// <response code="200">
    ///     Accepted:
    ///     - Operation successful.
    /// </response>
    /// <returns>Retorna la lista de campos modificados.</returns>
    [Authorize(Policy = AuthorizationPolicies.AdminPolicy )]
    [HttpPut("fields/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<FieldResponse>>> UpdateField([FromBody] FieldRequest request, Guid id)
    {
        _logger.LogInformation("Entrando al método que actualiza los campos de un servicio");
        try
        {
            FieldValidator validator = new FieldValidator();
            validator.ValidateAndThrow(request);
            var query = new UpdateFieldCommand(request, id);
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (CustomException ex)
        {
            _logger.LogError("Ocurrio un error al intentar actualizar los campos de un servicio. Exception: " + ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Ocurrio un error al intentar actualizar los campos de un servicio. Exception: " + ex.Message);
            return BadRequest(ex.Message + ex.InnerException?.Message);
        }
    }


    /// <summary>
    ///     Endpoint que registra un formato de archivo de conciliacion
    /// </summary>
    /// <remarks>
    ///     ## Description
    ///     ### Post registra campos de archivo de conciliacion.
    ///     ## Url
    ///     POST /format
    /// </remarks>
    /// <response code="200">
    ///     Accepted:
    ///     - Operation successful.
    /// </response>
    /// <returns>Retorna la lista de los ids de los campo .</returns>
    [Authorize(Policy = AuthorizationPolicies.AdminPolicy )]
    [HttpPost("format")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<Guid>>> CreateFormat(List<FieldRequest> fieldsRequests)
    {
        _logger.LogInformation("Entrando al método que registra el formato de conciliacion dado un servicio");
        try
        {
            FieldValidator validator = new FieldValidator();
            foreach (var fieldRequest in fieldsRequests)
            {
                validator.ValidateAndThrow(fieldRequest);
            }

            List<Guid> responsesList = new();
            foreach (var fieldRequest in fieldsRequests)
            {
                var query = new CreateFieldCommand(fieldRequest);
                responsesList.Add((await _mediator.Send(query)));
            }

            return Ok(responsesList);
        }
        catch (CustomException ex)
        {
            _logger.LogError("Ocurrio un error al intentar registrar un campo. Exception: " + ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Ocurrio un error al intentar registrar un campo. Exception: " + ex.Message);
            return BadRequest(ex.Message + ex.InnerException?.Message);
        }
    }
    
    /// <summary>
    ///     Endpoint para la consulta de field por id
    /// </summary>
    /// <remarks>
    ///     ## Description
    ///     ### Get field by id
    ///     ## Url
    ///     GET /fields/{id}
    /// </remarks>
    /// <response code="200">
    ///     Accepted:
    ///     - Operation successful.
    /// </response>
    /// <returns>Retorna el field con el id que se paso.</returns>
    [Authorize(Policy = AuthorizationPolicies.AdminOrProviderPolicy )]
    [HttpGet("fields/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FieldResponse>> GetFieldById(Guid id)
    {
        _logger.LogInformation("Entrando al método que consultas los fields con el id");
        try
        {
            var query = new GetFieldByIdQuery(id);
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (CustomException ex)
        {
            _logger.LogError("Ocurrio un error en la consulta de un field dado el id. Exception: " + ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Ocurrio un error en la consulta de un field dado el id. Exception: " + ex.Message);
            return BadRequest(ex.Message + ex.InnerException?.Message);
        }
    }
    
    // [Authorize(Policy = AuthorizationPolicies.ProviderPolicy)]
    [HttpPost("{id:guid}/debtors")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> UploadConfirmationList(Guid id, IFormFile file)
    {
        _logger.LogInformation("Entrando al método que recibe la lista de deudores de un servicio");
        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            var data = stream.ToArray();
            var request = new UploadDebtorsRequest(data, id);
            return await _mediator.Send(request)
                ? Ok("El archivo fue agregado a la cola")
                : BadRequest("Hubo un error agregando el archivo a la cola");
        }
        catch (CustomException e)
        {
            _logger.LogError("Ocurrio un error inesperado" + e.Message);
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Ocurrio un error inesperado" + e.Message);
            return BadRequest(e.Message + e.InnerException?.Message);
        }
        
    }
    
    /// <summary>
    ///     Endpoint para la consulta de deudores de un servicio
    /// </summary>
    /// <remarks>
    ///     ## Description
    ///     ### Get field by id
    ///     ## Url
    ///     GET /fields/{id}
    /// </remarks>
    /// <response code="200">
    ///     Accepted:
    ///     - Operation successful.
    /// </response>
    /// <returns>Retorna el field con el id que se paso.</returns>
    [Authorize(Policy = AuthorizationPolicies.AdminOrProviderPolicy )]
    [HttpGet("debtors/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<DebtorsResponse>>> GetDebtorsByServiceIdQuery(Guid id)
    {
        _logger.LogInformation("Entrando al método que consulta los deudores dado el id de un servicio");
        try
        {
            var query = new GetDebtorsByServiceIdQuery(id);
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (CustomException ex)
        {
            _logger.LogError("Ocurrio un error en la consulta de los deudores dado el id del servicio. Exception: " + ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Ocurrio un error en la consulta de los deudores dado el id del servicio. Exception: " + ex.Message);
            return BadRequest(ex.Message + ex.InnerException?.Message);
        }
    }
    
}