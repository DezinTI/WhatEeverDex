using DzDex.API.Data;
using DzDex.API.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configurar o banco de dados SQLite
builder.Services.AddDbContext<DzDexContext>(options =>
    options.UseSqlite("Data Source=dzdex.db"));

// Configurar controllers
builder.Services.AddControllers();

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
});

var app = builder.Build();

// Configurar arquivos estÃ¡ticos
app.UseStaticFiles();

// Se for usar apenas HTTP, comente a linha abaixo
// app.UseHttpsRedirection();

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

// Configurar autorizaÃ§Ã£o
app.UseAuthorization();

// Configurar redirecionamento para a pÃ¡gina inicial
app.MapGet("/", () => Results.Redirect("/index.html"));

// Configurar fallback para index.html
app.MapFallbackToFile("index.html");

app.Run();

