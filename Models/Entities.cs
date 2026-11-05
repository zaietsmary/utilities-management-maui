using System.ComponentModel.DataAnnotations;

namespace Lab7.Models
{
    public class Tenant
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = default!;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = default!;

        [StringLength(100)]
        public string MiddleName { get; set; } = string.Empty;

        public List<Account> Accounts { get; set; } = new();

        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
    }

    public class Service
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = default!;

        public decimal PricePerSqm { get; set; }

        public decimal PricePerPerson { get; set; }

        public List<AccountService> AccountServices { get; set; } = new();
    }

    public class City
    {
        public int Id { get; set; }

        public string Name { get; set; } = default!;

        public List<Account> Accounts { get; set; } = new();
    }

    public class Account
    {
        [Key]
        public int AccountNumber { get; set; }

        public double Area { get; set; }

        public int Residents { get; set; }

        public int CityId { get; set; }

        public City? City { get; set; }

        public int TenantId { get; set; }

        public Tenant? Tenant { get; set; }

        public string Street { get; set; } = default!;

        public string House { get; set; } = default!;

        public string Apt { get; set; } = default!;

        public List<AccountService> AccountServices { get; set; } = new();
    }

    public class AccountService
    {
        public int AccountNumber { get; set; }

        public Account Account { get; set; } = default!;

        public int ServiceId { get; set; }
        
        public Service Service { get; set; } = default!;
    }

    public class BillingReportViewModel
    {
        public int AccountNumber { get; set; }
        public string Owner { get; set; } = "";
        public string Address { get; set; } = "";
        public string ServiceName { get; set; } = "";
        public decimal TotalAmount { get; set; }
    }
}
