//--version 8.0.*
using UserAccess.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddUserAccessInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();

