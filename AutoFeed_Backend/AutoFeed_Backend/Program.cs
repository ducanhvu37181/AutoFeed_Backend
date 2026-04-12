using AutoFeed_Backend_DAO.Settings;
using AutoFeed_Backend_Services.Interfaces;
using AutoFeed_Backend_Services.ServiceProvider;
using AutoFeed_Backend_Services.Services;
using AutoFeed_Backend_DAO.Models; // Thêm dòng này để nhận diện DbContext
using Microsoft.EntityFrameworkCore; // Thêm dòng này để dùng UseSqlServer
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- DÒNG QUAN TRỌNG NHẤT ĐỂ SỬA LỖI aggregateexception ---
builder.Services.AddDbContext<AutoFeedDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// ---------------------------------------------------------

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new AutoFeed_Backend.Json.FlexibleDateOnlyJsonConverter());
    });
builder.Services.AddServiceProvider();
builder.Services.AddScoped<AutoFeed_Backend_Repositories.UnitOfWork.IUnitOfWork, AutoFeed_Backend_Repositories.UnitOfWork.UnitOfWork>();
builder.Services.AddScoped<IIoTDeviceService, IoTDeviceService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c => {
    c.MapType<IFormFile>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string", Format = "binary" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Token: "
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            }, new string[] { }
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("key")),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
//builder.WebHost.UseUrls("http://localhost:5207");
var app = builder.Build();

var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
try
{
    using (var scope = app.Services.CreateScope())
    {
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
       // await userService.MigratePasswordsAsync();
    }
}
catch (Exception ex)
{
    startupLogger.LogWarning(ex, "Password migration skipped: database not reachable. Start SQL Server (e.g. docker compose up -d) and restart the app.");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();