namespace RDS_Error_Repication
{
    public class ContactInformationModel
    {
        public string Id { get; set; }
        public string CellNumber { get; set; }
        public string Email { get; set; }
        public string CustomerId { get; set; }
        public ContactInformationModel()
        {

        }

        public virtual CustomerModel Customer { get; set; }
    }


    public abstract class AuditableEntity
    {
        public DateTime Created { get; set; } = DateTime.UtcNow;

        public string CreatedBy { get; set; }

        public DateTime? LastModified { get; protected set; }

        public string? LastModifiedBy { get; protected set; }
    }
    public class POPIInfoModel : AuditableEntity
    {
        public string Id { get; set; }
        public bool TermsAndConditions { get; set; }
        public bool Marketing { get; set; }
        public virtual CustomerModel Customer { get; set; }


        public POPIInfoModel()
        {

        }
        public POPIInfoModel(bool Marketing, bool TermsAndConditions, string OwnerId)
        {
            this.Marketing = Marketing;
            this.CreatedBy = OwnerId;
            this.TermsAndConditions = TermsAndConditions;
        }
    }

    public class CustomerInterestBridge
    {
        public string CustomerUUID { get; set; }
        public string CategoryId { get; set; }
        public virtual CustomerModel Customer { get; set; }
        public virtual CategoryModel Category { get; set; }

        public int Weight { get; set; }

        public CustomerInterestBridge()
        {
            Weight = 1;
        }

    }
    public class CategoryModel
    {
        public string Id { get; set; }
        public string? ParentCategoryId { get; set; }
        public string Name { get; set; }
        public virtual ICollection<CustomerInterestBridge> Customers { get; set; }
        public virtual CategoryModel? ParentCategory { get; set; }

        public CategoryModel()
        {
            Customers = new List<CustomerInterestBridge>();
        }

        public CategoryModel(string id, string name, string? parentCategoryId = null)
        {
            Id = id;
            Name = name;
            ParentCategoryId = parentCategoryId;
            Customers = new List<CustomerInterestBridge>();
        }


    }

    public class FavouriteStoreBridge
    {
        public virtual CustomerModel Customer { get; set; }
        public string CustomerId { get; set; }
        public string StoreId { get; set; }
        public string MallId { get; set; }
        public int Weight { get; set; }
        public FavouriteStoreBridge(
            string customerId,
            string storeId,
            string mallId,
            int weight = 1
        )
        {
            this.CustomerId = customerId;
            this.StoreId = storeId;
            this.MallId = mallId;
            this.Weight = weight;
        }


        public class FavouriteProductBridge
        {
            public virtual CustomerModel Customer { get; set; }
            public string CustomerId { get; set; }
            public string ProductId { get; set; }
            public string StoreId { get; set; }
            public string MallId { get; set; }
            public int Weight { get; set; }
            public FavouriteProductBridge(
                string customerId,
                string productId,
                string storeId,
                string mallId,
                int weight = 1
            )
            {
                this.CustomerId = customerId;
                this.ProductId = productId;
                this.StoreId = storeId;
                this.MallId = mallId;
                this.Weight = weight;
            }


            public FavouriteProductBridge()
            {

            }

        }
    }




    public class FavouriteProductBridge
    {
        public virtual CustomerModel Customer { get; set; }
        public string CustomerId { get; set; }
        public string ProductId { get; set; }
        public string StoreId { get; set; }
        public string MallId { get; set; }
        public int Weight { get; set; }
        public FavouriteProductBridge(
            string customerId,
            string productId,
            string storeId,
            string mallId,
            int weight = 1
        )
        {
            this.CustomerId = customerId;
            this.ProductId = productId;
            this.StoreId = storeId;
            this.MallId = mallId;
            this.Weight = weight;
        }


        public FavouriteProductBridge()
        {

        }

    }
}
