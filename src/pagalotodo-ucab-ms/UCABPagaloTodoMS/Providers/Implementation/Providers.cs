﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SendGrid;
using UCABPagaloTodoMS.Core.Services;
using UCABPagaloTodoMS.Infrastructure.Database;
using UCABPagaloTodoMS.Infrastructure.Services;
using UCABPagaloTodoMS.Infrastructure.Settings;
using UCABPagaloTodoMS.Providers.Interface;

namespace UCABPagaloTodoMS.Providers.Implementation
{
    [ExcludeFromCodeCoverage]
    public class Providers : IProviders
    {
        private const string AllowAllOriginsPolicy = "_AllowAllOriginsPolicy";

        public IServiceCollection AddAuthorizationServices(IServiceCollection services, IConfiguration configuration,
            AppSettings appSettings)
        {
            services.AddAuthorization();
            services.AddAuthentication("Bearer").AddIdentityServerAuthentication(options =>
            {
                options.Authority = configuration["IdentityServerUrl"];
                options.RequireHttpsMetadata = false;
                options.ApiName = appSettings.ApiName;
            });
            return services;
        }


        public IServiceCollection AddControllers(IServiceCollection services, IConfiguration configuration,
            AppSettings appSettings)
        {
            services.AddControllers();

            return services;
        }

        public IServiceCollection AddCors(IServiceCollection services)
        {
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddCors(options =>
            {
                options.AddPolicy(AllowAllOriginsPolicy,
                    builder =>
                    {
                        builder.AllowAnyMethod();
                        builder.AllowAnyHeader();
                    });
            });
            return services;
        }

        public IServiceCollection AddDatabaseService(IServiceCollection services, IConfiguration configuration,
            string environment, bool isRequired)
        {
            string DBConnectionString = configuration["DBConnectionString"];
            services.AddDbContext<UCABPagaloTodoDbContext>(options => options.UseSqlServer(DBConnectionString));


            services.AddHealthChecks()
                .AddDbContextCheck<UCABPagaloTodoDbContext>(null, null, new[] {"ready"});
            return services;
        }

        public IServiceCollection AddRabbitMqService(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient(sp=>new ConciliationProducer());
            services.AddTransient(sp => new ConfirmationListProducer());
            services.AddTransient<ProducerResolver>(provider => key =>
            {
                switch (key)
                {
                    case "Conciliation":
                        return provider.GetService<ConciliationProducer>();
                    case "Confirmation":
                        return provider.GetService<ConfirmationListProducer>();
                    default:
                        throw new KeyNotFoundException();
                }
            });
            services.AddSingleton(new DbContextFactory(configuration["DBConnectionString"]));
            services.AddHostedService<ConciliationRabbitMqConsumer>();
            services.AddHostedService<ConfirmationListRabbitMqConsumer>();
            return services;
        }
        
        public IServiceCollection AddSendGridService(IServiceCollection services, IConfiguration configuration)
        {
            var apiKey = configuration["SendGrid:ApiKey"];
            var client = new SendGridClient(apiKey);
            services.AddSingleton<ISendGridClient>(client);
            var senderEmail = configuration["SendGrid:SenderEmail"];
            var senderName = configuration["SendGrid:SenderName"];
            services.AddTransient(provider =>
                new ForgotPasswordEmailSender(provider.GetRequiredService<ISendGridClient>(), senderEmail, senderName));
            services.AddTransient(provider =>
                new ConciliationEmailSender(provider.GetRequiredService<ISendGridClient>(), senderEmail, senderName));
            services.AddTransient<SenderResolver>(provider => key =>
            {
                switch (key)
                {
                    case "ForgotPassword":
                        return provider.GetService<ForgotPasswordEmailSender>();
                    case "Conciliation":
                        return provider.GetService<ConciliationEmailSender>();
                    default:
                        throw new KeyNotFoundException();
                }
            });
            return services;
        }

        public IServiceCollection AddHealthCheck(IServiceCollection services, IConfiguration configuration,
            AppSettings appSettings)
        {
            services.AddHealthChecks();
            return services;
        }


        public IServiceCollection AddSwagger(IServiceCollection services, string versionNumber, AppSettings appSettings)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(versionNumber,
                    new OpenApiInfo
                    {
                        Title = "API Starter",
                        Version = versionNumber,
                        Description = "An API to perform The Starter Ops",
                        Contact = new OpenApiContact
                        {
                            Name = "Ucab DS",
                            Email = appSettings.SharedMail,
                            Url = new Uri(appSettings.UCABUrl)
                        },
                        License = new OpenApiLicense {Name = "UCAB", Url = new Uri(appSettings.UCABUrl)}
                    });
                c.AddSecurityDefinition("Authorization",
                    new OpenApiSecurityScheme
                    {
                        Description =
                            "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                        In = ParameterLocation.Header,
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT"
                    });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Authorization"
                            },
                            Scheme = "Bearer",
                            Name = "Authorization",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });

                c.AddSecurityDefinition("Application-Key",
                    new OpenApiSecurityScheme
                    {
                        Description = "Identify the client from the api management and the subscription that uses it",
                        In = ParameterLocation.Header,
                        Name = "Application-Key",
                        Type = SecuritySchemeType.ApiKey
                    });
            });
            return services;
        }
    }
}