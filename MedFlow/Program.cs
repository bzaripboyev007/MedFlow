using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using MedFlow.Application;
using MedFlow.Application.Common.Permissions;
using MedFlow.BackgroundServices;
using MedFlow.DataLayer.Repository;
using MedFlow.Infrastructure;
using MedFlow.Middleware;
using MedFlow.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minio;

TypeDescriptor.AddAttributes(typeof(DateOnly), new TypeConverterAttribute(typeof(DateOnlyTypeConverter)));

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new DateJsonConverter());
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT access token: Bearer {token}"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

const string FrontendCorsPolicy = "FrontendCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var minioConfig = builder.Configuration.GetSection("Minio");
builder.Services.AddMinio(client => client
    .WithEndpoint(minioConfig["Endpoint"])
    .WithCredentials(minioConfig["AccessKey"], minioConfig["SecretKey"])
    .WithSSL(bool.Parse(minioConfig["UseSSL"]!))
    .Build());

builder.Services.AddDataLayerRepository(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddHostedService<MissedSessionSweepService>();

var jwtSigningKey = builder.Configuration["Jwt:SigningKey"]
    ?? throw new InvalidOperationException("Jwt:SigningKey topilmadi.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MedFlow";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtIssuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Standart bo'yicha har bir endpoint autentifikatsiyani talab qiladi;
    // ochiq endpoint'lar (AuthController) [AllowAnonymous] bilan belgilanadi.
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    foreach (var code in PermissionCode.All)
    {
        options.AddPolicy(code, policy => policy.RequireAssertion(ctx => RolePermissions.Has(ctx.User, code)));
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
