using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Adicionar configurações de JWT ao serviço de configuração
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<ITokenService, TokenService>();

// Configurar autenticação com JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]))
    };
});

// Adicionar autorização
builder.Services.AddAuthorization();

// Outros serviços (como controllers e HTTP client)
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.UseAuthentication(); // Middleware de autenticação
app.UseAuthorization();  // Middleware de autorização

// Mapear controladores automaticamente
app.MapControllers();

// Rota de exemplo (opcional)
app.MapGet("/", () => "Bem-vindo à API ProxyFallback! Acesse /swagger para a documentação.");

app.Run();
