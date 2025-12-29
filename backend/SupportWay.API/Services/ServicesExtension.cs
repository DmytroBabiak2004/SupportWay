using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace SupportWay.API.Services
{
    public static class ServicesExtension
    {
        public static void ConfigAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var key = Encoding.ASCII.GetBytes(config["Jwt:Key"]);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        // Забороняємо дефолтну поведінку (щоб не було "response already started")
                        context.HandleResponse();

                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";

                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            error = "Unauthorized"
                        });

                        await context.Response.WriteAsync(result);
                    },

                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";

                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            error = "Forbidden"
                        });

                        await context.Response.WriteAsync(result);
                    }
                };
            });
        }
    }

}
