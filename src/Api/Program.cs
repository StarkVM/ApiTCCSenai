//--version 8.0.*
//dotnet user-secrets init
//dotnet user-secrets set "ConnectionStrings:UserAccessDb" "Connection string verdadeira"
//dotnet user-secrets list

using UserAccess.Infrastructure;
using UserAccess.Infrastructure.Persistence;

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

