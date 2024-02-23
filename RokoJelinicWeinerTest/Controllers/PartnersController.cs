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
    public class PartnersController : Controller
    {
        private readonly string _connectionString;

        public PartnersController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: Partners
        public async Task<IActionResult> Index()
        {
            return RedirectToAction("Index", "Home");
        }

        // GET: Partners/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sqlQuery = @"
        SELECT p.*, pp.*, po.* 
        FROM Partners p 
        LEFT JOIN PartnersPolicies pp ON p.Id = pp.PartnerId 
        LEFT JOIN Policies po ON pp.PolicyNumber = po.PolicyNumber
        WHERE p.Id = @PartnerId";

            using (var connection = new SqlConnection(_connectionString))
            {
                var partnersDictionary = new Dictionary<int, PartnersModel>();

                await connection.QueryAsync<PartnersModel, PartnersPolicies, PoliciesModel, PartnersModel>(
                    sqlQuery,
                    (partner, partnerPolicy, policy) =>
                    {
                        if (!partnersDictionary.TryGetValue(partner.Id, out var existingPartner))
                        {
                            existingPartner = partner;
                            existingPartner.PartnersPolicies = new List<PartnersPolicies>();
                            partnersDictionary.Add(existingPartner.Id, existingPartner);
                        }

                        if (policy != null)
                        {
                            partnerPolicy.Policies = policy;
                            existingPartner.PartnersPolicies.Add(partnerPolicy);
                        }

                        return existingPartner;
                    },
                    new { PartnerId = id },
                    splitOn: "PartnerId,PolicyNumber");

                var partner = partnersDictionary.Values.FirstOrDefault();

                if (partner == null)
                {
                    return NotFound();
                }

                var allPolicies = await connection.QueryAsync<PoliciesModel>("SELECT * FROM Policies");

                var existingPolicyNumbers = partner.PartnersPolicies.Select(pp => pp.PolicyNumber).ToList();

                var availablePolicies = allPolicies.Where(p => !existingPolicyNumbers.Contains(p.PolicyNumber)).ToList();

                ViewBag.Policies = new SelectList(availablePolicies, "PolicyNumber", "PolicyNumber");

                return View(partner);
            }
        }

        // GET: Partners/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Partners/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Address,PartnerNumber,CroatianPIN,PartnerTypeId,CreatedAtUtc,CreateByUser,IsForeign,ExternalCode,Gender")] PartnersModel partnersModel)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var sqlQueryUniqueCheck = "SELECT COUNT(1) FROM Partners WHERE ExternalCode = @ExternalCode";
                    var count = await connection.ExecuteScalarAsync<int>(sqlQueryUniqueCheck, new { ExternalCode = partnersModel.ExternalCode });

                    if (count > 0)
                    {
                        ModelState.AddModelError(nameof(partnersModel.ExternalCode), "External Code must be unique.");
                        return View(partnersModel);
                    }

                    var sqlQuery = @"
                INSERT INTO Partners (FirstName, LastName, Address, PartnerNumber, CroatianPIN, PartnerTypeId, CreatedAtUtc, CreateByUser, IsForeign, ExternalCode, Gender)
                VALUES (@FirstName, @LastName, @Address, @PartnerNumber, @CroatianPIN, @PartnerTypeId, @CreatedAtUtc, @CreateByUser, @IsForeign, @ExternalCode, @Gender);
                SELECT CAST(SCOPE_IDENTITY() AS int);";

                    var partnerId = await connection.ExecuteScalarAsync<int>(sqlQuery, partnersModel);
                    partnersModel.Id = partnerId;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(partnersModel);
        }

        // GET: Partners/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sqlQuery = "SELECT * FROM Partners WHERE Id = @Id;";

            using (var connection = new SqlConnection(_connectionString))
            {
                var partnersModel = await connection.QueryFirstOrDefaultAsync<PartnersModel>(sqlQuery, new { Id = id });

                if (partnersModel == null)
                {
                    return NotFound();
                }

                return View(partnersModel);
            }
        }

        public async Task<IActionResult> Search(string category, string searchTerm)
        {
            
            if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(searchTerm))
            {
                
                return RedirectToAction("Index", "Home");
            }

            using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            {
                string sqlQuery;


                switch (category)
                {
                    case "FullName":

                        if (searchTerm.Contains(' '))
                        {
                            sqlQuery = "SELECT * FROM Partners WHERE CONCAT(FirstName, ' ', LastName) LIKE @searchTerm";
                        }
                        else
                        {
                            sqlQuery = $"SELECT * FROM Partners WHERE FirstName LIKE @searchTerm OR LastName LIKE @searchTerm";
                        }
                        break;
                    case "IsForeign":
                        bool isForeign;
                        if (bool.TryParse(searchTerm, out isForeign))
                        {
                            sqlQuery = "SELECT * FROM Partners WHERE IsForeign = @IsForeign";
                            var partner = await dbConnection.QueryAsync<PartnersModel>(sqlQuery, new { IsForeign = isForeign });
                            return View("~/Views/Home/Index.cshtml", partner);
                        }
                        else
                        {
                            
                            return RedirectToAction("Index", "Home");
                        }
                    default:
                        sqlQuery = $"SELECT * FROM Partners WHERE {category} LIKE '%{searchTerm}%'";
                        break;
                }


                var partners = await dbConnection.QueryAsync<PartnersModel>(sqlQuery, new { searchTerm = $"%{searchTerm}%" });

                return View("~/Views/Home/Index.cshtml", partners);
            }
        }

        public async Task<IActionResult> RemovePolicy(int id, string policyNumber, string returnUrl)
        {
            string sql = "DELETE FROM PartnersPolicies WHERE PartnerId = @PartnerId AND PolicyNumber = @PolicyNumber";

           
            using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            {
                await dbConnection.ExecuteAsync(sql, new { PartnerId = id, PolicyNumber = policyNumber });
            }

            
            return Redirect(returnUrl);
        }

        // POST: Partners/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Address,PartnerNumber,CroatianPIN,PartnerTypeId,CreatedAtUtc,CreateByUser,IsForeign,ExternalCode,Gender")] PartnersModel partnersModel)
        {
            if (id != partnersModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                using (IDbConnection dbConnection = new SqlConnection(_connectionString))
                {
                    string sql = @"UPDATE Partners 
                           SET FirstName = @FirstName, 
                               LastName = @LastName, 
                               Address = @Address, 
                               PartnerNumber = @PartnerNumber, 
                               CroatianPIN = @CroatianPIN, 
                               PartnerTypeId = @PartnerTypeId, 
                               CreatedAtUtc = @CreatedAtUtc, 
                               CreateByUser = @CreateByUser, 
                               IsForeign = @IsForeign, 
                               ExternalCode = @ExternalCode, 
                               Gender = @Gender
                           WHERE Id = @Id";

                    await dbConnection.ExecuteAsync(sql, partnersModel);
                }

                return RedirectToAction(nameof(Index));
            }

            return View(partnersModel);
        }

        // GET: Partners/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            {
                string sql = "SELECT * FROM Partners WHERE Id = @Id";
                var partnersModel = await dbConnection.QueryFirstOrDefaultAsync<PartnersModel>(sql, new { Id = id });

                if (partnersModel == null)
                {
                    return NotFound();
                }

                return View(partnersModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddPolicy(int id, string policyNumber)
        {
            string query = "INSERT INTO PartnersPolicies (PartnerId, PolicyNumber) " +
                  "VALUES (@PartnerId, @PolicyNumber)";
            using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            {
                try
                {
                    await dbConnection.ExecuteAsync(query, new { PartnerId = id, PolicyNumber = policyNumber });

                    
                    return Ok(new { success = true, partnerId = id });
                }
                catch (Exception ex)
                {
                    // Return error response if policy addition fails
                    return StatusCode(500, new { success = false, message = ex.Message });
                }
            }
        }

        // POST: Partners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            {
                string sql = "DELETE FROM Partners WHERE Id = @Id";
                await dbConnection.ExecuteAsync(sql, new { Id = id });
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> PartnersModelExists(int id)
        {
            using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            {
                string sql = "SELECT COUNT(*) FROM Partners WHERE Id = @Id";
                int count = await dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
                return count > 0;
            }
        }
    }
}
