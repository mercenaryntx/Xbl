using AutoMapper;
using Xbl.Client;
using Xbl.Client.Io;
using Xbl.Data;
using Xbl.Data.Extensions;
using Xbl.Web;

var builder = WebApplication.CreateBuilder(args);
var config = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<MappingProfile>();
    cfg.AddProfile<Xbl.Client.Infrastructure.MappingProfile>();
});

builder.Services
    .AddSingleton(sp =>
    {
        var c = sp.GetRequiredService<IConfiguration>();
        var s = new Settings();
        c.GetSection("Settings").Bind(s);
        return s;
    })
    .AddSingleton<IConsole, NullConsole>()
    .AddSingleton(config.CreateMapper())
    .AddSingleton(sp => new GlobalConfig
    {
        DataFolder = sp.GetRequiredService<IConfiguration>().GetValue<string>("DataFolder")
    })
    .AddData(DataSource.Live, DataSource.Xbox360, DataSource.Dbox, DataSource.Xbl)
    .AddHttpClient<IXblClient, XblClient>((s, c) =>
    {
        var settings = s.GetRequiredService<Settings>();
        c.DefaultRequestHeaders.Add("x-authorization", settings.ApiKey);
        c.BaseAddress = new Uri("https://xbl.io/api/v2/");
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddResponseCaching();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("X-Titles-Last-Update");
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors();
app.UseAuthorization();
app.UseResponseCaching(); 
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    // Add this for SPA fallback
    endpoints.MapFallbackToFile("index.html");
});

app.Run();