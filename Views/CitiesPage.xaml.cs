using Lab7.Data;
using Lab7.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab7.Views;

public partial class CitiesPage : ContentPage
{
    private readonly AppDbContext _db;
    private City? _editingCity = null;

    public CitiesPage(AppDbContext db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCitiesAsync();
    }

    private async Task LoadCitiesAsync()
    {
        try
        {
            var cities = await _db.Cities.AsNoTracking().ToListAsync();
            CitiesCollectionView.ItemsSource = cities;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", ex.Message, "OK");
        }
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlert("Увага", "Введіть назву міста", "OK");
            return;
        }

        var city = new City
        {
            Name = NameEntry.Text.Trim()
        };

        _db.Cities.Add(city);
        await _db.SaveChangesAsync();

        ClearForm();
        await LoadCitiesAsync();
    }

    private void OnEditItemClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is City city)
        {
            _editingCity = city;
            NameEntry.Text = city.Name;

            FormTitleLabel.Text = $"Редагування міста: {city.Name}";
            FormTitleLabel.TextColor = Color.FromArgb("#F59E0B");

            AddBtn.IsVisible = false;
            SaveBtn.IsVisible = true;
            CancelBtn.IsVisible = true;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_editingCity == null) return;

        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlert("Увага", "Введіть назву міста", "OK");
            return;
        }

        var dbCity = await _db.Cities.FindAsync(_editingCity.Id);
        if (dbCity != null)
        {
            dbCity.Name = NameEntry.Text.Trim();
            await _db.SaveChangesAsync();
        }

        ClearForm();
        await LoadCitiesAsync();
    }

    private async void OnDeleteItemClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is City city)
        {
            bool confirm = await DisplayAlert("Видалення", $"Видалити місто \"{city.Name}\"?", "Так, видалити", "Скасувати");
            if (confirm)
            {
                var dbCity = await _db.Cities.FindAsync(city.Id);
                if (dbCity != null)
                {
                    _db.Cities.Remove(dbCity);
                    await _db.SaveChangesAsync();
                    await LoadCitiesAsync();
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
        _editingCity = null;
        NameEntry.Text = string.Empty;

        FormTitleLabel.Text = "Додати нове місто";
        FormTitleLabel.TextColor = Color.FromArgb("#38BDF8");

        AddBtn.IsVisible = true;
        SaveBtn.IsVisible = false;
        CancelBtn.IsVisible = false;
    }
}

