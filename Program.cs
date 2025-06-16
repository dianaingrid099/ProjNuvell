using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500") // ou "http://localhost:5500"
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<EstoqueService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<PedidoService>();

var app = builder.Build();
app.UseCors("PermitirFrontend");

app.MapGet("/", () => "API Disponível!");

app.MapGet("/getProdutos", async (EstoqueService service) =>
{
    try
    {
        return await service.GetProdutos();
    }
    catch (Exception e)
    {
        throw new Exception("", e);
    }
});

app.MapGet("/getProdutoById/{id}", async (EstoqueService service, Guid id) =>
{
    try
    {
        return await service.GetProdutoById(id);
    }
    catch (Exception e)
    {
        throw new Exception("", e);
    }
});

app.MapPost("/addProdutoEstoque", async (EstoqueService service, Produto produto) =>
{
    try
    {
        await service.AdicionarProduto(produto);
        return Results.Ok("Produto adicionado.");

    }
    catch (Exception e)
    {
        return Results.Problem(e.Message);
    }
});

app.MapDelete("/removerProdutoEstoque/{id}", async (EstoqueService service, string id) =>
{
    await service.RemoverProdutoAsync(id);
    return Results.Ok("Produto removido.");
});

app.MapPut("/atualizarProdutoEstoque/{id}", async (EstoqueService service, string id, Produto produtoAtualizado) =>
{
    await service.AtualizarProdutoAsync(id, produtoAtualizado);
    return Results.Ok("Produto atualizado.");
});

app.MapPost("/login", async (ClienteService service, LoginRequest login) =>
{
    var usuario = await service.Login(login.Email, login.Senha);
    return usuario is not null ? Results.Ok(usuario) : Results.Unauthorized();
});

app.MapPost("/cadastrarUsuario", async (ClienteService service, Cliente cliente) =>
{
    var request = await service.CadastrarCliente(cliente);
    return request;
});

app.MapPost("/finalizarCompra/{id}", async (PedidoService service, [FromBody] List<ItemPedido> itens, Guid id) =>
{
    var request = await service.FinalizarCompra(itens, id);
    return request;
});

app.MapGet("/getPedidosUsuario/{id}", async (PedidoService service, Guid id) =>
{
    var request = await service.GetPedidosByUserId(id);
    return request;
});

app.Run();
