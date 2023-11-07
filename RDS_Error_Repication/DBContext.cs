using Microsoft.EntityFrameworkCore;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon;
using Newtonsoft.Json;
using static RDS_Error_Repication.ContactInformationModelConfiguration;

namespace RDS_Error_Repication
{
    public class DBContext : DbContext
    {
        public DBContext()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = GetConnectionString();
                if (connectionString != null)
                {
                    optionsBuilder.UseLazyLoadingProxies().UseMySql(
                        connectionString,
                         ServerVersion.AutoDetect(connectionString),
                        x => x.EnableRetryOnFailure(2)
                       );
                    return;
                }

                base.OnConfiguring(optionsBuilder);
            }
        }

        private string GetConnectionString()
        {
            var config = new AmazonSecretsManagerConfig { RegionEndpoint = RegionEndpoint.AFSouth1 };
            var client = new AmazonSecretsManagerClient(config);
            var secretName = Environment.GetEnvironmentVariable("SECRET_NAME");
            if (string.IsNullOrEmpty(secretName))
            {
                return "Host = localhost; Database = proxyTest; Username = postgres; Pwd = secret; ";
            }

            var request = new GetSecretValueRequest
            {
                SecretId = secretName
            };

            GetSecretValueResponse response = null;


            response = client.GetSecretValueAsync(request).GetAwaiter().GetResult();
            var secret = JsonConvert.DeserializeObject<SecretManager>(response.SecretString);

            var connectionString = secret.connectionString();
            return connectionString;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            new CustomerModelConfiguration().Configure(builder.Entity<CustomerModel>());
            new PopiInfoModelConfiguration().Configure(builder.Entity<POPIInfoModel>());
            new ContactInformationModelConfiguration().Configure(builder.Entity<ContactInformationModel>());
            new CustomerInterestBridgeConfiguration().Configure(builder.Entity<CustomerInterestBridge>());
            new FavouriteStoreBridgeConfiguration().Configure(builder.Entity<FavouriteStoreBridge>());
            new FavouriteProductBridgeConfiguration().Configure(builder.Entity<FavouriteProductBridge>());

            base.OnModelCreating(builder);
        }


        public virtual DbSet<CustomerModel> Customers { get; set; }
    }
}
