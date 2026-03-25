namespace SmartInsuranceHub.Models
{
    public class CompanyViewModel
    {
        public int CompanyId { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Icon { get; set; } = "bi-building"; // Placeholder for icon
        public int TotalPolicies { get; set; }
    }

    public class HomeViewModel
    {
        public int TotalCompanies { get; set; }
        public int TotalCustomers { get; set; }
        public int YearsExperience { get; set; }
        public List<CompanyViewModel> Companies { get; set; } = new List<CompanyViewModel>();
        public List<InsuranceType> InsuranceTypes { get; set; } = new List<InsuranceType>();
        public List<Advertisement> Advertisements { get; set; } = new List<Advertisement>();
    }
}
