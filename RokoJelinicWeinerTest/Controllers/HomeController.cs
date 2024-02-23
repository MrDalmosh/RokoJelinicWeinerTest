using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RokoJelinicWeinerTest.Data;
using RokoJelinicWeinerTest.Models;
using System.Diagnostics;
using Dapper;
using System.Data;
using Microsoft.AspNetCore.Identity;

namespace RokoJelinicWeinerTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly string _connectionString;

        public HomeController(ILogger<HomeController> logger, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, ApplicationDbContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IActionResult> Index()
        {
            using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            {
                var partnersDictionary = new Dictionary<int, PartnersModel>();

                var partners = await dbConnection.QueryAsync<PartnersModel, PartnersPolicies, PoliciesModel, PartnersModel>(
                    @"SELECT p.*, pp.*, po.* 
      FROM Partners p 
      LEFT JOIN PartnersPolicies pp ON p.Id = pp.PartnerId 
      LEFT JOIN Policies po ON pp.PolicyNumber = po.PolicyNumber
      ORDER BY p.CreatedAtUtc DESC",
                    (partner, policy, polices) =>
                    {
                        if (!partnersDictionary.TryGetValue(partner.Id, out var existingPartner))
                        {
                            existingPartner = partner;
                            existingPartner.PartnersPolicies = new List<PartnersPolicies>();
                            partnersDictionary.Add(existingPartner.Id, existingPartner);
                        }
                        if (polices != null)
                        {
                            policy.Policies = polices;
                            existingPartner.PartnersPolicies.Add(policy);
                        }
                        return existingPartner;
                    },
                    splitOn: "PartnerId,PolicyNumber");

                var distinctPartners = partnersDictionary.Values.ToList();

                return View(distinctPartners);
            }
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
