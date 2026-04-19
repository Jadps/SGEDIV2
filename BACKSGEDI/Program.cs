using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using BACKSGEDI.Infrastructure.Data;
using BACKSGEDI.Infrastructure.Services;
using BACKSGEDI.Infrastructure.Middleware;
using BACKSGEDI.Infrastructure.Data.Interceptors;
using BACKSGEDI.Configuration;
using Serilog;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.Features;

DotNetEnv.Env.Load();
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditInterceptor>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=SGEDI;Username=postgres;Password=DefaultPassword";

builder.Services.AddDbContext<ApplicationDbContext>((sp, opts) => 
{
    opts.UseNpgsql(connectionString);
    opts.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
});

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<AppOptions>()
    .Bind(builder.Configuration.GetSection(AppOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<AdminOptions>()
    .Bind(builder.Configuration.GetSection(AdminOptions.SectionName))
    .ValidateOnStart();

builder.Services.AddOptions<StorageOptions>()
    .Bind(builder.Configuration.GetSection(StorageOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddScoped<IStorageService, LocalFileStorageService>();

var appOptions = builder.Configuration.GetSection(AppOptions.SectionName).Get<AppOptions>() 
                 ?? new AppOptions { FrontendUrl = "http://localhost:4201" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        corsBuilder => corsBuilder
            .WithOrigins(appOptions.FrontendUrl)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                 ?? new JwtOptions { SecretKey = "x38472h4x32h,8947-=.;['´ø«o-;2h84!!" };

builder.Services.AddAuthenticationJwtBearer(s =>
{
    s.SigningKey = jwtOptions.SecretKey;
});

builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.TryGetValue("AccessToken", out var token))
                context.Token = token;
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = appOptions.AntiforgeryHeaderName;
});

builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(o => 
{
    o.DocumentSettings = s =>
    {
        s.Title = "API REST SGEDI V2";
        s.Version = "v1";
    };
});
builder.Services.Configure<FormOptions>(x =>
{
    x.MultipartBodyLengthLimit = 5242880;
});
var app = builder.Build();

app.UseExceptionHandler();

app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.UseAntiforgeryTokenMiddleware();

app.UseFastEndpoints();
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi(c => c.Path = "/openapi/{documentName}.json"); 

    app.MapScalarApiReference(options =>
    {
        options.WithOpenApiRoutePattern("/openapi/v1.json");
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var appOpts = services.GetRequiredService<IOptions<AppOptions>>().Value;
        if (appOpts.RunSeeder)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var adminOptions = services.GetRequiredService<IOptions<AdminOptions>>();
            await DbInitializer.SeedAsync(context, adminOptions);
            
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Database seeding executed successfully.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al sembrar la base de datos.");
    }
}


app.Run();
