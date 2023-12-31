using System.Globalization;
using System.Net;
using System.Reflection;
using Castle.Core.Internal;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.Extensions.Logging;
using UCABPagaloTodoMS.Application.Exceptions;
using UCABPagaloTodoMS.Application.Requests;
using UCABPagaloTodoMS.Application.Responses;
using UCABPagaloTodoMS.Core.Database;
using UCABPagaloTodoMS.Core.Entities;
using UCABPagaloTodoMS.Core.Services;

namespace UCABPagaloTodoMS.Application.Handlers;

public class AccountingCloseRequestHandler : IRequestHandler<AccountingCloseRequest, string>
{
    private readonly IUCABPagaloTodoDbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<AccountingCloseRequestHandler> _logger;

    public AccountingCloseRequestHandler(IUCABPagaloTodoDbContext dbContext, SenderResolver resolver,
        ILogger<AccountingCloseRequestHandler> logger)
    {
        _dbContext = dbContext;
        _emailSender = resolver("Conciliation");
        _logger = logger;
    }

    public async Task<string> Handle(AccountingCloseRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.Providers is null)
            {
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
    /// Handles the processing of an accounting close request, generating CSV files for each payment associated with each service of each provider in the request, and sending those CSV files to the corresponding provider email addresses.
    /// </summary>
    /// <param name="request">The accounting close request.</param>
    /// <returns>A message indicating the number of files generated, providers processed, and errors encountered during the processing.</returns>
    private async Task<string> HandleAsync(AccountingCloseRequest request)
    {
        try
        {
            var numPrestadores = 0;
            var numArchivos = 0;
            var numErrores = 0;
            foreach (var provider in request.Providers)
            {
                if (provider.Services is not null)
                {
                    var csvList = new List<CsvResponse>();
                    foreach (var service in provider.Services)
                    {
                        if (service.Payments is not null && service.Payments.Any())
                        {
                            csvList.Add(GenerateCsv(service));
                            numArchivos++;
                        }
                    }

                    if (csvList.Any())
                    {
                        numPrestadores++;
                        var count = 0;
                        while (await _emailSender.SendEmailAsync(provider.Email!, csvList) != HttpStatusCode.Accepted)
                        {
                            if (count >= 4)
                            {
                                numErrores++;
                                break;
                            }

                            count++;
                        }
                    }
                }
            }

            await AddAccountingClose();
            return $"Se emitieron {numArchivos} archivos a {numPrestadores} prestadores. Errores: {numErrores}";
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error AccountingCloseRequestHandler.HandleAsync. {Mensaje}", e.Message);
            throw;
        }
    }

    /// <summary>
    /// Generates a CSV file for the payments associated with a service, based on the service's conciliation format and payment details.
    /// </summary>
    /// <param name="service">The service for which to generate the CSV file.</param>
    /// <returns>A CSV response containing the generated CSV file.</returns>
    private CsvResponse GenerateCsv(ServiceResponse service)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";"
        };
        using var csv = new CsvWriter(writer, config);
        foreach (var field in service.ConciliationFormat!)
        {
            csv.WriteField(field.Name);
        }

        csv.WriteField("Confirmacion");
        csv.NextRecord();
        var paymentProperties = typeof(PaymentResponse).GetProperties().ToDictionary(p => p.Name.ToLower());
        var consumerProperties = typeof(ConsumerResponse).GetProperties().ToDictionary(p => p.Name.ToLower());
        foreach (var payment in service.Payments!)
        {
            var paymentDetails = payment.PaymentDetails?.ToDictionary(d => d.Name!.ToLower(), d => d.Value!);
            foreach (var field in service.ConciliationFormat)
            {
                var className = field.AttrReference!.Split(".")[0];
                var propName = field.AttrReference!.Split(".")[1].ToLower();
                object? propValue = null;
                switch (className.ToLower())
                {
                    case "payment":
                        propValue = paymentProperties.GetValueOrDefault(propName)!.GetValue(payment);
                        break;
                    case "consumer":
                        propValue = consumerProperties.GetValueOrDefault(propName)!.GetValue(payment.Consumer);
                        break;
                    case "paymentdetail":
                        propValue = paymentDetails!.GetValueOrDefault(propName);
                        break;
                }

                var stringValue = propValue!.ToString();
                bool isString = true;
                var formatted = stringValue;
                if (!string.IsNullOrWhiteSpace(field.Format))
                {
                    if (double.TryParse(stringValue, out double doubleValue))
                    {
                        formatted = doubleValue.ToString(field.Format);
                        isString = false;
                    }
                    else if (DateTime.TryParse(stringValue, out DateTime dateValue))
                    {
                        formatted = dateValue.ToString(field.Format);
                        isString = false;
                    }
                }
                if(field.Length is not null && isString)
                {
                    formatted = formatted.Length < field.Length ? formatted.PadRight((int) field.Length - formatted.Length) : formatted.Remove((int)field.Length);
                }
                
                csv.WriteField(formatted);
            }

            csv.WriteField("");
            csv.NextRecord();
        }

        writer.Flush();
        return new CsvResponse(
            String.Concat(service.Name!.Replace(" ", "-"), "_BalanceSheet_", DateTime.UtcNow.ToString("yyyy-MM-dd"),
                ".csv"), stream.ToArray());
    }

    /// <summary>
    /// Adds an entry for the executed accounting close to the database.
    /// </summary>
    /// <returns>A completed task.</returns>
    private async Task<Unit> AddAccountingClose()
    {
        var transaction = _dbContext.BeginTransaction();
        try
        {
            _logger.LogInformation("AccountingCloseRequestHandler.AddAccountingClose");
            var entity = new AccountingCloseEntity()
            {
                Id = new Guid(),
                ExecutedAt = DateTime.UtcNow
            };
            _dbContext.AccountingClosures.Add(entity);
            await _dbContext.SaveEfContextChanges("APP");
            transaction.Commit();
            return Unit.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error AccountingCloseRequestHandler.AddAccountingClose. {Mensaje}", ex.Message);
            transaction.Rollback();
            throw;
        }
    }
}