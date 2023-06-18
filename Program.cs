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
        var connectionString = "";
        builder.UseAzureStorageClustering(options =>
            options.ConfigureTableServiceClient(connectionString))
        .AddAzureBlobGrainStorage("emailStore", options => options.ConfigureBlobServiceClient(connectionString));
    });
}

var app = builder.Build();
// this should be always available, because I want to save it to cloud with Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapGet("/emails/{email}",
    async (IGrainFactory grains, HttpRequest request, string email) =>
    {
        var emailSplit = email.Trim().Split("@");
        if (emailSplit.Length != 2)
        {
            return Results.BadRequest();
        }

        var domain = emailSplit.Last().ToString().Trim();
        if (domain.Split(".").Length != 2)
        {
            return Results.BadRequest();
        }
        var emailGrain = grains.GetGrain<IEmailGrain>(email);
        var exists = await emailGrain.GetEmail();

        if (exists == null)
        {
            return Results.NotFound();
        }
        else
        {
            return Results.Ok();
        }
    })
.WithName("GetEmail")
.WithOpenApi();

app.MapPost("/emails/{email}",
    async (IGrainFactory grains, HttpRequest request, string email) =>
    {
        var emailSplit = email.Trim().Split("@");
        if (emailSplit.Length != 2)
        {
            return Results.BadRequest();
        }

        var domain = emailSplit.Last().ToString().Trim();
        if (domain.Split(".").Length != 2)
        {
            return Results.BadRequest();
        }
        var emailGrain = grains.GetGrain<IEmailGrain>(email);
        var exists = await emailGrain.GetEmail();

        if (exists == null)
        {
            emailGrain.SetSavingTimer();
            return Results.Created($"/emails/{email}", email);
        }
        else
        {
            return Results.Conflict();
        }

    })
.WithName("PostEmail")
.WithOpenApi();

app.Run();