using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using PlasticQC.Models;
using PlasticQC.Services;
using PlasticQC.ViewModels;

namespace PlasticQC.Views
{
    public partial class DailyEntryPage : ContentPage
    {
        private readonly DailyEntryViewModel _viewModel;
        private readonly DatabaseService _databaseService;
        private readonly AuthService _authService;

        public DailyEntryPage(DailyEntryViewModel viewModel, DatabaseService databaseService, AuthService authService)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _databaseService = databaseService;
            _authService = authService;
            BindingContext = viewModel;
            Debug.WriteLine("DailyEntryPage initialized");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("DailyEntryPage appeared");
            await _viewModel.LoadDataAsync();
        }

        private void ProductPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("Product picker selection changed");
            if (sender is Picker picker && _viewModel.SelectedProduct != null)
            {
                Debug.WriteLine($"Selected product: {_viewModel.SelectedProduct.Name}, ID: {_viewModel.SelectedProduct.Id}");
                _viewModel.ProductSelectedCommand.Execute(null);
            }
        }

        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry && entry.BindingContext is MeasurementRowItem row)
            {
                Debug.WriteLine($"Entry changed for item {row.ItemNumber}");
                _viewModel.UpdateSpecStatusCommand.Execute(row);
            }
        }
        private async void DirectSaveButton_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Direct save button clicked");

            try
            {
                // Manual save implementation
                var measurements = new List<MeasurementEntry>();
                bool dataComplete = true;

                foreach (var row in _viewModel.MeasurementRows)
                {
                    if (row.Weight <= 0 || row.RimThickness <= 0 || row.Load <= 0)
                    {
                        dataComplete = false;
                        break;
                    }

                    measurements.Add(new MeasurementEntry
                    {
                        ItemNumber = row.ItemNumber,
                        VisualLookOk = row.VisualLookOk,
                        Weight = row.Weight,
                        WeightInSpec = row.WeightInSpec,
                        HeightOk = row.HeightOk,
                        RimThickness = row.RimThickness,
                        RimThicknessInSpec = row.RimThicknessInSpec,
                        Load = row.Load,
                        LoadInSpec = row.LoadInSpec
                    });
                }

                if (!dataComplete)
                {
                    await DisplayAlert("Error", "Please complete all measurements", "OK");
                    return;
                }

                // Get the auth service and set the user ID
                var authService = Handler.MauiContext.Services.GetService<AuthService>();
                if (authService?.CurrentUser != null)
                {
                    _viewModel.CurrentRecord.CreatedById = authService.CurrentUser.Id;
                }
                else
                {
                    _viewModel.CurrentRecord.CreatedById = 1; // Default to first user if not logged in
                }

                // Get the database service and save directly
                var dbService = Handler.MauiContext.Services.GetService<DatabaseService>();
                var result = await dbService.SaveProductionRecordAsync(_viewModel.CurrentRecord, measurements);

                if (result > 0)
                {
                    await DisplayAlert("Success", "Record saved successfully", "OK");

                    // Reset UI state
                    _viewModel.IsEnteringData = false;
                    _viewModel.MeasurementRows.Clear();
                    _viewModel.CurrentRecord = null;
                }
                else
                {
                    await DisplayAlert("Error", "Failed to save record", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Save failed: {ex.Message}", "OK");
                Debug.WriteLine($"Save error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Save button clicked - direct implementation");

            if (_viewModel.CurrentRecord == null || !_viewModel.IsRecordComplete)
            {
                await DisplayAlert("Error", "Please complete all measurements", "OK");
                return;
            }

            try
            {
                // Manually perform the save operation
                var measurements = new List<MeasurementEntry>();

                foreach (var row in _viewModel.MeasurementRows)
                {
                    measurements.Add(new MeasurementEntry
                    {
                        ItemNumber = row.ItemNumber,
                        VisualLookOk = row.VisualLookOk,
                        Weight = row.Weight,
                        WeightInSpec = row.WeightInSpec,
                        HeightOk = row.HeightOk,
                        RimThickness = row.RimThickness,
                        RimThicknessInSpec = row.RimThicknessInSpec,
                        Load = row.Load,
                        LoadInSpec = row.LoadInSpec
                    });
                }

                var currentUser = _authService.CurrentUser;
                if (currentUser != null)
                {
                    _viewModel.CurrentRecord.CreatedById = currentUser.Id;
                }

                var recordId = await _databaseService.SaveProductionRecordAsync(_viewModel.CurrentRecord, measurements);

                // Show success message and reset
                await DisplayAlert("Success", "Record saved successfully", "OK");
                _viewModel.IsEnteringData = false;
                _viewModel.MeasurementRows.Clear();
                _viewModel.CurrentRecord = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in direct save: {ex.Message}");
                await DisplayAlert("Error", $"Failed to save: {ex.Message}", "OK");
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Logout button clicked");
            try
            {
                _authService.Logout();
                await Shell.Current.GoToAsync("//Login");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Logout error: {ex.Message}");
                await DisplayAlert("Error", $"Could not logout: {ex.Message}", "OK");
            }
        }
    }
}