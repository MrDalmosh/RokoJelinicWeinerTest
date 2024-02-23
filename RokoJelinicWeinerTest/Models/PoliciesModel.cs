using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RokoJelinicWeinerTest.Models
{
    public class PoliciesModel
    {
        [Key]
        [Required(ErrorMessage = "Policy number is required.")]
        [StringLength(15, MinimumLength = 10, ErrorMessage = "Policy number must be between 10 and 15 characters.")]
        public string PolicyNumber { get; set; }
        [Required]
        public decimal PolicyPrice { get; set; }

        public ICollection<PartnersPolicies> PartnersPolicies { get; set; } = new List<PartnersPolicies>();
    }
}
