using DrumBuddy.Endpoint.Extensions;
using DrumBuddy.Endpoint.Services;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
var builder = WebApplication.CreateBuilder(args);



builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.Services.AddApplicationDbContext(builder.Configuration);
builder.Services.AddApplicationIdentity();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApplicationCors();

var app = builder.Build();

// Configure middleware and map endpoints
app.UseApplicationMiddleware();
app.MapApplicationEndpoints();

app.Run();
