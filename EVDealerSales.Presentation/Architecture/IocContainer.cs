using EVDealerSales.Business.Interfaces;
using EVDealerSales.Business.Services;
using EVDealerSales.DataAccess;
using EVDealerSales.DataAccess.Commons;
using EVDealerSales.DataAccess.Interfaces;
using EVDealerSales.DataAccess.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace EVDealerSales.Presentation.Architecture
{
    public static class IocContainer
    {
        public static IServiceCollection SetupIocContainer(this IServiceCollection services)
        {
            services.SetupDbContext();

            //Add generic repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            //Add business services
            services.SetupBusinessServicesLayer();

            services.SetupJwt();
            return services;
        }

        private static IServiceCollection SetupDbContext(this IServiceCollection services)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            // Get the connection string from "DefaultConnection"
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Register DbContext with SQL Server
            services.AddDbContext<EVDealerSalesDbContext>(options =>
                options.UseSqlServer(connectionString,
                    sql => sql.MigrationsAssembly(typeof(EVDealerSalesDbContext).Assembly.FullName)
                )
            );

            return services;
        }

        public static IServiceCollection SetupBusinessServicesLayer(this IServiceCollection services)
        {
            // Inject service vào DI container
            services.AddScoped<ICurrentTime, CurrentTime>();
            services.AddScoped<IClaimsService, ClaimsService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<ITestDriveService, TestDriveService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IDeliveryService, DeliveryService>();
            services.AddScoped<IFeedbackService, FeedbackService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IDataAnalyzerService, DataAnalyzerService>();
            services.AddScoped<IGeminiService, GeminiService>();
            services.AddScoped<IChatbotService, ChatbotService>();
            services.AddHttpContextAccessor();

            return services;
        }

        private static IServiceCollection SetupJwt(this IServiceCollection services)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,   // Bật kiểm tra Issuer
                        ValidateAudience = true, // Bật kiểm tra Audience
                        ValidateLifetime = true,
                        ValidIssuer = configuration["JWT:Issuer"],
                        ValidAudience = configuration["JWT:Audience"],
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"] ??
                                                                            throw new InvalidOperationException())),
                        ClockSkew = TimeSpan.Zero,
                        NameClaimType = ClaimTypes.Name,
                        RoleClaimType = ClaimTypes.Role
                    };
                    x.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // Read from Session
                            var token = context.HttpContext.Session.GetString("AuthToken");

                            // For SignalR: read token from query string
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;

                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                            {
                                context.Token = accessToken;
                            }
                            else if (!string.IsNullOrEmpty(token))
                            {
                                context.Token = token;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("CustomerPolicy", policy =>
                    policy.RequireRole("Customer"));

                options.AddPolicy("StaffPolicy", policy =>
                    policy.RequireRole("DealerStaff"));

                options.AddPolicy("ManagerPolicy", policy =>
                    policy.RequireRole("DealerManager"));
            });

            return services;
        }
    }

}
