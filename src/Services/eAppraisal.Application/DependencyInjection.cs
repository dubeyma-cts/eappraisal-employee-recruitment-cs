using Microsoft.Extensions.DependencyInjection;
using eAppraisal.Application.Services;
using eAppraisal.Domain.Interfaces;

namespace eAppraisal.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Domain Services
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<ICycleService, CycleService>();
        services.AddScoped<IAuthService, AuthService>();

        // Appraisal Domain Services (decomposed from monolithic AppraisalService)
        services.AddScoped<IAppraisalWorkflowService, AppraisalWorkflowService>();
        services.AddScoped<ICommentsService, CommentsService>();
        services.AddScoped<ICtcService, CtcService>();
        services.AddScoped<IReportingService, ReportingService>();
        services.AddScoped<IEligibilityRulesService, EligibilityRulesService>();
        services.AddScoped<IPolicyMaskingEngine, PolicyMaskingEngine>();

        return services;
    }
}
