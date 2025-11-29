using Graduation_project.Data;
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

var builder = WebApplication.CreateBuilder(args);

// Database configuration

builder.Services.AddDbContext<AppDbContext>(option =>
            option.UseSqlServer(builder.Configuration.GetConnectionString("constr")));

builder.Services.AddIdentity<AppUser, IdentityRole>()
          .AddEntityFrameworkStores<AppDbContext>()
           .AddDefaultTokenProviders();

//services configuration

builder.Services.AddScoped<IGenerateToken, GenerateToken>();
builder.Services.AddSingleton<IImageManagementService, ImageManagementService>();
builder.Services.AddSingleton<IFileProvider>(
 new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))); ;

// Token Configuration

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
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Token:Secret"])),
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration["Token:Issuer"],
                        ValidateAudience = false,
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
    options.Title = "Friendify API";
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
if (app.Environment.IsDevelopment())
{
    // Serve the OpenAPI/Swagger JSON at /openapi/v1.json
    app.UseOpenApi(settings => settings.Path = "/openapi/v1.json");

    // Serve the NSwag UI; point the UI to the JSON above
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Friendify API"));
}

app.UseCors("CORSPolicy");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();

app.MapGet("/", () => "Hello World!");

app.Run();

