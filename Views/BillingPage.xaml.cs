using Lab7.Data;
using Lab7.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab7.Views;

public partial class BillingPage : ContentPage
{
    private readonly AppDbContext _db;

    public BillingPage(AppDbContext db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadBillingReportAsync();
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await LoadBillingReportAsync();
    }

    private async Task LoadBillingReportAsync()
    {
        try
        {
            var data = await _db.AccountServices
                .Include(acs => acs.Account)
                    .ThenInclude(a => a.Tenant)
                .Include(acs => acs.Account)
                    .ThenInclude(a => a.City)
                .Include(acs => acs.Service)
                .AsNoTracking()
                .ToListAsync();

            var reportItems = data.Select(acs =>
            {
                var acc = acs.Account;
                var s = acs.Service;
                var t = acc.Tenant;
                var c = acc.City;

                string owner = t != null 
                    ? $"{t.LastName} {t.FirstName.FirstOrDefault()}. {t.MiddleName.FirstOrDefault()}.".Trim() 
                    : "Не вказано";

                string cityName = c?.Name ?? "Не вказано";
                string address = $"{cityName}, вул. {acc.Street} {acc.House}, кв. {acc.Apt}";

                decimal costFromArea = (decimal)acc.Area * s.PricePerSqm;
                decimal costFromResidents = acc.Residents * s.PricePerPerson;
                decimal total = costFromArea + costFromResidents;

                return new BillingReportViewModel
                {
                    AccountNumber = acc.AccountNumber,
                    Owner = owner,
                    Address = address,
                    ServiceName = s.Name,
                    TotalAmount = total
                };
            }).OrderBy(x => x.AccountNumber).ToList();

            BillingCollectionView.ItemsSource = reportItems;

            decimal totalOverall = reportItems.Sum(x => x.TotalAmount);
            TotalSumLabel.Text = $"{totalOverall:F2} ₴";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", ex.Message, "OK");
        }
    }
}

