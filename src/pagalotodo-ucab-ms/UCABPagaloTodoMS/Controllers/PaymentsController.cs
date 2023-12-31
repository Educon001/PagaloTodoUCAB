using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UCABPagaloTodoMS.Application.Commands.Payments;
using UCABPagaloTodoMS.Application.Queries.Payments;
using UCABPagaloTodoMS.Application.Queries.Services;
using UCABPagaloTodoMS.Application.Requests;
using UCABPagaloTodoMS.Application.Responses;
using UCABPagaloTodoMS.Authorization;
using UCABPagaloTodoMS.Base;
using UCABPagaloTodoMS.Core.Enums;
using System.Linq;
using UCABPagaloTodoMS.Application.Exceptions;

namespace UCABPagaloTodoMS.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentsController : BaseController<PaymentsController>
{
    private readonly IMediator _mediator;

    public PaymentsController(ILogger<PaymentsController> logger, IMediator mediator) : base(logger)
    {
        _mediator = mediator;
    }

    /// <summary>
    ///     Endpoint para consultar los pagos por servicio
    /// </summary>
    /// <remarks>
    ///     ## Description
    ///     ### Get pagos
    ///     ## Url
    ///     GET /payments
    /// </remarks>
    /// <response code="200">
    ///     Accepted:
    ///     - Operation successful.
    /// </response>
    /// <returns>Retorna la lista de todos los pagos</returns>
    [Authorize(Policy = AuthorizationPolicies.AdminPolicy)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> GetPayments([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        _logger.LogInformation("Entrando al método que consulta los pagos");
        try
        {
            var query = new GetPaymentsQuery(startDate, endDate);
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (CustomException ex)
        {
            _logger.LogError($"Ocurrio un error al consultar los pagos. Exception: " + ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ocurrio un error al consultar los pagos. Exception: " + ex.Message);
            return BadRequest(ex.Message + ex.InnerException?.Message);
        }
    }

    /// <summary>
    ///     Endpoint para consultar los pagos por Consumidor
    /// </summary>
    /// <remarks>
    ///     ## Description
    ///     ### Get pagos por consumidor
    ///     ## Url
    ///     GET /payments/Consumer/{id}
    /// </remarks>
    /// <response code="200">
    ///     Accepted:
    ///     - Operation successful.
    /// </response>
    /// <returns>Retorna la lista de todos los pagos de un consumidor</returns>
    [Authorize(Policy = AuthorizationPolicies.ConsumerPolicy)]
    [HttpGet("Consumer/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<PaymentResponse>>> GetPaymentsByConsumerId(Guid id,
        [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        _logger.LogInformation("Entrando al método que consulta los pagos de un consumidor");
        try
        {
            var query = new GetPaymentsByConsumerIdQuery(id, startDate, endDate);
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (CustomException ex)
        {
            _logger.LogError($"Ocurrio un error al consultar los pagos del consumidor {id}. Exception: " + ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ocurrio un error al consultar los pagos del consumidor {id}. Exception: " + ex.Message);
            return BadRequest(ex.Message + ex.InnerException?.Message);
        }
    }

    /// <summary>
    ///     Endpoint para consultar los pagos por prestador
    /// </summary>
    /// <remarks>
    ///     ## Description
    ///     ### Get pagos
    ///     ## Url
    ///     GET /payments
    /// </remarks>
    /// <response code="200">
    ///     Accepted:
    ///     - Operation successful.
    /// </response>
    /// <returns>Retorna la lista de todos los pagos recibidos por un prestador</returns>
    [Authorize(Policy = AuthorizationPolicies.ProviderPolicy)]
    [HttpGet("Provider/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<PaymentResponse>>> GetPaymentsByProviderId(Guid id,
        [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        _logger.LogInformation("Entrando al método que consulta los pagos de un prestador");
        try
        {
            var sericesQuery = new GetServicesByProviderIdQuery(id);
            var servicesResponse = await _mediator.Send(sericesQuery);
            var response = new List<PaymentResponse>();
            int count = 0;
            foreach (var service in servicesResponse)
            {
                response.AddRange(
                    service.Payments?.Where(p => p.PaymentDate >= (startDate ?? DateTime.MinValue) && p.PaymentDate <=
                        (endDate ?? DateTime.MaxValue)).ToList() ??
                    Enumerable.Empty<PaymentResponse>());
                for (int i = count; i < response.Count; i++)
                {
                    response[i].Service = new ServiceResponse { Id = service.Id, Name = service.Name};
                }
                count = response.Count;
            }
            return Ok(response);
        }
        catch (CustomException ex)
        {
            _logger.LogError($"Ocurrio un error al consultar los pagos del prestador {id}. Exception: " + ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ocurrio un error al consultar los pagos del prestador {id}. Exception: " + ex.Message);
            return BadRequest(ex.Message + ex.InnerException?.Message);
        }
    }

    /// <summary>
    ///     Endpoint para consultar los pagos por servicio
    /// </summary>
    /// <remarks>
    ///     ## Description
    ///     ### Get pagos por servicio
    ///     ## Url
    ///     GET /payments/Service/{id}
    /// </remarks>
    /// <response code="200">
    ///     Accepted:
    ///     - Operation successful.
    /// </response>
    /// <returns>Retorna la lista de todos los pagos de un servicio</returns>
    [Authorize(Policy = AuthorizationPolicies.ProviderPolicy)]
    [HttpGet("Service/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> GetPaymentsByServiceId(Guid id,
        [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        _logger.LogInformation("Entrando al método que consulta los pagos de un servicio");
        try
        {
            var query = new GetPaymentsByServiceIdQuery(id, startDate, endDate);
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (CustomException ex)
        {
            _logger.LogError($"Ocurrio un error al consultar los pagos del servicio {id}. Exception: " + ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ocurrio un error al consultar los pagos del servicio {id}. Exception: " + ex.Message);
            return BadRequest(ex.Message + ex.InnerException?.Message);
        }
    }

    /// <summary>
    ///     Endpoint que registra un pago
    /// </summary>
    /// <remarks>
    ///     ## Description
    ///     ### Post pago
    ///     ## Url
    ///     POST /payments
    /// </remarks>
    /// <response code="200">
    ///     Accepted:
    ///     - Operation successful.
    /// </response>
    /// <returns>Retorna el id del nuevo registro</returns>
    [Authorize(Policy = AuthorizationPolicies.ConsumerPolicy)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> PostPayment(PaymentRequest request)
    {
        _logger.LogInformation("Entrando al método que registra un pago");
        try
        {
            request.PaymentStatus = PaymentStatusEnum.Pendiente;
            var query = new CreatePaymentCommand(request);
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (CustomException ex)
        {
            _logger.LogError("Ocurrio un error al intentar registrar un pago. Exception: " + ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Ocurrio un error al intentar registrar un pago. Exception: " + ex.Message);
            return BadRequest(ex.Message + ex.InnerException?.Message);
        }
    }
    
    /// <summary>
    ///     Endpoint que actualiza el estado de un pago
    /// </summary>
    /// <remarks>
    ///     ## Description
    ///     ### Put pago
    ///     ## Url
    ///     PATCH /payments/{id}
    /// </remarks>
    /// <response code="200">
    ///     Accepted:
    ///     - Operation successful.
    /// </response>
    /// <returns>Retorna un mensaje de exito</returns>
    [Authorize(Policy = AuthorizationPolicies.AdminPolicy)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> UpdatePaymentStatus(Guid id, [FromBody] PaymentStatusEnum? newPaymentStatus)
    {
        _logger.LogInformation("Entrando al método que registra un pago");
        try
        {
            var query = new UpdatePaymentStatusCommand(id,newPaymentStatus);
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (CustomException ex)
        {
            _logger.LogError("Ocurrio un error al intentar registrar un pago. Exception: " + ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Ocurrio un error al intentar registrar un pago. Exception: " + ex.Message);
            return BadRequest(ex.Message + ex.InnerException?.Message);
        }
    }
    
    /// <summary>
    ///     Endpoint que registra un formato de campos de pago
    /// </summary>
    /// <remarks>
    ///     ## Description
    ///     ### Post registra campos de pago.
    ///     ## Url
    ///     POST /paymentformat
    /// </remarks>
    /// <response code="200">
    ///     Accepted:
    ///     - Operation successful.
    /// </response>
    /// <returns>Retorna la lista de los ids de los campos .</returns>
    [Authorize(Policy = AuthorizationPolicies.AdminPolicy )]
    [HttpPost("paymentformat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<Guid>>> CreatePaymentFormat(List<PaymentFieldRequest> fieldsRequests)
    {
        _logger.LogInformation("Entrando al método que registra el formato de conciliacion dado un servicio");
        try
        {
            List<Guid> responsesList = new();
            foreach (var fieldRequest in fieldsRequests)
            {
                var query = new CreatePaymentFieldCommand(fieldRequest);
                responsesList.Add((await _mediator.Send(query)));
            }
            return Ok(responsesList);
        }
        catch (CustomException ex)
        {
            _logger.LogError("Ocurrio un error al intentar registrar un campo de Pago. Exception: " + ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Ocurrio un error al intentar registrar un campo de Pago. Exception: " + ex.Message);
            return BadRequest(ex.Message + ex.InnerException?.Message);
        }
    }
    
    /// <summary>
    ///     Endpoint que retorna los campos de pago dado un id de servicio
    /// </summary>
    /// <remarks>
    ///     ## Description
    ///     ### Get campos de pago dado un servicio.
    ///     ## Url
    ///     GET /paymentformat
    /// </remarks>
    /// <response code="200">
    ///     Accepted:
    ///     - Operation successful.
    /// </response>
    /// <returns>Retorna la lista de los campos de pago.</returns>
    // [Authorize(Policy = AuthorizationPolicies.AdminPolicy )]
    [HttpGet("paymentformat/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]

    public async Task<ActionResult<List<PaymentFieldResponse>>> GetPaymentFormat(Guid id)
    {
        _logger.LogInformation("Entrando al método que consultas los fields con el id");
        try
        {
            var query = new GetPaymentFieldsByServiceIdQuery(id);
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (CustomException ex)
        {
            _logger.LogError("Ocurrio un error al intentar registrar un campo de Pago. Exception: " + ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError("Ocurrio un error en la consulta de un field dado el id. Exception: " + ex.Message);
            return BadRequest(ex.Message + ex.InnerException?.Message);
        }
    }
}