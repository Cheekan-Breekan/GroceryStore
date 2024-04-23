using GroceryStore.Application.Auth;
using GroceryStore.Application.Services;
using GroceryStore.Core.Entities;
using GroceryStore.Core.Interfaces.Repositories;
using GroceryStore.Core.Interfaces.Services;
using GroceryStore.Core.Interfaces;
using GroceryStore.Infrastructure;
using GroceryStore.Persistance.Repositories;
using GroceryStore.Persistance;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GroceryStore.MVC.Filters;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddResponseCaching();

        builder.Services.AddControllersWithViews(opts =>
        {
            opts.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            opts.Filters.Add<GlobalExceptionFilter>();
        }).AddRazorRuntimeCompilation();

        //Add DbContext and identity
        var connString = builder.Configuration.GetConnectionString("Docker-mssql");
        builder.Services.AddDbContext<AppBaseDbContext, AppMSSQLDbContext>(opts => {
            opts.UseSqlServer(connString);
            opts.EnableSensitiveDataLogging();
        });
        builder.Services.AddDefaultIdentity<MarketUser>(opts =>
        {
            opts.User.RequireUniqueEmail = true;
            opts.SignIn.RequireConfirmedEmail = true;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppMSSQLDbContext>()
            .AddDefaultTokenProviders();

        //configure cache
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddStackExchangeRedisCache(opts =>
        {
            opts.Configuration = builder.Configuration.GetConnectionString("Redis");
            opts.InstanceName = "TestAppMarket_";
        });

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

        //configure cookies
        builder.Services.ConfigureApplicationCookie(opts =>
        {
            opts.LoginPath = "/Account/Login";
            opts.LogoutPath = "/Account/Logout";
            opts.AccessDeniedPath = "/Account/AccessDenied";
        });

        // Add generated problem details to middleware (RFC 7807 standart)
        builder.Services.AddProblemDetails();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler();
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }

        // Обход того, что в Ajax реквест прилетает рендер вью еррор пейджа, а не ошибка со статусом.
        app.UseStatusCodePages(async context =>
        {
            var request = context.HttpContext.Request;
            var response = context.HttpContext.Response;

            if (request.Headers.XRequestedWith == "XMLHttpRequest")
            {
                // Если ajax, то возврат статуса ошибки
                response.ContentType = "application/json";
                await response.WriteAsync("{\"status\": " + (int)context.HttpContext.Response.StatusCode + "}");
            }
            else
            {
                // Если нет, то редирект
                context.HttpContext.Response.Redirect("/Error/" + context.HttpContext.Response.StatusCode);
            }
        });

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapDefaultControllerRoute();

        app.Run();
    }
}