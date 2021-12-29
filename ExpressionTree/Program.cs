using ExpressionTree.Model.EFCoreContext;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Debug;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CoreContext>(options =>
{
    options.UseSqlServer("Data Source=localhost;Initial Catalog=db_test;Integrated Security=True;Pooling=False")
           .UseLoggerFactory(new LoggerFactory(new[]
           {
               new DebugLoggerProvider()
           }));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
