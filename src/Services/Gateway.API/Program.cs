var builder = WebApplication.CreateBuilder(args);

// YARP Reverse Proxy servisini appsettings.json'dan okuyarak ekliyoruz
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// İstekleri ilgili mikroservislere eşleyen middleware
app.MapReverseProxy();

app.Run();