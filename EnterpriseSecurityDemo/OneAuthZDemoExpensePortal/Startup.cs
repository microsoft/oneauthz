namespace ExpenseDemo
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using ExpenseDemo.Services;
    using ExpenseDemo.Services.Interfaces;
    using ExpenseDemo.Models;
    using Microsoft.Enterprise.Authorization.Client.Middleware;
    using Microsoft.AspNetCore.Authorization.Infrastructure;
    using Microsoft.AspNetCore.Authorization;
    using ExpenseDemo.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Enterprise.Authorization.Client.Experimental;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Environment Specific
            var mode = this.Configuration.GetValue<Mode>("ExpenseDemoOptions:Mode");
            if(mode == Mode.Service)
            {
                services.AddScoped<IExpenseRepository, ExpenseRepository>();
                services.AddScoped<IAttributeMetadataService, AttributeStoreService>();
                services.AddScoped<IAzureKustoService, AzureKustoService>();
                services.AddScoped<IPolicyDecisionPoint, RemotePolicyDecisionPoint>();
                services.AddScoped<IAttributeAssignmentService, AttributeStoreService>();

                services.AddAadAuthorization(options =>
                {
                    this.Configuration.GetSection("AuthorizationClient").Bind(options);
                    this.Configuration.GetSection("ExpenseDemoOptions:ClientOptions").Bind(options.AuthenticationOptions);
                });
            }
            else if (mode == Mode.Local)
            {
                services.AddSingleton<IExpenseRepository, LocalFileExpenseRepository>();
                services.AddSingleton<IAttributeMetadataService, StaticAttributeMetadataService>();
                services.AddSingleton<IAzureKustoService, StaticKustoService>();
                services.AddScoped<IPolicyDecisionPoint, LocalPolicyDecisionPoint>();
                services.AddSingleton<IAttributeAssignmentService, LocalAttributeStore>();

                services.AddLocalAadAuthorization(options =>
                {
                    this.Configuration.GetSection("LocalAuthorizationClientOptions").Bind(options);
                });

                services.Configure<LocalAuthorizationClientOptions>(this.Configuration.GetSection("LocalAuthorizationClientOptions"));
                services.AddSingleton<IPolicyAdministrationPoint, LocalPolicyAdministrationPoint>();
            }

            // Common Dependencies
            services.Configure<ExpenseDemoOptions>(this.Configuration.GetSection("ExpenseDemoOptions"));
            services.AddScoped<IUserMetadataService, AttributeStoreService>();
            services.AddScoped<AuthorizationService>();

            // Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var clientOptions = new ClientOptions();
                this.Configuration.Bind("ExpenseDemoOptions:ClientOptions", clientOptions);
                options.Audience = clientOptions.ClientId;
                options.Authority = clientOptions.Authority;
            })
            .AddJwtBearer("AdminUI", options =>
            {
                var clientOptions = new ClientOptions();
                this.Configuration.Bind("ExpenseDemoOptions:ClientOptions", clientOptions);
                options.Audience = clientOptions.AdminUIClientId;
                options.Authority = clientOptions.Authority;
            });

            // Authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy(AadCheckAccessAuthorizeAttribute.PolicyName,
                    policy =>
                    {
                        policy.Requirements.Add(new DenyAnonymousAuthorizationRequirement());
                        policy.Requirements.Add(new AadAuthorizationCheckAccessRequirement());
                    });
            });
            services.AddScoped<IAuthorizationHandler>(serviceProvider =>
            {
                var authorizatioService = serviceProvider.GetRequiredService<AuthorizationService>();
                var logger = serviceProvider.GetService<Microsoft.Enterprise.Authorization.Client.Internal.ILogger>();

                return new ExpenseAuthorizationHandler(authorizatioService, logger);
            });

            //Temporarily enable CORS on Local PAP
            services.AddCors(options =>
            {
                options.AddPolicy("LocalPAPCors",
                builder =>
                {
                    builder.WithOrigins("https://localhost:44315").AllowAnyMethod().AllowAnyHeader();
                });
            });

            // Framework
            services.AddMemoryCache();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMvc();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                await next();
            });

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc();

        }
    }
}
