using GroceryStore.Api;
using GroceryStore.Api.Middlewares;
using GroceryStore.Application.Auth;
using GroceryStore.Application.Services;
using GroceryStore.Core.Entities;
using GroceryStore.Core.Interfaces;
using GroceryStore.Core.Interfaces.Repositories;
using GroceryStore.Core.Interfaces.Services;
using GroceryStore.Infrastructure;
using GroceryStore.Persistance;
using GroceryStore.Persistance.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        //Auth
        builder.Services.AddAuthentication(opts =>
        {
            opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = AuthOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = AuthOptions.Audience,
                    IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                    ValidateIssuerSigningKey = true,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(2)
                };
            });
        builder.Services.AddAuthorization();

        //Database
        builder.Services.AddDbContext<AppBaseDbContext, AppPostgreSQLDbContext>(opts =>
        {
            opts.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"));
            opts.EnableSensitiveDataLogging();
        });

        //Identity
        builder.Services.AddDefaultIdentity<MarketUser>(opts =>
        {
            opts.User.RequireUniqueEmail = true;
            opts.SignIn.RequireConfirmedEmail = true;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppPostgreSQLDbContext>()
            .AddDefaultTokenProviders();

        //Configure cache
        builder.Services.AddResponseCaching();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddStackExchangeRedisCache(opts =>
        {
            opts.Configuration = builder.Configuration.GetConnectionString("Redis");
            opts.InstanceName = "TestAppMarket_";
        });

        //Custom middleware
        builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();

        //DI injection
        builder.Services.AddScoped<IFileService, FileService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<ICartRepository, CartRepository>();
        builder.Services.AddScoped<ICartService, CartService>();
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IEmailSender, EmailSender>();

        //Configure cookies
        builder.Services.ConfigureApplicationCookie(opts =>
        {
            opts.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            };
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Add generated problem details to middleware (RFC 7807 standart)
        builder.Services.AddProblemDetails();

        var app = builder.Build();


        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

        app.MapControllers();

        app.Run();
    }
}