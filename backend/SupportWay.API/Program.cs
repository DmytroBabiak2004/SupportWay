using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SupportWay.API.Services;
using SupportWay.API.Services.Implementations;
using SupportWay.API.Services.Interface;
using SupportWay.Core.Services;
using SupportWay.Data.Context;
using SupportWay.Data.Models;
using SupportWay.Data.Repositories.Implementations;
using SupportWay.Data.Repositories.Interfaces;
using SupportWay.Services;
using SupportWay.Services.Implementations;
using SupportWay.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SupportWay API", Version = "v1" });

    c.SupportNonNullableReferenceTypes();

    c.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field (Example: 'Bearer eyJ...')",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddScoped<IChatsRepository, ChatsRepository>();
builder.Services.AddScoped<IFollowRepository, FollowsRepository>();
builder.Services.AddScoped<IRequestStatusesRepository, RequestStatusesRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IHelpRequestsRepository, HelpRequestsRepository>();
builder.Services.AddScoped<IPostCommentsRepository, PostCommentsRepository>();
builder.Services.AddScoped<IPostLikesRepoository, PostLikesRepository>();
builder.Services.AddScoped<IPostRepository, PostsRepository>();
builder.Services.AddScoped<IProfilesRepository, ProfilesRepository>();
builder.Services.AddScoped<IRequestItemsRepository, RequestItemsRepository>();
builder.Services.AddScoped<IProfileRatingRepository, ProfileRatingRepository>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<ProfileRatingRepository>();

builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<IHelpRequestService, HelpRequestService>();
builder.Services.AddScoped<IPostCommentService, PostCommentService>();
builder.Services.AddScoped<IPostLikeService, PostLikeService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IRequestItemService, RequestItemService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:4200") 
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});


builder.Services.AddSignalR();

builder.Services.AddControllers();

builder.Services.AddDbContext<SupportWayContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("SupportWayDb"));
});

builder.Services.AddIdentity<User, IdentityRole>(options =>
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

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub")
   .RequireCors("AllowAngular");


app.Run();