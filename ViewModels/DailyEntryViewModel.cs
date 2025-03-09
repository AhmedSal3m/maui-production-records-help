using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlasticQC.Models;
using PlasticQC.Services;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace PlasticQC.ViewModels
{
    public partial class DailyEntryViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly AuthService _authService;

        [ObservableProperty]
        private ObservableCollection<Product> _products;

        [ObservableProperty]
        private Product _selectedProduct;

        [ObservableProperty]
        private ObservableCollection<ProductStandard> _productStandards;

        [ObservableProperty]
        private ProductStandard _selectedStandard;

        [ObservableProperty]
        private ProductionRecord _currentRecord;

        [ObservableProperty]
        private ObservableCollection<MeasurementRowItem> _measurementRows;

        [ObservableProperty]
        private bool _isEnteringData;

        [ObservableProperty]
        private bool _isRecordComplete;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private string _successMessage;

        public DailyEntryViewModel(DatabaseService databaseService, AuthService authService)
        {
            _databaseService = databaseService;
            _authService = authService;
            Title = "Daily Entry";
            Products = new ObservableCollection<Product>();
            ProductStandards = new ObservableCollection<ProductStandard>();
            MeasurementRows = new ObservableCollection<MeasurementRowItem>();

            Debug.WriteLine($"DailyEntryViewModel initialized. Auth active: {_authService?.IsLoggedIn}");
            Debug.WriteLine($"Current user: {_authService?.CurrentUser?.Username ?? "None"}");
        }

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            try
            {
                Debug.WriteLine("Loading products for daily entry");
                var products = await _databaseService.GetProductsAsync();
                Products.Clear();

                foreach (var product in products)
                {
                    Products.Add(product);
                }
                Debug.WriteLine($"Loaded {Products.Count} products");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading products: {ex.Message}");
                ErrorMessage = $"Error loading products: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task ProductSelectedAsync()
        {
            if (SelectedProduct == null)
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;
            SelectedStandard = null;
            ProductStandards.Clear();

            try
            {
                Debug.WriteLine($"Loading standards for product: {SelectedProduct.Id} - {SelectedProduct.Name}");
                var standards = await _databaseService.GetProductStandardsAsync(SelectedProduct.Id);

                if (standards == null || standards.Count == 0)
                {
                    Debug.WriteLine("No standards found for this product");
                    ErrorMessage = "No standards found for this product";
                    return;
                }

                foreach (var standard in standards)
                {
                    ProductStandards.Add(standard);
                    Debug.WriteLine($"Added standard: Machine {standard.MachineNumber}, ID: {standard.Id}");
                }
                Debug.WriteLine($"Loaded {ProductStandards.Count} standards");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading standards: {ex.Message}");
                ErrorMessage = $"Error loading standards: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        void StartEntry()
        {
            if (SelectedProduct == null || SelectedStandard == null)
            {
                ErrorMessage = "Please select a product and a standard";
                return;
            }

            Debug.WriteLine($"Starting entry for Product {SelectedProduct.Name}, Machine {SelectedStandard.MachineNumber}");
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            IsEnteringData = true;
            IsRecordComplete = false;

            // Initialize the record
            CurrentRecord = new ProductionRecord
            {
                ProductId = SelectedProduct.Id,
                StandardId = SelectedStandard.Id,
                MachineNumber = SelectedStandard.MachineNumber,
                QuantityMeasured = SelectedStandard.QuantityPerCycle,
                RecordDate = DateTime.Now,
                CreatedById = _authService.CurrentUser?.Id ?? 1 // Default to user 1 if not logged in
            };

            Debug.WriteLine($"Creating measurement rows for {SelectedStandard.QuantityPerCycle} items");
            // Create measurement rows based on the standard quantity
            MeasurementRows.Clear();
            for (int i = 1; i <= SelectedStandard.QuantityPerCycle; i++)
            {
                MeasurementRows.Add(new MeasurementRowItem
                {
                    ItemNumber = i,
                    WeightMin = SelectedStandard.StandardWeight - SelectedStandard.WeightToleranceMinus,
                    WeightMax = SelectedStandard.StandardWeight + SelectedStandard.WeightTolerancePlus,
                    RimThicknessMin = SelectedStandard.StandardRimThickness - SelectedStandard.RimThicknessToleranceMinus,
                    RimThicknessMax = SelectedStandard.StandardRimThickness + SelectedStandard.RimThicknessTolerancePlus,
                    LoadMin = SelectedStandard.StandardLoad - SelectedStandard.LoadToleranceMinus,
                    LoadMax = SelectedStandard.StandardLoad + SelectedStandard.LoadTolerancePlus
                });
            }
        }

        [RelayCommand]
        void CancelEntry()
        {
            Debug.WriteLine("Cancelling entry");
            IsEnteringData = false;
            MeasurementRows.Clear();
            CurrentRecord = null;
        }

        [RelayCommand]
        void UpdateSpecStatus(MeasurementRowItem row)
        {
            if (row == null)
                return;

            // Update in-spec flags
            row.WeightInSpec = row.Weight >= row.WeightMin && row.Weight <= row.WeightMax;
            row.RimThicknessInSpec = row.RimThickness >= row.RimThicknessMin && row.RimThickness <= row.RimThicknessMax;
            row.LoadInSpec = row.Load >= row.LoadMin && row.Load <= row.LoadMax;

            // Check if all rows have data
            IsRecordComplete = MeasurementRows.All(r => r.HasData);
            Debug.WriteLine($"Record complete: {IsRecordComplete}");
        }

        [RelayCommand]
        async Task SaveRecordAsync()
        {
            Debug.WriteLine("SaveRecordAsync called");
            if (!IsRecordComplete)
            {
                ErrorMessage = "Please complete all measurements";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                // Convert the UI model to database model
                var measurements = new List<MeasurementEntry>();
                foreach (var row in MeasurementRows)
                {
                    Debug.WriteLine($"Adding measurement for item {row.ItemNumber}: Weight={row.Weight}, RimThickness={row.RimThickness}, Load={row.Load}");
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

                Debug.WriteLine($"Current user ID: {_authService.CurrentUser?.Id ?? -1}");
                Debug.WriteLine($"CurrentRecord: ProductId={CurrentRecord.ProductId}, StandardId={CurrentRecord.StandardId}");

                // Ensure CreatedById is set correctly
                if (_authService.CurrentUser != null)
                {
                    CurrentRecord.CreatedById = _authService.CurrentUser.Id;
                }
                else
                {
                    Debug.WriteLine("WARNING: No current user found!");
                    // Default to user ID 1 if needed
                    CurrentRecord.CreatedById = 1;
                }

                // Save the record
                var recordId = await _databaseService.SaveProductionRecordAsync(CurrentRecord, measurements);
                Debug.WriteLine($"Record saved with ID: {recordId}");

                // Show success message
                SuccessMessage = "Record saved successfully";
                await Application.Current.MainPage.DisplayAlert("Success", "Record saved successfully", "OK");

                // Reset the form
                IsEnteringData = false;
                MeasurementRows.Clear();
                CurrentRecord = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving record: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                ErrorMessage = $"Error saving record: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save record: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task LogoutAsync()
        {
            Debug.WriteLine("Logout command executed in DailyEntryViewModel");
            _authService.Logout();
            await Shell.Current.GoToAsync("//Login");
        }
    }

    // UI helper class for measurement rows
    public partial class MeasurementRowItem : ObservableObject
    {
        [ObservableProperty]
        private int _itemNumber;

        [ObservableProperty]
        private bool _visualLookOk = true;

        [ObservableProperty]
        private double _weight;

        [ObservableProperty]
        private bool _weightInSpec;

        [ObservableProperty]
        private double _weightMin;

        [ObservableProperty]
        private double _weightMax;

        [ObservableProperty]
        private bool _heightOk = true;

        [ObservableProperty]
        private double _rimThickness;

        [ObservableProperty]
        private bool _rimThicknessInSpec;

        [ObservableProperty]
        private double _rimThicknessMin;

        [ObservableProperty]
        private double _rimThicknessMax;

        [ObservableProperty]
        private double _load;

        [ObservableProperty]
        private bool _loadInSpec;

        [ObservableProperty]
        private double _loadMin;

        [ObservableProperty]
        private double _loadMax;

        public bool HasData => Weight > 0 && RimThickness > 0 && Load > 0;
    }
}