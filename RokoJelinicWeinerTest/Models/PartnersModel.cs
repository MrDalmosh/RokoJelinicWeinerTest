using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace RokoJelinicWeinerTest.Models
{

    [Index(nameof(ExternalCode), IsUnique = true)]
    public class PartnersModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "First Name must be between 2 and 255 characters.")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Last Name must be between 2 and 255 characters.")]
        public string LastName { get; set; }
        [Required]
        public string Address { get; set; }
        [Required(ErrorMessage = "Partner Number is required.")]
        [StringLength(20, ErrorMessage = "Partner Number must be exactly 20 characters.")]
        [RegularExpression("^[0-9]{20}$", ErrorMessage = "Partner Number must be a 20-digit number.")]
        public string PartnerNumber { get; set; }

        [StringLength(11, ErrorMessage = "Croatian PIN must be exactly 11 characters.")]
        [RegularExpression("^[0-9]{11}$", ErrorMessage = "Croatian PIN must be a 11-digit number.")]
        public string? CroatianPIN { get; set; }
        [Required(ErrorMessage = "Partner Type is required.")]
        [Range(1, 2, ErrorMessage = "Partner Type can only be 1 or 2.")]
        public int PartnerTypeId { get; set; }
        [Required]
        public DateTime CreatedAtUtc { get; set; }
        [Required]
        public string CreateByUser { get; set; }
        [Required]
        public bool IsForeign { get; set; }
        [Required(ErrorMessage = "External Code is required.")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "External Code must be between 10 and 20 characters.")]
        public string ExternalCode { get; set; }
        [Required]
        public string Gender { get; set; }

        public ICollection<PartnersPolicies> PartnersPolicies { get; set; } = new List<PartnersPolicies>();

    }
}
