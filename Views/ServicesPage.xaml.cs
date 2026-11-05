using Lab7.Data;
using Lab7.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab7.Views;

public partial class ServicesPage : ContentPage
{
    private readonly AppDbContext _db;
    private Service? _editingService = null;

    public ServicesPage(AppDbContext db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadServicesAsync();
    }

    private async Task LoadServicesAsync()
    {
        try
        {
            var services = await _db.Services.AsNoTracking().ToListAsync();
            ServicesCollectionView.ItemsSource = services;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка бази", ex.Message, "OK");
        }
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlert("Увага", "Введіть назву послуги", "OK");
            return;
        }

        decimal.TryParse(PriceSqmEntry.Text, out var priceSqm);
        decimal.TryParse(PricePersonEntry.Text, out var pricePerson);

        var service = new Service
        {
            Name = NameEntry.Text.Trim(),
            PricePerSqm = priceSqm,
            PricePerPerson = pricePerson
        };

        _db.Services.Add(service);
        await _db.SaveChangesAsync();

        ClearForm();
        await LoadServicesAsync();
    }

    private void OnEditItemClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Service service)
        {
            _editingService = service;
            NameEntry.Text = service.Name;
            PriceSqmEntry.Text = service.PricePerSqm.ToString("0.00");
            PricePersonEntry.Text = service.PricePerPerson.ToString("0.00");

            FormTitleLabel.Text = $"Редагування послуги: {service.Name}";
            FormTitleLabel.TextColor = Color.FromArgb("#F59E0B");

            AddBtn.IsVisible = false;
            SaveBtn.IsVisible = true;
            CancelBtn.IsVisible = true;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_editingService == null) return;

        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlert("Увага", "Введіть назву послуги", "OK");
            return;
        }

        var dbService = await _db.Services.FindAsync(_editingService.Id);
        if (dbService != null)
        {
            decimal.TryParse(PriceSqmEntry.Text, out var priceSqm);
            decimal.TryParse(PricePersonEntry.Text, out var pricePerson);

            dbService.Name = NameEntry.Text.Trim();
            dbService.PricePerSqm = priceSqm;
            dbService.PricePerPerson = pricePerson;

            await _db.SaveChangesAsync();
        }

        ClearForm();
        await LoadServicesAsync();
    }

    private async void OnDeleteItemClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Service service)
        {
            bool confirm = await DisplayAlert("Видалення", $"Видалити послугу \"{service.Name}\"?", "Так, видалити", "Скасувати");
            if (confirm)
            {
                var dbService = await _db.Services.FindAsync(service.Id);
                if (dbService != null)
                {
                    _db.Services.Remove(dbService);
                    await _db.SaveChangesAsync();
                    await LoadServicesAsync();
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
        _editingService = null;
        NameEntry.Text = string.Empty;
        PriceSqmEntry.Text = string.Empty;
        PricePersonEntry.Text = string.Empty;

        FormTitleLabel.Text = "Додати нову послугу";
        FormTitleLabel.TextColor = Color.FromArgb("#38BDF8");

        AddBtn.IsVisible = true;
        SaveBtn.IsVisible = false;
        CancelBtn.IsVisible = false;
    }
}