using Lab7.Data;
using Lab7.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab7.Views;

public partial class TenantsPage : ContentPage
{
    private readonly AppDbContext _db;
    private Tenant? _editingTenant = null;

    public TenantsPage(AppDbContext db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadTenantsAsync();
    }

    private async Task LoadTenantsAsync()
    {
        try
        {
            var tenants = await _db.Tenants.AsNoTracking().ToListAsync();
            TenantsCollectionView.ItemsSource = tenants;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", ex.Message, "OK");
        }
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(LastNameEntry.Text) || string.IsNullOrWhiteSpace(FirstNameEntry.Text))
        {
            await DisplayAlert("Увага", "Вкажіть прізвище та ім'я мешканця", "OK");
            return;
        }

        var tenant = new Tenant
        {
            LastName = LastNameEntry.Text.Trim(),
            FirstName = FirstNameEntry.Text.Trim(),
            MiddleName = MiddleNameEntry.Text?.Trim() ?? string.Empty
        };

        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync();

        ClearForm();
        await LoadTenantsAsync();
    }

    private void OnEditItemClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Tenant tenant)
        {
            _editingTenant = tenant;
            LastNameEntry.Text = tenant.LastName;
            FirstNameEntry.Text = tenant.FirstName;
            MiddleNameEntry.Text = tenant.MiddleName;

            FormTitleLabel.Text = $"Редагування мешканця: {tenant.FullName}";
            FormTitleLabel.TextColor = Color.FromArgb("#F59E0B");

            AddBtn.IsVisible = false;
            SaveBtn.IsVisible = true;
            CancelBtn.IsVisible = true;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_editingTenant == null) return;

        if (string.IsNullOrWhiteSpace(LastNameEntry.Text) || string.IsNullOrWhiteSpace(FirstNameEntry.Text))
        {
            await DisplayAlert("Увага", "Вкажіть прізвище та ім'я", "OK");
            return;
        }

        var dbTenant = await _db.Tenants.FindAsync(_editingTenant.Id);
        if (dbTenant != null)
        {
            dbTenant.LastName = LastNameEntry.Text.Trim();
            dbTenant.FirstName = FirstNameEntry.Text.Trim();
            dbTenant.MiddleName = MiddleNameEntry.Text?.Trim() ?? string.Empty;

            await _db.SaveChangesAsync();
        }

        ClearForm();
        await LoadTenantsAsync();
    }

    private async void OnDeleteItemClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Tenant tenant)
        {
            bool confirm = await DisplayAlert("Видалення", $"Видалити мешканця \"{tenant.FullName}\"?", "Так, видалити", "Скасувати");
            if (confirm)
            {
                var dbTenant = await _db.Tenants.FindAsync(tenant.Id);
                if (dbTenant != null)
                {
                    _db.Tenants.Remove(dbTenant);
                    await _db.SaveChangesAsync();
                    await LoadTenantsAsync();
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
        _editingTenant = null;
        LastNameEntry.Text = string.Empty;
        FirstNameEntry.Text = string.Empty;
        MiddleNameEntry.Text = string.Empty;

        FormTitleLabel.Text = "Додати нового мешканця";
        FormTitleLabel.TextColor = Color.FromArgb("#38BDF8");

        AddBtn.IsVisible = true;
        SaveBtn.IsVisible = false;
        CancelBtn.IsVisible = false;
    }
}

