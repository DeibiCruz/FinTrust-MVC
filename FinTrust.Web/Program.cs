
using FinTrust.Web.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Servicios MVC
builder.Services.AddControllersWithViews();

// Conexion a SQL Server
builder.Services.AddDbContext<FinTrustDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("FinTrustConnection")
    ));

var app = builder.Build();

// Crear o actualizar la base de datos con las migraciones
using (var scope = app.Services.CreateScope())
{
    var contexto = scope.ServiceProvider.GetRequiredService<FinTrustDbContext>();
    contexto.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));
    contexto.Database.Migrate();

    if (app.Environment.IsDevelopment())
    {
        DatosDePrueba.Cargar(contexto);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
