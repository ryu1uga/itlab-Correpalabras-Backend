using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using CorrePalabras.Data;
using CorrePalabras.Services;
using CorrePalabras.Services.Interfaces;
using CorrePalabras.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Cargar variables de entorno del archivo .env
if (builder.Environment.IsDevelopment())
{
    DotNetEnv.Env.Load();
}

// Configuración de Kestrel para Docker
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080); 
});

// --- SERVICIOS BASE ---
builder.Services.AddControllers().AddJsonOptions(x =>
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddEndpointsApiExplorer();

// 2. Configuración de Swagger con soporte para JWT (Botón de Autorización)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CorrePalabras API", Version = "v1" });
    
    // Configurar el esquema de seguridad Bearer
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. \r\n\r\n Escribe 'Bearer' [espacio] y luego tu token.\r\n\r\nEjemplo: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// 3. Configuración de la Base de Datos (PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("CRÍTICO: 'ConnectionStrings__DefaultConnection' no está definida. La aplicación no puede iniciar sin base de datos.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 4. Configuración de Autenticación JWT
var jwtKey = builder.Configuration["JWT_KEY"];
var jwtIssuer = builder.Configuration["JWT_ISSUER"];
var jwtAudience = builder.Configuration["JWT_AUDIENCE"];

if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("CRÍTICO: Las variables JWT (JWT_KEY, JWT_ISSUER, JWT_AUDIENCE) son obligatorias para el funcionamiento del sistema.");
}

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// Email e Infraestructura
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// Servicio de JWT (Crucial para Login)
builder.Services.AddScoped<IJwtService, JwtService>();

// Servicios de Negocio
builder.Services.AddScoped<IAttachmentsService, AttachmentsService>();
builder.Services.AddScoped<IAvatarsService, AvatarsService>();
builder.Services.AddScoped<IBadgesService, BadgesService>();
builder.Services.AddScoped<ICategoriesService, CategoriesService>();
builder.Services.AddScoped<ILanguagesService, LanguagesService>();
builder.Services.AddScoped<IPageContentsService, PageContentsService>();
builder.Services.AddScoped<IPagesService, PagesService>();
builder.Services.AddScoped<IProfilesService, ProfilesService>();
builder.Services.AddScoped<IProfileStoriesService, ProfileStoriesService>();
builder.Services.AddScoped<IStoriesService, StoriesService>();
builder.Services.AddScoped<IStoryCategoriesService, StoryCategoriesService>();
builder.Services.AddScoped<IStoryLanguagesService, StoryLanguagesService>();
builder.Services.AddScoped<IUnlockedAvatarsService, UnlockedAvatarsService>();
builder.Services.AddScoped<IUnlockedBadgesService, UnlockedBadgesService>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();

builder.Services.AddHttpClient<ISynologyService, SynologyService>();

// Configuración de CORS - Restringida según ambiente
var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(",") ?? new[] { "http://localhost:3000" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy => policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
});

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// --- PIPELINE DE MIDDLEWARES (EL ORDEN IMPORTA) ---

// 0. Global Exception Handler (PRIMERO: captura todas las excepciones)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// 2. Swagger (solo en desarrollo)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

// 3. Seguridad y CORS
app.UseCors("AllowSpecificOrigin");

// 4. AUTENTICACIÓN Y AUTORIZACIÓN (OBLIGATORIO ANTES DE CONTROLLERS)
app.UseAuthentication(); // ¿Quién es el usuario?
app.UseAuthorization();  // ¿Tiene permiso?

app.MapControllers();

app.Run();