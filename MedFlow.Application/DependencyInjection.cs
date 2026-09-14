using AutoMapper;
using MedFlow.Application.MedFlowServis.AuthServices.Concrete;
using MedFlow.Application.MedFlowServis.BookingServices.Concrete;
using MedFlow.Application.MedFlowServis.CourseServices.Concrete;
using MedFlow.Application.MedFlowServis.NurseAssignmentServices.Concrete;
using MedFlow.Application.MedFlowServis.NurseAvailabilityServices.Concrete;
using MedFlow.Application.MedFlowServis.NurseServices.Concrete;
using MedFlow.Application.MedFlowServis.OtpServices.Concrete;
using MedFlow.Application.MedFlowServis.PatientServices.Concrete;
using MedFlow.Application.MedFlowServis.PaymentServices.Concrete;
using MedFlow.Application.MedFlowServis.PharmacyServices.Concrete;
using MedFlow.Application.MedFlowServis.PrescriptionServices.Concrete;
using MedFlow.Application.MedFlowServis.ServiceTypeServices.Concrete;
using MedFlow.Application.MedFlowServis.SessionServices.Concrete;
using MedFlow.Application.MedFlowServis.WalletServices.Concrete;
using Microsoft.Extensions.DependencyInjection;

namespace MedFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<INurseService, NurseService>();
        services.AddScoped<INurseAvailabilityService, NurseAvailabilityService>();
        services.AddScoped<INurseAssignmentService, NurseAssignmentService>();
        services.AddScoped<IPrescriptionService, PrescriptionService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IPharmacyService, PharmacyService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IServiceTypeService, ServiceTypeService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IBookingService, BookingService>();

        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        return services;
    }
}
