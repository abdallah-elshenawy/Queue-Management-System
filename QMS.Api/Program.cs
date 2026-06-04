using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QMS.Application.Abstracts;
using QMS.Infrastructure;
using QMS.Infrastructure.Repositories;
using QMS.Domain.IRepositories;
using System.Text;
using QMS.Application.Services;
using QMS.Application.Profiles;
using QMS.Api.Hubs;
using QMS.Api.Services;

namespace QMS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ?? Authentication ????????????????????????????????????????????????????????????????????
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:IssuerUrl"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:AudienceUrl"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };

                // Allow SignalR to receive the JWT from the query string,
                // because browsers can't set Authorization headers on WebSocket connections.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            context.Token = accessToken;
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();

            // ?? CORS ??????????????????????????????????????????????????????????????????????????????
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy", policy =>
                {
                    policy
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .SetIsOriginAllowed(_ => true);
                });
            });

            // ?? Database ??????????????????????????????????????????????????????????????????????????
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("cs"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // ?? SignalR ???????????????????????????????????????????????????????????????????????????
            builder.Services.AddSignalR();

            // ?? DI registrations ?????????????????????????????????????????????????????????????????
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IBranchService, BranchService>();
            builder.Services.AddScoped<IUtilityService, UtilityService>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<ITicketService, TicketService_WithSignalR>();
            builder.Services.AddScoped<IQueueService, QueueService>();
            builder.Services.AddScoped<IDisplayScreenService, DisplayScreenService>();

            // SignalR notification service — implements the Application-layer abstraction
            builder.Services.AddScoped<IQueueNotificationService, SignalRNotificationService>();

            builder.Services.AddAutoMapper(options =>
            {
                options.AddProfile<UserProfile>();
                options.AddProfile<BranchProfile>();
                options.AddProfile<TicketProfile>();
                options.AddProfile<ServiceProfile>();
                options.AddProfile<DisplayScreenProfile>();
            });

            builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ?? Pipeline ??????????????????????????????????????????????????????????????????????????
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("MyPolicy");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Map the SignalR hub — all hub endpoints live under /hubs/
            app.MapHub<QueueHub>("/hubs/queue");

            app.Run();
        }
    }
}