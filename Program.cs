using Orleans.Configuration;

var builder = WebApplication.CreateBuilder();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (builder.Environment.IsDevelopment())
{
    builder.Host.UseOrleans(builder =>
    {
        builder.UseLocalhostClustering();
        builder.AddMemoryGrainStorage("emailStore");
    });
}
else
{
    builder.Host.UseOrleans(builder =>
    {
        var connectionString = "your_storage_connection_string";

        builder.AddAzureBlobGrainStorage("emailStore", options => options.ConfigureBlobServiceClient(connectionString));
        builder.Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "emails-storage";
            options.ServiceId = "emails";
        });
    });
}

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