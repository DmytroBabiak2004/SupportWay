using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SupportWay.API.Services;
using SupportWay.Data.Context;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SupportWay API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
     {
         {
             new Microsoft.OpenApi.Models.OpenApiSecurityScheme
             {
                 Reference = new Microsoft.OpenApi.Models.OpenApiReference
                 {
                     Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                     Id = "Bearer"
                 }
             },
             new string[] {}
         }
     });
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddCors(options =>
{
    // Політика для розробки (дозволяє все)
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

});

// Додаємо сервіси
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Налаштування DbContext
var connectionString = builder.Configuration.GetConnectionString("SupportWayDB");

builder.Services.AddDbContext<SupportWayContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("SupportWayDB"));
});

// Налаштування Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
    .AddEntityFrameworkStores<SupportWayContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigAuthentication(builder.Configuration);
var app = builder.Build();

// Налаштування pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
    app.UseCors("AllowAll");
}
else
{
    app.UseCors("AllowSpecificOrigin");
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();