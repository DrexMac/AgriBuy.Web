using AgriBuy.Contracts;
using AgriBuy.Contracts.MapperProfiles;
using AgriBuy.EntityFramework;
using AgriBuy.MySql;
using AgriBuy.Services;
using AgriBuy.Services.Checkout; 
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


builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ILoginInfoService, LoginInfoService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
builder.Services.AddScoped<IStoreService, StoreService>();


builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddHttpClient("MayaClient", client =>
{
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

// Add Razor Pages and session support
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
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession();

app.MapRazorPages();

app.Run();
