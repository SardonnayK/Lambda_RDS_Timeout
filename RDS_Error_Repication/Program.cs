using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RDS_Error_Repication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;
services.AddAWSLambdaHosting(LambdaEventSource.RestApi);
var configuration = builder.Configuration;
services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();


if (false)
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    services.AddDbContextFactory<DBContext>(options =>
        options.UseLazyLoadingProxies().UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString),
            x => x.EnableRetryOnFailure(2)
           ).LogTo(Console.WriteLine));

    Console.WriteLine("The connection string is " + connectionString);
    Console.WriteLine("Attempting to seed database");
    using (var sp = services.BuildServiceProvider())
    using (var readDBContext = sp.GetService<DBContext>())
    {
        readDBContext?.Database.EnsureCreated();
        Console.WriteLine("Ensure Creation Complete");

        readDBContext.SaveChanges();
    }

}
else
{

    string connectionString = getConnectionString(builder.Configuration);
    services.AddDbContextFactory<DBContext>(options =>
    {
        options.UseLazyLoadingProxies().UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString),
            x => x.EnableRetryOnFailure(2)
           ).LogTo(Console.WriteLine);
    });

    services.AddDbContext<DBContext>(options =>
    {
        options.UseLazyLoadingProxies().UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString),
            x => x.EnableRetryOnFailure(2)
           ).LogTo(Console.WriteLine);
    });


    Console.WriteLine("The connection string is " + connectionString);
}


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();




string getConnectionString(IConfiguration configuration)
{
    var DB_HOST = Environment.GetEnvironmentVariable("DB_HOSTNAME");
    var region = configuration.GetValue<string>("AWS_REGION") ?? RegionEndpoint.AFSouth1.SystemName;
    var regionEndpoint = RegionEndpoint.GetBySystemName(region);
    var config = new AmazonSecretsManagerConfig { RegionEndpoint = regionEndpoint };

    var client = new AmazonSecretsManagerClient(config);
    var secretName = Environment.GetEnvironmentVariable("SECRET_NAME") ?? "dev";

    var request = new GetSecretValueRequest
    {
        SecretId = secretName
    };

    GetSecretValueResponse response = null;

    response = client.GetSecretValueAsync(request).GetAwaiter().GetResult();
    var secret = JsonConvert.DeserializeObject<SecretManager>(response.SecretString);
    var connectionString = secret.connectionString(DB_HOST);
    return connectionString;
}
