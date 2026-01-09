using CardLedger.Api.Application.Commands;
using CardLedger.Api.Infrastructure;
using CardLedger.Api.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("client", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateCardCommand>());

builder.Services.AddSingleton<IFxRateProvider, JsonFxRateProvider>();
builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
builder.Services.AddScoped<ICardLedgerRepository, CardLedgerRepository>();
builder.Services.AddSingleton<DbInitializer>();

var app = builder.Build();

app.Use(async (ctx, next) =>
{
    try { await next(); }
    catch (Exception ex)
    {
        ctx.Response.ContentType = "application/problem+json";
        var result = CardLedger.Api.Infrastructure.ProblemDetailsExtensions.ToProblem(ex);
        await result.ExecuteAsync(ctx);
    }
});

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("client");

app.MapControllers();

app.MapGet("/fx-rates", ([FromServices] IFxRateProvider fx) => Results.Ok(fx.GetAll()))
   .WithName("GetFxRates")
   .WithOpenApi();

await using (var scope = app.Services.CreateAsyncScope())
{
    var init = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await init.InitializeAsync(app.Lifetime.ApplicationStopping);
}

// Health
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
