using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using mapapi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace mapapi
{
    public class Startup
    {
        private string secretKey = null;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private string connectionString;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddHostedService<FScoreHostedService>();
            //services.AddHostedService<ActivityScoreHostedService>();

            //$Env:dbconnstring = "Host=localhost;Port=<port>;Database=<database>;Username=<username>;Password=5<pass>;"
            connectionString = Environment.GetEnvironmentVariable("dbconnstring");
            //connectionString = Configuration.GetValue<string>("AppSettings:Dbconnstring");
            services.AddCors();
            //services.AddMvc().AddNewtonsoftJson();
            services.AddControllers()
                //.AddNewtonsoftJson(x => x.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.IsoDateTimeConverter()));
                //.AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
                .AddJsonOptions(options =>
                 {
                     //options.JsonSerializerOptions.Converters.Add(new Newtonsoft.Json.Converters.IsoDateTimeConverter());
                     options.JsonSerializerOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
                 });
            //services.AddDbContext<geosocdbContext>(options => options.UseNpgsql(Configuration["ConnectionStrings:geosocdbcontext"], x => x.UseNetTopologySuite()));
            services.AddDbContext<GeosocdbContext>(options => options.UseNpgsql(connectionString, x => x.UseNetTopologySuite()));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // укзывает, будет ли валидироваться издатель при валидации токена
                        ValidateIssuer = true,
                        // строка, представляющая издателя
                        ValidIssuer = "MapApiServer",

                        // будет ли валидироваться потребитель токена
                        ValidateAudience = true,
                        // установка потребителя токена
                        ValidAudience = "MapApiClient",
                        // будет ли валидироваться время существования
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,

                        // установка ключа безопасности
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetValue<string>(
                            "AppSettings:Secret"))),
                        // валидация ключа безопасности
                        ValidateIssuerSigningKey = true,
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                context.Response.Headers.Add("Token-Expired", "true");
                            }
                            return Task.CompletedTask;
                        },
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            // если запрос направлен хабу
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                (path.StartsWithSegments("/chat")))
                            {
                                // получаем токен из строки запроса
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                    
                });


            //services.AddAuthorization(options =>
            //    options.AddPolicy("ValidAccessToken", policy =>
            //    {
            //        policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
            //        policy.RequireAuthenticatedUser();
            //    }));

            // установка конфигурации подключения
            //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //    .AddCookie(options => //CookieAuthenticationOptions
            //    {
            //        options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
            //    });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(options =>
            {
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field. Example: Bearer {token}",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                      new string[] { }
                    }
                });
            });
            services.AddSignalR(o =>
            {
                o.EnableDetailedErrors = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //For monitoring exception messages on clients while development process
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //    // FOR MONITORING EXCEPTION MESSAGES ON CLIENTS DURING DEVELOPMENT PROCESS
            //    // DISSABLE ON PRODUCTION
            //else
                app.UseExceptionHandler("/error");

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();


            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "mapapi");
            });

            app.UseHttpsRedirection();
            /////////////////
            //app.UseDefaultFiles();

            app.UseStaticFiles();
            ////////////////
            app.UseRouting();

            app.UseCors(builder => builder.AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed(origin => true).AllowCredentials());
            app.UseAuthentication();

            app.UseAuthorization();

            //app.UseMiddleware<ApiKeyMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapHub<Hubs.ChatHub>("/chat");
            });
        }
    }
}
