using Microsoft.EntityFrameworkCore;
using MinimalApiProdutos.Data;
using MinimalApiProdutos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure() 
    )
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Minimal API Produtos e Categorias (FIAP)", 
        Description = "API CRUD Completo para Gerenciamento de Produtos e Categorias.",
        Version = "v1" 
    });
});

var app = builder.Build();

app.MigrateDatabase();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => "API de Produtos e Categorias com Minimal APIs!");

app.MapGroup("/categorias")
    .WithTags("Categorias")
    .MapCategoriasApi();

app.MapGroup("/produtos")
    .WithTags("Produtos")
    .MapProdutosApi();

app.Run();

public static class MigrationManager
{
    public static WebApplication MigrateDatabase(this WebApplication webApp)
    {
        using (var scope = webApp.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var db = services.GetRequiredService<AppDbContext>();
                db.Database.Migrate(); 
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Ocorreu um erro durante a migração do banco de dados.");
            }
        }
        return webApp;
    }
}

public static class CategoriaEndpoints
{
    public static RouteGroupBuilder MapCategoriasApi(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (AppDbContext db) =>
        {
            return Results.Ok(await db.Categorias.ToListAsync());
        })
        .Produces<List<Categoria>>(StatusCodes.Status200OK);

        group.MapGet("/{id}", async (AppDbContext db, int id) =>
        {
            var categoria = await db.Categorias.FindAsync(id);
            return categoria == null ? Results.NotFound() : Results.Ok(categoria);
        })
        .Produces<Categoria>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id}/produtos", async (AppDbContext db, int id) =>
        {
            var categoria = await db.Categorias
                .Include(c => c.Produtos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null)
                return Results.NotFound();

            return Results.Ok(categoria.Produtos);
        })
        .Produces<List<Produto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
        
        group.MapPost("/", async (AppDbContext db, Categoria categoria) =>
        {
            db.Categorias.Add(categoria);
            await db.SaveChangesAsync();
            return Results.Created($"/categorias/{categoria.Id}", categoria);
        })
        .Produces<Categoria>(StatusCodes.Status201Created)
        .Accepts<Categoria>("application/json");
        
        group.MapPut("/{id}", async (AppDbContext db, int id, Categoria categoriaRecebida) =>
        {
            var categoriaExistente = await db.Categorias.FindAsync(id);
            if (categoriaExistente == null) return Results.NotFound();

            categoriaExistente.Nome = categoriaRecebida.Nome; 

            await db.SaveChangesAsync();
            return Results.Ok(categoriaExistente); 
        })
        .Produces<Categoria>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
        
        group.MapDelete("/{id}", async (AppDbContext db, int id) =>
        {
            var categoria = await db.Categorias.FindAsync(id);
            if (categoria == null)
                return Results.NotFound();

            db.Categorias.Remove(categoria);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}

public static class ProdutoEndpoints
{
    public static RouteGroupBuilder MapProdutosApi(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (AppDbContext db) =>
        {
            return Results.Ok(await db.Produtos.Include(p => p.Categoria).ToListAsync());
        })
        .Produces<List<Produto>>(StatusCodes.Status200OK);
        
        group.MapGet("/{id}", async (AppDbContext db, int id) =>
        {
            var produto = await db.Produtos.Include(p => p.Categoria).FirstOrDefaultAsync(p => p.Id == id);
            return produto is null ? Results.NotFound() : Results.Ok(produto);
        })
        .Produces<Produto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (AppDbContext db, Produto produto, ILogger<Program> logger) =>
        {
            var categoriaExiste = await db.Categorias.AnyAsync(c => c.Id == produto.CategoriaId);
            if (!categoriaExiste)
            {
                logger.LogError("Tentativa de criar produto com CategoriaId inválida: {CategoriaId}", produto.CategoriaId);
                return Results.BadRequest("CategoriaId inválida. O produto deve pertencer a uma categoria existente.");
            }

            db.Produtos.Add(produto);
            await db.SaveChangesAsync();
            
            logger.LogInformation("Produto criado: {Nome}", produto.Nome);

            return Results.Created($"/produtos/{produto.Id}", produto);
        })
        .Produces<Produto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Accepts<Produto>("application/json");
        
        group.MapPut("/{id}", async (AppDbContext db, int id, Produto produtoRecebido) =>
        {
            var produtoExistente = await db.Produtos.FindAsync(id);
            if (produtoExistente == null) return Results.NotFound();
            
            if (!await db.Categorias.AnyAsync(c => c.Id == produtoRecebido.CategoriaId)) 
                return Results.BadRequest("CategoriaId inválida.");

            produtoExistente.Nome = produtoRecebido.Nome;
            produtoExistente.Preco = produtoRecebido.Preco;
            produtoExistente.Estoque = produtoRecebido.Estoque;
            produtoExistente.CategoriaId = produtoRecebido.CategoriaId;

            await db.SaveChangesAsync();
            return Results.Ok(produtoExistente); 
        })
        .Produces<Produto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);
        
        group.MapDelete("/{id}", async (AppDbContext db, int id) =>
        {
            var produto = await db.Produtos.FindAsync(id);
            if (produto == null)
                return Results.NotFound();

            db.Produtos.Remove(produto);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}