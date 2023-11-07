using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace RDS_Error_Repication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<TestController> _logger;
        private readonly DBContext _dbContext;

        public TestController(ILogger<TestController> logger, DBContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet(Name = "Read Customer")]
        public async Task<IActionResult> Get()
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
    }
}