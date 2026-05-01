using UserService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<UserStore>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/users", async (UserStore store) =>
    Results.Ok(await store.GetAllAsync()));

app.MapGet("/api/users/{id}", async (int id, UserStore store) =>
    await store.GetByIdAsync(id) is { } user
        ? Results.Ok(user)
        : Results.NotFound());

app.Run();
