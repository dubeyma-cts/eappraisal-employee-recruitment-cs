using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using eAppraisal.Application.Contracts;
using eAppraisal.Domain.Interfaces;
using eAppraisal.Infrastructure.CrossCutting;
using eAppraisal.Infrastructure.Data;
using eAppraisal.Infrastructure.ExternalServices;
using eAppraisal.Infrastructure.Identity;

namespace eAppraisal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
    {
        // Data Layer
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        // Identity Provider
        services.AddScoped<IUserStore, IdentityUserStore>();

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders()
        .AddClaimsPrincipalFactory<AppClaimsPrincipalFactory>();

        // Platform & Cross-Cutting Services
        services.AddScoped<IAuditService, AuditLogCollector>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IObservabilityAgent, ObservabilityAgent>();
        services.AddSingleton<IRateLimiter, InMemoryRateLimiter>();
        services.AddSingleton<IFeatureFlagService, InMemoryFeatureFlagService>();

        // External Services
        services.AddSingleton<ISmtpGateway, LocalSmtpGateway>();
        services.AddSingleton<IHrisPayrollService, StubHrisPayrollService>();

        return services;
    }
}
