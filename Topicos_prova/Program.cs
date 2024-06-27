using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using prova.Controllers;
using prova.models;
using prova.data;

var builder = WebApplication.CreateBuilder(args);

// Configuração do método de autenticação
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes("qwertyuiopasdfghjklzxcvbnmqwerty")
            )
        };
    });

// Adiciona serviços de autorização
builder.Services.AddAuthorization();

// Configuração do Entity Framework Core
builder.Services.AddDbContext<ServicoContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 33))));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/login", async (HttpContext context) =>
{
    using var reader = new System.IO.StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    
    // Deserializar o objeto Json
    var user = JsonDocument.Parse(body);
    var email = user.RootElement.GetProperty("email").GetString();
    var password = user.RootElement.GetProperty("password").GetString();

    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
    {
        return Results.BadRequest("Credenciais inválidas");
    }

    // Invocar o UserController
    UserController userController = new UserController();
    // Se o usuário existe e tem senha => gerar o token (credencial que dá acesso aos demais endpoints)
    var token = "";
    if (userController.Login(email, password))
    {
        // Gerar o token
        token = GenerateToken(email);
    }

    return Results.Ok(token);
});

app.MapPost("/clientes", [Authorize] async (ServicoContext context, Cliente cliente) =>
{
    context.Clientes.Add(cliente);
    await context.SaveChangesAsync();
    return Results.Created($"/clientes/{cliente.Id}", cliente);
});

app.MapPost("/servicos", [Authorize] async (ServicoContext context, Servico servico) =>
{
    context.Servicos.Add(servico);
    await context.SaveChangesAsync();
    return Results.Created($"/servicos/{servico.Id}", servico);
});

app.MapGet("/servicos/{id}", [Authorize] async (ServicoContext context, int id) =>
{
    var servico = await context.Servicos.FindAsync(id);
    return servico is not null ? Results.Ok(servico) : Results.NotFound();
});

app.MapPut("/servicos/{id}", [Authorize] async (ServicoContext context, int id, Servico updatedServico) =>
{
    var servico = await context.Servicos.FindAsync(id);
    if (servico is null) return Results.NotFound();

    servico.Nome = updatedServico.Nome;
    servico.Preco = updatedServico.Preco;
    servico.Status = updatedServico.Status;

    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.MapPost("/contratos", [Authorize] async (ServicoContext context, Contrato contrato) =>
{
    var cliente = await context.Clientes.FindAsync(contrato.ClienteId);
    var servico = await context.Servicos.FindAsync(contrato.ServicoId);

    if (cliente == null || servico == null)
    {
        return Results.BadRequest("Cliente ou Serviço inválido");
    }

    context.Contratos.Add(contrato);
    await context.SaveChangesAsync();
    return Results.Created($"/contratos/{contrato.Id}", contrato);
});

app.MapGet("/clientes/{clienteId}/servicos", [Authorize] async (ServicoContext context, int clienteId) =>
{
    var cliente = await context.Clientes
        .Include(c => c.Contratos)
        .ThenInclude(c => c.Servico)
        .FirstOrDefaultAsync(c => c.Id == clienteId);

    if (cliente is null) return Results.NotFound();

    var servicos = cliente.Contratos.Select(c => c.Servico).ToList();
    return Results.Ok(servicos);
});

app.Run();

// Método de criação do token
string GenerateToken(string email)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var secretKey = Encoding.ASCII.GetBytes("qwertyuiopasdfghjklzxcvbnmqwerty");
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, email)
        }),
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(secretKey),
            SecurityAlgorithms.HmacSha256Signature
        )
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}
