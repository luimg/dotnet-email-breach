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
        var connectionString = "";
        builder.UseAzureStorageClustering(options =>
            options.ConfigureTableServiceClient(connectionString))
        .AddAzureTableGrainStorage("emailStore", options => options.ConfigureTableServiceClient(connectionString));
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

app.MapGet("/emails/{email}", 
    async (IGrainFactory grains, HttpRequest request, string email) =>
    {
        var emailSplit = email.Split("@");
        if (emailSplit.Length != 2)
        {
            return Results.BadRequest();
        }

        var domain = emailSplit.Last().ToString().Trim();
        var emailGrain = grains.GetGrain<IEmailGrain>(domain);
        var emailList = await emailGrain.GetEmailsByDomain();

        if (emailList == null || !emailList.Contains(email)) {
            return Results.NotFound();
        } else
        {
            return Results.Ok();
        }
    })
.WithName("GetEmail")
.WithOpenApi();

app.MapPost("/emails/{email}", 
    async (IGrainFactory grains, HttpRequest request, string email) =>
    {
        var emailSplit = email.Split("@");
        if (emailSplit.Length != 2)
        {
            return Results.BadRequest();
        }

        var domain = emailSplit.Last().ToString().Trim();
        var emailGrain = grains.GetGrain<IEmailGrain>(domain);
        var emailList = await emailGrain.GetEmailsByDomain();

        if (emailList == null)
        {
            emailList = new List<String>();
        }

        if (emailList.AsParallel().Contains(email))
        {
            return Results.Conflict();
        } else {
            emailList.Add(email);
            await emailGrain.SetEmailsForDomain(emailList);
            return Results.Created($"/emails/{email}", email);
        }
    })
.WithName("PostEmail")
.WithOpenApi();

app.Run();