namespace RokoJelinicWeinerTest.Models
{
    public class PartnersPolicies
    {
        public int PartnerId { get; set; }
        public PartnersModel Partner { get; set; }
        public string PolicyNumber { get; set; }

        public PoliciesModel Policies { get; set; }
    }
}
