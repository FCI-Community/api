using DotNetEnv;
using Graduation_project.Data;
using Graduation_project.Repositories.Implementation;
using Graduation_project.Repositories.Interfaces;
using Graduation_project.Services.Implementation;
using Graduation_project.Services.IService;
using GraduationProject.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors.Security;
using System.Text;

Env.Load();

// Create folder if it doesn't exist
var wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(wwwRootPath))
{
    Directory.CreateDirectory(wwwRootPath);
}

var builder = WebApplication.CreateBuilder(args);


// Database configuration
var connectionString = Environment.GetEnvironmentVariable("MSSQL_URL") 
                    ?? builder.Configuration.GetConnectionString("constr");

if (string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("No connection string found!");

builder.Services.AddDbContext<AppDbContext>(option =>
            option.UseSqlServer(connectionString));

builder.Services.AddIdentity<AppUser, IdentityRole>()
          .AddEntityFrameworkStores<AppDbContext>()
           .AddDefaultTokenProviders();

//services configuration

builder.Services.AddScoped<IGenerateToken, GenerateToken>();
builder.Services.AddSingleton<IImageManagementService, ImageManagementService>();
builder.Services.AddScoped<IAuth, AuthRepository>();
builder.Services.AddScoped<IMajorRepository, MajorRepository>();
builder.Services.AddSingleton<IFileProvider>(
    new PhysicalFileProvider(wwwRootPath)
);

// Identity configuration
builder.Services.Configure<IdentityOptions>(options =>
{ 
    options.User.AllowedUserNameCharacters += " ";
});

// Token Configuration
var tokenSecret = Environment.GetEnvironmentVariable("Token__Secret")
                 ?? builder.Configuration["Token:Secret"];
var tokenIssuer = Environment.GetEnvironmentVariable("Token__Issuer")
                 ?? builder.Configuration["Token:Issuer"];
var tokenAudience = Environment.GetEnvironmentVariable("Token__Audience")
                 ?? builder.Configuration["Token:Audience"];

builder.Services.AddAuthentication(
                op =>
                {
                    op.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    op.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    op.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                }).AddCookie(o =>
                {
                    o.Cookie.Name = "token";
                    o.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized; ;
                        return Task.CompletedTask;
                    };
                }).AddJwtBearer(o =>
                {
                    o.RequireHttpsMetadata = false;
                    o.SaveToken = true;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecret)),
                        ValidateIssuer = true,
                        ValidIssuer = tokenIssuer,
                        ValidateAudience = false,
                        ValidateLifetime = false,
                        ClockSkew = TimeSpan.Zero
                    };
                    o.Events = new JwtBearerEvents()
                    {
                        OnMessageReceived = context =>
                        {
                            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                            {
                                context.Token = authHeader.Substring("Bearer ".Length).Trim();
                            }
                            else
                            {
                                // Fallback to cookie
                                var token = context.Request.Cookies["token"];
                                context.Token = token;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApiDocument(options =>
{
    options.DocumentName = "v1";
    options.Title = "FCI Community API";
    options.Version = "v1";

    options.AddSecurity("Bearer", new NSwag.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Type = OpenApiSecuritySchemeType.Http,
        In = OpenApiSecurityApiKeyLocation.Header,
        Name = "Authorization",
        Scheme = "Bearer"
    });

    options.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("Bearer"));
});

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("CORSPolicy",
    builder =>
    {
        builder.WithOrigins()
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.

    // Serve the OpenAPI/Swagger JSON at /openapi/v1.json
    app.UseOpenApi(settings => settings.Path = "/openapi/v1.json");

    // Serve the NSwag UI; point the UI to the JSON above
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "FCI Community API"));


app.UseCors("CORSPolicy");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();

app.MapGet("/", () => "Hello World!");

app.Run();

