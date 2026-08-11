using ContextEngineering.MemoryPOC.Repository;
using ContextEngineering.MemoryPOC.Service;
using Microsoft.EntityFrameworkCore;
using OllamaSharp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure EF Core with SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<MemoryDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure Memory Services
builder.Services.AddSingleton<IWorkingMemoryManager, WorkingMemoryManager>(); // Working memory lives across requests (singleton for POC)
builder.Services.AddScoped<IMemoryExtractor, MemoryExtractor>();
builder.Services.AddScoped<IMemoryClassifier, MemoryClassifier>();
builder.Services.AddScoped<IMemoryRepository, MemoryRepository>();
builder.Services.AddScoped<IMemoryManager, MemoryManager>();

// Configure Ollama
var ollamaEndpoint = builder.Configuration["Ollama:Endpoint"];
var ollamaModel = builder.Configuration["Ollama:Model"];
builder.Services.AddScoped<OllamaApiClient>(sp => 
{
    var client = new OllamaApiClient(ollamaEndpoint);
    client.SelectedModel = ollamaModel;
    return client;
});
builder.Services.AddScoped<ILLMService, LLMService>();

var app = builder.Build();

// Create database and run migrations on startup (for POC purposes)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MemoryDbContext>();
    dbContext.Database.EnsureCreated(); // Creates DB schema without migrations
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
