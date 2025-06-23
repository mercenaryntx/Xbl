using AutoMapper;
using Xbl.Client;
using Xbl.Data;
using Xbl.Data.Extensions;

var builder = WebApplication.CreateBuilder(args);
var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());

// Add services to the container.
builder.Services.AddSingleton(config.CreateMapper())
    .AddSingleton(sp => new GlobalConfig { DataFolder = sp.GetRequiredService<IConfiguration>().GetValue<string>("DataFolder") })
    .AddData(DataSource.Live, DataSource.Xbox360, DataSource.Dbox, DataSource.Xbl)
    .AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(c => c.AllowAnyOrigin());

app.UseAuthorization();

app.MapControllers();

app.Run();