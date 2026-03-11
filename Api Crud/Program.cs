using System.Text;
using DzDex.API.Data;
using DzDex.API.Seed;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configurar o banco de dados SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=dzdex.db";
EnsureSqliteDirectoryExists(connectionString);

builder.Services.AddDbContext<DzDexContext>(options =>
    options.UseSqlite(connectionString));

// Configurar controllers
builder.Services.AddControllers();

var jwt = builder.Configuration.GetSection("Jwt");
var jwtKey = jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key nao configurado.");
var jwtIssuer = jwt["Issuer"] ?? "DzDexAPI";
var jwtAudience = jwt["Audience"] ?? "DzDexClient";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

builder.Services.AddAuthorization();

// Ativar o Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DzDex API",
        Version = "v1",
        Description = "API CRUD para lutas de anime e aliens do Ben 10 com upload de imagem, busca, renomear e prÃ©via de vÃ­deo do YouTube."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT no formato: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configurar arquivos estÃ¡ticos
app.UseStaticFiles();

// Se for usar apenas HTTP, comente a linha abaixo
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Configurar o Swagger apenas em desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DzDex API v1");
        c.RoutePrefix = "swagger";
    });
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DzDexContext>();
    context.Database.EnsureCreated();
    SeedDatabase.Initialize(context);
}

// Configurar rotas
app.MapControllers();

// Configurar redirecionamento para a pÃ¡gina inicial
app.MapGet("/", () => Results.Redirect("/login.html"));

// Configurar fallback para index.html
app.MapFallbackToFile("login.html");

app.Run();

static void EnsureSqliteDirectoryExists(string connectionString)
{
    var builder = new SqliteConnectionStringBuilder(connectionString);
    var dataSource = builder.DataSource;

    if (string.IsNullOrWhiteSpace(dataSource) || dataSource == ":memory:")
    {
        return;
    }

    var fullPath = Path.IsPathRooted(dataSource)
        ? dataSource
        : Path.Combine(AppContext.BaseDirectory, dataSource);

    var directory = Path.GetDirectoryName(fullPath);

    if (!string.IsNullOrWhiteSpace(directory))
    {
        Directory.CreateDirectory(directory);
    }
}

