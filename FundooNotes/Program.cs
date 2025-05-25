using System.Text;
using BusinessLayer.Interface;
using BusinessLayer.Service;
using ConsumerLayer.Consumer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RepoLayer.Context;
using RepoLayer.Helper;
using RepoLayer.Interface;
using RepoLayer.Middleware;
using RepoLayer.Service;
using RepositoryLayer.Middleware;
using NLog;
using NLog.Web;
using StackExchange.Redis;
using System.Reflection;

namespace FundooNotes
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Info("Starting application...");

                var builder = WebApplication.CreateBuilder(args);
                var redisConfig = builder.Configuration.GetSection("Redis:ConnectionString").Value;

                // Use NLog instead of default .NET Logger
                builder.Logging.ClearProviders();
                builder.Host.UseNLog();

                // Add services to the container.
                builder.Services.AddDbContext<UserContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("StartingConnection")));
                builder.Services.AddScoped<IUserRL, UserImplRL>();
                builder.Services.AddScoped<IUserBL, UserImplBL>();
                builder.Services.AddScoped<INotesBL, NotesImplBL>();
                builder.Services.AddScoped<INotesRL, NotesImplRL>();
                builder.Services.AddScoped<ILabelBL, LabelImplBL>();
                builder.Services.AddScoped<ILabelRL, LabelImplRL>();
                builder.Services.AddScoped<ICollabBL, CollabImplBL>();
                builder.Services.AddScoped<ICollabRL, CollabImplRL>();
                builder.Services.AddScoped<AuthService>();
                builder.Services.AddScoped<PasswordHashService>();
                builder.Services.AddScoped<EmailService>();
                builder.Services.AddScoped<RabbitMQProducer>();
                builder.Services.AddScoped<RabbitMQConsumer>();
                builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConfig));

                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();

                // JWT Authentication
                var key = Encoding.ASCII.GetBytes(builder.Configuration["JwtSettings:Key"]);
                builder.Services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });

                // Swagger configuration with JWT
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fundoo Notes", Version = "v1" });

                    var securityScheme = new OpenApiSecurityScheme
                    {
                        Name = "JWT Authentication",
                        Description = "Enter JWT token **_only_**",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        Reference = new OpenApiReference
                        {
                            Id = JwtBearerDefaults.AuthenticationScheme,
                            Type = ReferenceType.SecurityScheme
                        }
                    };
                    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {securityScheme, Array.Empty<string>()}
                    });
                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                });
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAll", policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
                });
                builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
                logger.Info("App build successful. Starting request pipeline...");

                var app = builder.Build();
                app.MapReverseProxy();

                // HTTP request pipeline
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }
                app.UseHttpsRedirection();
                app.UseMiddleware<ExceptionMiddleware>();
                app.UseMiddleware<UnauthorisedMiddleware>();
                app.UseCors("AllowAll");
                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Application stopped due to an exception");
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }
    }
}
