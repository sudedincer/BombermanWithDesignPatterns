using Bomberman.Services.Network;
using System;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSignalR();
builder.Services.AddSingleton<ExplosionService>();
// Register Repository
// Use a local file database
string dbPath = Path.Combine(Environment.CurrentDirectory, "bomberman.db");
builder.Services.AddSingleton<Bomberman.Services.Data.IUserRepository>(sp => 
    new Bomberman.Services.Data.SqliteUserRepository(dbPath));

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder => builder
        // SetIsOriginAllowed ile yerel IP'lere ve portlara izin veriyoruz.
        .SetIsOriginAllowed(origin => 
        {
            var uri = new Uri(origin);
            // Hem localhost, hem de yerel IP (127.0.0.1) adreslerinden gelen tüm portlara izin ver.
            // Bu, 403 hatasını tamamen ortadan kaldırmalıdır.
            return uri.Host == "localhost" || uri.Host == "127.0.0.1";
        })
        .AllowAnyMethod()
        .AllowAnyHeader()
        // AllowCredentials, sadece yukarıdaki kısıtlı yerel adresler listelendiği için artık çalışır.
        .AllowCredentials());
});

var app = builder.Build();

// 2. CORS politikasını etkinleştir
app.UseCors("CorsPolicy"); 


// 3. SignalR Hub yolunu tanımla
app.MapHub<GameHub>("/gamehub");

app.Run();