//--version 8.0.*
//dotnet user-secrets init
//dotnet user-secrets set "ConnectionStrings:UserAccessDb" "Connection string verdadeira"
//dotnet user-secrets list
using Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.AddApiServices();

var app = builder.Build();

app.UseApi();

app.Run();

