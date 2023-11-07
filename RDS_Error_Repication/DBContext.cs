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

        }
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = GetConnectionString();
                if (connectionString != null)
                {
                    optionsBuilder.UseLazyLoadingProxies().UseNpgsql(
                        connectionString,
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
            using (var context = new DBContext())
            {
                context.Customers.Add(new CustomerModel
                {
                    UUID = "26Yesql5fceBxmLRrWTecm6eolH3",
                    ContactInformation = new ContactInformationModel
                    {
                        Email = "test@gmail.com",
                        CellNumber = "0844456552",
                        CustomerId = "26Yesql5fceBxmLRrWTecm6eolH3",
                    },
                    PopiInfo = new POPIInfoModel
                    {
                        Marketing = false,
                        TermsAndConditions = false,
                    },
                    Name = "First Name",
                    Surname = "Last Name",
                    ExternalId = "asdasd",
                });

                context.SaveChanges();
            }
            base.OnModelCreating(builder);
        }


        public virtual DbSet<CustomerModel> Customers { get; set; }
    }
}
