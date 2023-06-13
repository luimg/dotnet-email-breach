var builder = WebApplication.CreateBuilder(args);
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


app.MapGet("/emails/{email}", (string email) =>
{
    return $"Got GET {email}";
})
.WithName("GetEmail")
.WithOpenApi();

app.MapPost("/emails/{email}", (string email) =>
{
    return $"Got POST {email}";
})
.WithName("PostEmail")
.WithOpenApi();

app.Run();