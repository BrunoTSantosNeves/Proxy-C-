var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços ao contêiner
builder.Services.AddControllers(); // Habilita Controllers
builder.Services.AddEndpointsApiExplorer(); // Necessário para expor endpoints no Swagger
builder.Services.AddSwaggerGen(); // Configura o Swagger para documentação da API
builder.Services.AddHttpClient();


var app = builder.Build();

// Configuração do pipeline de requisições HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Página de erro para dev
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProxyFallbackAPI v1");
    });
}

// Middlewares padrão
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapGet("/", () => "Bem-vindo à API ProxyFallback! Acesse /swagger para a documentação.");

// Mapear controladores automaticamente
app.MapControllers();

// Rota de exemplo (Weather Forecast)
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

// Registro de exemplo para retorno de dados
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
