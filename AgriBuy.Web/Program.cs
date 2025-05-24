using AgriBuy.Contracts;
using AgriBuy.Contracts.MapperProfiles;
using AgriBuy.EntityFramework;
using AgriBuy.MySql;
using AgriBuy.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContextFactory<DefaultDbContext>(dbContextOptions =>
    dbContextOptions.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnectionString"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnectionString")),
        mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            mySqlOptions.MigrationsAssembly(typeof(Program).Assembly.FullName); 
        })
    .LogTo(Console.WriteLine, LogLevel.Information)
    .EnableSensitiveDataLogging()
    .EnableDetailedErrors()
);


builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddAutoMapper(typeof(AgriBuyMapperProfile));

// Register repositories and services
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ILoginInfoService, LoginInfoService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
builder.Services.AddScoped<IStoreService, StoreService>();

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".AgriBuy.session";
    options.IdleTimeout = TimeSpan.FromSeconds(5000);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession();

app.MapRazorPages();

app.Run();
