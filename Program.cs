using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PersistentRegister.Interfaces;
using PersistentRegister.Repositories;
using PersistentRegister.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// builder.Services.AddLogging();
// builder.Logging.AddConsole();
// builder.Logging.AddJsonConsole();
// builder.Logging.AddFile("logs/myapp-{Date}.txt");
// builder.Host.UseSerilog();

// Log.Logger = new LoggerConfiguration

//Register DB Context and use SQL Server
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program).Assembly);

Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

builder.Host.UseSerilog();

#region DI
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
#endregion

var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
