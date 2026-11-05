using Lab7.Data;
using Lab7.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab7.Views;

public partial class AccountsPage : ContentPage
{
    private readonly AppDbContext _db;
    private Account? _editingAccount = null;
    private List<Tenant> _tenants = new();
    private List<City> _cities = new();

    public AccountsPage(AppDbContext db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDropdownsAsync();
        await LoadAccountsAsync();
    }

    private async Task LoadDropdownsAsync()
    {
        try
        {
            _tenants = await _db.Tenants.AsNoTracking().ToListAsync();
            TenantPicker.ItemsSource = _tenants;

            _cities = await _db.Cities.AsNoTracking().ToListAsync();
            CityPicker.ItemsSource = _cities;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка довідників", ex.Message, "OK");
        }
    }

    private async Task LoadAccountsAsync()
    {
        try
        {
            var accounts = await _db.Accounts
                .Include(a => a.Tenant)
                .Include(a => a.City)
                .AsNoTracking()
                .ToListAsync();

            AccountsCollectionView.ItemsSource = accounts;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка бази", ex.Message, "OK");
        }
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        if (!int.TryParse(AccountNumberEntry.Text, out var accNum) || accNum <= 0)
        {
            await DisplayAlert("Увага", "Вкажіть коректний номер особового рахунку", "OK");
            return;
        }

        if (TenantPicker.SelectedItem is not Tenant selectedTenant)
        {
            await DisplayAlert("Увага", "Оберіть власника рахунку", "OK");
            return;
        }

        if (CityPicker.SelectedItem is not City selectedCity)
        {
            await DisplayAlert("Увага", "Оберіть місто", "OK");
            return;
        }

        var exists = await _db.Accounts.AnyAsync(a => a.AccountNumber == accNum);
        if (exists)
        {
            await DisplayAlert("Помилка", $"Рахунок №{accNum} вже існує в системі!", "OK");
            return;
        }

        double.TryParse(AreaEntry.Text, out var area);
        int.TryParse(ResidentsEntry.Text, out var residents);

        var account = new Account
        {
            AccountNumber = accNum,
            TenantId = selectedTenant.Id,
            CityId = selectedCity.Id,
            Street = StreetEntry.Text?.Trim() ?? string.Empty,
            House = HouseEntry.Text?.Trim() ?? string.Empty,
            Apt = AptEntry.Text?.Trim() ?? string.Empty,
            Area = area,
            Residents = residents
        };

        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();

        ClearForm();
        await LoadAccountsAsync();
    }

    private void OnEditItemClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Account account)
        {
            _editingAccount = account;
            AccountNumberEntry.Text = account.AccountNumber.ToString();
            AccountNumberEntry.IsEnabled = false;

            TenantPicker.SelectedItem = _tenants.FirstOrDefault(t => t.Id == account.TenantId);
            CityPicker.SelectedItem = _cities.FirstOrDefault(c => c.Id == account.CityId);

            StreetEntry.Text = account.Street;
            HouseEntry.Text = account.House;
            AptEntry.Text = account.Apt;
            AreaEntry.Text = account.Area.ToString("0.##");
            ResidentsEntry.Text = account.Residents.ToString();

            FormTitleLabel.Text = $"Редагування рахунку №{account.AccountNumber}";
            FormTitleLabel.TextColor = Color.FromArgb("#F59E0B");

            AddBtn.IsVisible = false;
            SaveBtn.IsVisible = true;
            CancelBtn.IsVisible = true;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_editingAccount == null) return;

        if (TenantPicker.SelectedItem is not Tenant selectedTenant)
        {
            await DisplayAlert("Увага", "Оберіть власника рахунку", "OK");
            return;
        }

        if (CityPicker.SelectedItem is not City selectedCity)
        {
            await DisplayAlert("Увага", "Оберіть місто", "OK");
            return;
        }

        var dbAccount = await _db.Accounts.FindAsync(_editingAccount.AccountNumber);
        if (dbAccount != null)
        {
            double.TryParse(AreaEntry.Text, out var area);
            int.TryParse(ResidentsEntry.Text, out var residents);

            dbAccount.TenantId = selectedTenant.Id;
            dbAccount.CityId = selectedCity.Id;
            dbAccount.Street = StreetEntry.Text?.Trim() ?? string.Empty;
            dbAccount.House = HouseEntry.Text?.Trim() ?? string.Empty;
            dbAccount.Apt = AptEntry.Text?.Trim() ?? string.Empty;
            dbAccount.Area = area;
            dbAccount.Residents = residents;

            await _db.SaveChangesAsync();
        }

        ClearForm();
        await LoadAccountsAsync();
    }

    private async void OnDeleteItemClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Account account)
        {
            bool confirm = await DisplayAlert("Видалення", $"Видалити особовий рахунок №{account.AccountNumber}?", "Так, видалити", "Скасувати");
            if (confirm)
            {
                var dbAccount = await _db.Accounts
                    .Include(a => a.AccountServices)
                    .FirstOrDefaultAsync(a => a.AccountNumber == account.AccountNumber);

                if (dbAccount != null)
                {
                    if (dbAccount.AccountServices.Any())
                    {
                        _db.AccountServices.RemoveRange(dbAccount.AccountServices);
                    }

                    _db.Accounts.Remove(dbAccount);
                    await _db.SaveChangesAsync();
                    await LoadAccountsAsync();
                }
            }
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        ClearForm();
    }

    private void ClearForm()
    {
        _editingAccount = null;
        AccountNumberEntry.Text = string.Empty;
        AccountNumberEntry.IsEnabled = true;

        TenantPicker.SelectedItem = null;
        CityPicker.SelectedItem = null;

        StreetEntry.Text = string.Empty;
        HouseEntry.Text = string.Empty;
        AptEntry.Text = string.Empty;
        AreaEntry.Text = string.Empty;
        ResidentsEntry.Text = string.Empty;

        FormTitleLabel.Text = "Створити новий особовий рахунок";
        FormTitleLabel.TextColor = Color.FromArgb("#38BDF8");

        AddBtn.IsVisible = true;
        SaveBtn.IsVisible = false;
        CancelBtn.IsVisible = false;
    }
}
