using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace RDS_Error_Repication.Controllers
{
    /// <summary>
    /// Controller for testing database context and factory
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly DBContext _dbContext;
        private readonly IDbContextFactory<DBContext> _dbContextFactory;

        public TestController(ILogger<TestController> logger, DBContext dbContext, IDbContextFactory<DBContext> dbContextFactory)
        {
            _logger = logger;
            _dbContext = dbContext;
            _dbContextFactory = dbContextFactory;
        }

        /// <summary>
        /// Adds a user to the database for testing purposes
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("/Add/DBContext/Customer")]
        public async Task<IActionResult> add()
        {
            _dbContext.Customers.Add(new CustomerModel
            {
                Created = DateTime.Now,
                UUID = "26Yesql5fceBxmLRrWTecm6eolH3",
                ContactInformation = new ContactInformationModel
                {
                    Id = "uuid",
                    Email = "test@gmail.com",
                    CellNumber = "0844456552",
                    CustomerId = "26Yesql5fceBxmLRrWTecm6eolH3",
                },
                PopiInfo = new POPIInfoModel
                {
                    Id = "uuid",
                    Marketing = false,
                    TermsAndConditions = false,
                    CreatedBy = "26Yesql5fceBxmLRrWTecm6eolH3"
                },
                Name = "First Name",
                Surname = "Last Name",
                ExternalId = "asdasd",
            });
            _dbContext.SaveChanges();
            return Ok("User Created");
        }

        /// <summary>
        /// Gets the user will all the relationships attached
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("/DBContextRead")]
        public async Task<IActionResult> DBContextRead()
        {
            Console.WriteLine("The Method is entered");
            var sw = new Stopwatch();
            sw.Start();
            var customerModel = await _dbContext.Customers
            .Include(customer => customer.Interests)
            .Include(customer => customer.Stores)
            .Include(customer => customer.ContactInformation)
            .Include(customer => customer.PopiInfo)
            .Include(customer => customer.Products)
            .AsSingleQuery()
            .FirstOrDefaultAsync(c => c.UUID == "26Yesql5fceBxmLRrWTecm6eolH3");
            sw.Stop();
            Console.WriteLine($"The Method took {sw.ElapsedMilliseconds} ms to execute");
            return Ok(customerModel?.Name);
        }

        /// <summary>
        /// Gets the Customer Only withou the relationships attached
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("DBContext/Read/Single/Customer")]
        public async Task<IActionResult> DBContextReadSingleCustomer()
        {
            Console.WriteLine("The Method is entered");
            var sw = new Stopwatch();
            sw.Start();
            var customerModel = await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.UUID == "26Yesql5fceBxmLRrWTecm6eolH3");
            sw.Stop();
            Console.WriteLine($"The Method took {sw.ElapsedMilliseconds} ms to execute");
            return Ok(customerModel?.Name);
        }

        /// <summary>
        /// Reads the list of Customers without any relationships attached
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("DBContext/Read/Customers")]
        public async Task<IActionResult> DBContextReadCustomer()
        {
            Console.WriteLine("The Method is entered");
            var sw = new Stopwatch();
            sw.Start();
            var customerModels = _dbContext.Customers.ToList();
            sw.Stop();
            Console.WriteLine($"The Method took {sw.ElapsedMilliseconds} ms to execute");
            return Ok(customerModels[0]?.Name);
        }

        /// <summary>
        /// Reads the Customer With their relationships Using a DBContext Factory
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("DBContextFactory/Read")]
        public async Task<IActionResult> DBContextFactoryRead()
        {

            Console.WriteLine("The Method is entered");
            var sw = new Stopwatch();
            sw.Start();
            using var dbContext = _dbContextFactory.CreateDbContext();

            var customerModel = await dbContext.Customers
            .Include(customer => customer.Interests)
            .Include(customer => customer.Stores)
            .Include(customer => customer.ContactInformation)
            .Include(customer => customer.PopiInfo)
            .Include(customer => customer.Products)
            .AsSingleQuery()
            .FirstOrDefaultAsync(c => c.UUID == "26Yesql5fceBxmLRrWTecm6eolH3");
            sw.Stop();
            Console.WriteLine($"The Method took {sw.ElapsedMilliseconds} ms to execute");
            return Ok(customerModel?.Name);
        }

    }
}