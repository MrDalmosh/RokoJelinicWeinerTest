using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RokoJelinicWeinerTest.Data;
using RokoJelinicWeinerTest.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace RokoJelinicWeinerTest.Controllers
{
    public class PoliciesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string _connectionString;

        public PoliciesController(ApplicationDbContext context, IConfiguration configuration)
        {
            
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: Policies
        public async Task<IActionResult> Index()
        {
            using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            {
                var policiesDictionary = new Dictionary<string, PoliciesModel>();

                var policies = await dbConnection.QueryAsync<PoliciesModel, PartnersPolicies, PartnersModel, PoliciesModel>(
    @"SELECT po.*, pp.*, p.* 
FROM Policies po
LEFT JOIN PartnersPolicies pp ON po.PolicyNumber = pp.PolicyNumber 
LEFT JOIN Partners p ON pp.PartnerId = p.Id
ORDER BY po.PolicyNumber",
    (policy, partnerPolicy, partner) =>
    {
        if (!policiesDictionary.TryGetValue(policy.PolicyNumber, out var existingPolicy))
        {
            existingPolicy = policy;
            existingPolicy.PartnersPolicies = new List<PartnersPolicies>();
            policiesDictionary.Add(existingPolicy.PolicyNumber, existingPolicy);
        }
        if (partner != null)
        {
            partnerPolicy.Partner = partner;
            existingPolicy.PartnersPolicies.Add(partnerPolicy);
        }
        return existingPolicy;
    },
    splitOn: "PolicyNumber,Id");

                var distinctPolicies = policiesDictionary.Values.ToList();

                return View(distinctPolicies);
            }
        }


        public async Task<IActionResult> Search(string category, string searchTerm)
        {

            if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(searchTerm))
            {

                return RedirectToAction("Index");
            }

            using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            {
                string sqlQuery = $"SELECT * FROM Policies WHERE {category} LIKE '%{searchTerm}%'";


                var policies = await dbConnection.QueryAsync<PoliciesModel>(sqlQuery);

                return View("Index", policies);
            }
        }


        // GET: Policies/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            {
                var policyDictionary = new Dictionary<string, PoliciesModel>();

                var policy = await dbConnection.QueryAsync<PoliciesModel, PartnersPolicies, PartnersModel, PoliciesModel>(
                    @"SELECT po.*, pp.*, p.* 
               FROM Policies po
               LEFT JOIN PartnersPolicies pp ON po.PolicyNumber = pp.PolicyNumber 
               LEFT JOIN Partners p ON pp.PartnerId = p.Id
               WHERE po.PolicyNumber = @Id",
                    (pol, partnerPolicy, partner) =>
                    {
                        if (!policyDictionary.TryGetValue(pol.PolicyNumber, out var existingPolicy))
                        {
                            existingPolicy = pol;
                            existingPolicy.PartnersPolicies = new List<PartnersPolicies>();
                            policyDictionary.Add(existingPolicy.PolicyNumber, existingPolicy);
                        }
                        if (partner != null)
                        {
                            partnerPolicy.Partner = partner;
                            existingPolicy.PartnersPolicies.Add(partnerPolicy);
                        }
                        return existingPolicy;
                    },
                    splitOn: "PolicyNumber,Id",
                    param: new { Id = id });

                var selectedPolicy = policy.FirstOrDefault(); // Get the first (and only) policy returned

                if (selectedPolicy == null)
                {
                    return NotFound();
                }

                return View(selectedPolicy);
            }
        }

        // GET: Policies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Policies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PolicyNumber,PolicyPrice")] PoliciesModel policiesModel)
        {
            if (ModelState.IsValid)
            {
                using (IDbConnection dbConnection = new SqlConnection(_connectionString))
                {
                    string sql = "INSERT INTO Policies (PolicyNumber, PolicyPrice) VALUES (@PolicyNumber, @PolicyPrice)";
                    await dbConnection.ExecuteAsync(sql, policiesModel);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(policiesModel);
        }

        // GET: Policies/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            {
                var policiesModel = await dbConnection.QueryFirstOrDefaultAsync<PoliciesModel>(
                    "SELECT * FROM Policies WHERE PolicyNumber = @PolicyNumber", new { PolicyNumber = id });

                if (policiesModel == null)
                {
                    return NotFound();
                }

                return View(policiesModel);
            }
        }

        // POST: Policies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("PolicyNumber,PolicyPrice")] PoliciesModel policiesModel)
        {
            if (id != policiesModel.PolicyNumber.ToString())
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                using (IDbConnection dbConnection = new SqlConnection(_connectionString))
                {
                    try
                    {
                        await dbConnection.ExecuteAsync(
                            "UPDATE Policies SET PolicyPrice = @PolicyPrice WHERE PolicyNumber = @PolicyNumber",
                            new { PolicyPrice = policiesModel.PolicyPrice, PolicyNumber = policiesModel.PolicyNumber });

                        return RedirectToAction(nameof(Index));
                    }
                    catch (SqlException ex)
                    {
                        if (!await PoliciesModelExists(policiesModel.PolicyNumber))
                        {
                            return NotFound();
                        }
                        else
                        {
                            
                            throw;
                        }
                    }
                }
            }
            return View(policiesModel);
        }


        // GET: Policies/Delete/5
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            {
                var policy = await dbConnection.QueryFirstOrDefaultAsync<PoliciesModel>(
                    "SELECT * FROM Policies WHERE PolicyNumber = @PolicyNumber", new { PolicyNumber = id });

                if (policy == null)
                {
                    return NotFound();
                }

                return View(policy);
            }
        }

        // POST: Policies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            {
                var affectedRows = await dbConnection.ExecuteAsync(
                    "DELETE FROM Policies WHERE PolicyNumber = @PolicyNumber", new { PolicyNumber = id });

                if (affectedRows > 0)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return NotFound();
                }
            }
        }

        private async Task<bool> PoliciesModelExists(string id)
        {
            using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            {
                var policy = await dbConnection.QueryFirstOrDefaultAsync<PoliciesModel>(
                    "SELECT TOP 1 * FROM Policies WHERE PolicyNumber = @PolicyNumber", new { PolicyNumber = id });

                return policy != null;
            }
        }
    }
}
