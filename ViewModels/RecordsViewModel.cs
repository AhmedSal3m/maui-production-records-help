using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlasticQC.Models;
using PlasticQC.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using System.Diagnostics.Metrics;

namespace PlasticQC.ViewModels
{
    public partial class RecordsViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly AuthService _authService;
        private readonly ExportService _exportService;

        [ObservableProperty]
        private ObservableCollection<Product> _products;

        [ObservableProperty]
        private Product _selectedProduct;

        [ObservableProperty]
        private ObservableCollection<RecordListItem> _records;

        [ObservableProperty]
        private RecordListItem _selectedRecord;

        [ObservableProperty]
        private ObservableCollection<MeasurementDisplayItem> _measurements;

        [ObservableProperty]
        private bool _isViewingDetails;

        [ObservableProperty]
        private DateTime _startDate = DateTime.Now.AddDays(-7);

        [ObservableProperty]
        private DateTime _endDate = DateTime.Now;

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private string _successMessage;

        public RecordsViewModel(DatabaseService databaseService, AuthService authService, ExportService exportService)
        {
            _databaseService = databaseService;
            _authService = authService;
            _exportService = exportService;
            Title = "Production Records";
            Products = new ObservableCollection<Product>();
            Records = new ObservableCollection<RecordListItem>();
            Measurements = new ObservableCollection<MeasurementDisplayItem>();
        }

        public override async Task LoadDataAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            try
            {
                // Load products
                var products = await _databaseService.GetProductsAsync();
                Products.Clear();
                Products.Add(new Product { Id = 0, Name = "All Products" });

                foreach (var product in products)
                {
                    Products.Add(product);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading products: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task SearchRecordsAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            Records.Clear();

            try
            {
                var records = await _databaseService.GetProductionRecordsAsync();

                // Filter by date
                records = records.Where(r => r.RecordDate.Date >= StartDate.Date &&
                                            r.RecordDate.Date <= EndDate.Date).ToList();

                // Filter by product if selected
                if (SelectedProduct != null && SelectedProduct.Id > 0)
                {
                    records = records.Where(r => r.ProductId == SelectedProduct.Id).ToList();
                }

                // Get additional data for each record
                foreach (var record in records)
                {
                    var product = await _databaseService.GetProductAsync(record.ProductId);
                    var user = await _databaseService.GetUserAsync(record.CreatedById);
                    var standard = await _databaseService.GetProductStandardAsync(record.StandardId);

                    Records.Add(new RecordListItem
                    {
                        Id = record.Id,
                        ProductId = record.ProductId,
                        ProductName = product.Name,
                        ProductNumber = product.ProductNumber,
                        MachineNumber = record.MachineNumber,
                        RecordDate = record.RecordDate,
                        CreatedByName = user?.FullName ?? "Unknown",
                        QuantityMeasured = record.QuantityMeasured,
                        StandardId = record.StandardId
                    });
                }

                // Apply text search if provided
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var searchLower = SearchText.ToLowerInvariant();

                    // Create a temporary list to avoid modifying the collection during iteration
                    var filteredRecords = Records.Where(r =>
                        r.ProductName.ToLowerInvariant().Contains(searchLower) ||
                        r.ProductNumber.ToLowerInvariant().Contains(searchLower) ||
                        r.MachineNumber.ToLowerInvariant().Contains(searchLower) ||
                        r.CreatedByName.ToLowerInvariant().Contains(searchLower)
                    ).ToList();

                    Records.Clear();
                    foreach (var record in filteredRecords)
                    {
                        Records.Add(record);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error searching records: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task ViewRecordDetailsAsync(RecordListItem record)
        {
            if (record == null)
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;
            SelectedRecord = record;

            try
            {
                // Load the record details
                var measurements = await _databaseService.GetMeasurementEntriesAsync(record.Id);

                // Load the standard to check against
                var standard = await _databaseService.GetProductStandardAsync(record.StandardId);

                Measurements.Clear();
                foreach (var measurement in measurements)
                {
                    Measurements.Add(new MeasurementDisplayItem
                    {
                        ItemNumber = measurement.ItemNumber,
                        VisualLookOk = measurement.VisualLookOk,
                        Weight = measurement.Weight,
                        WeightInSpec = measurement.WeightInSpec,
                        HeightOk = measurement.HeightOk,
                        RimThickness = measurement.RimThickness,
                        RimThicknessInSpec = measurement.RimThicknessInSpec,
                        Load = measurement.Load,
                        LoadInSpec = measurement.LoadInSpec,

                        StandardWeight = standard.StandardWeight,
                        WeightTolerancePlus = standard.WeightTolerancePlus,
                        WeightToleranceMinus = standard.WeightToleranceMinus,

                        StandardRimThickness = standard.StandardRimThickness,
                        RimThicknessTolerancePlus = standard.RimThicknessTolerancePlus,
                        RimThicknessToleranceMinus = standard.RimThicknessToleranceMinus,

                        StandardLoad = standard.StandardLoad,
                        LoadTolerancePlus = standard.LoadTolerancePlus,
                        LoadToleranceMinus = standard.LoadToleranceMinus
                    });
                }

                IsViewingDetails = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading record details: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        void BackToRecords()
        {
            IsViewingDetails = false;
            Measurements.Clear();
            SelectedRecord = null;
        }

        [RelayCommand]
        async Task ExportToPdfAsync()
        {
            if (SelectedRecord == null || !IsViewingDetails)
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var result = await _exportService.ExportRecordToPdfAsync(SelectedRecord, Measurements.ToList());

                if (result)
                {
                    SuccessMessage = "Record exported to PDF successfully";
                }
                else
                {
                    ErrorMessage = "Failed to export record to PDF";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error exporting to PDF: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task ExportToExcelAsync()
        {
            if (SelectedRecord == null || !IsViewingDetails)
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var result = await _exportService.ExportToExcelAsync(SelectedRecord, Measurements.ToList());

                if (result)
                {
                    SuccessMessage = "Record exported to Excel successfully";
                }
                else
                {
                    ErrorMessage = "Failed to export record to Excel";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error exporting to Excel: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    // Helper class for record list display
    public class RecordListItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductNumber { get; set; }
        public string MachineNumber { get; set; }
        public DateTime RecordDate { get; set; }
        public string CreatedByName { get; set; }
        public int QuantityMeasured { get; set; }
        public int StandardId { get; set; }
    }

    // Helper class for measurement display with standards
    public class MeasurementDisplayItem
    {
        public int ItemNumber { get; set; }
        public bool VisualLookOk { get; set; }

        public double Weight { get; set; }
        public bool WeightInSpec { get; set; }
        public double StandardWeight { get; set; }
        public double WeightTolerancePlus { get; set; }
        public double WeightToleranceMinus { get; set; }

        public bool HeightOk { get; set; }

        public double RimThickness { get; set; }
        public bool RimThicknessInSpec { get; set; }
        public double StandardRimThickness { get; set; }
        public double RimThicknessTolerancePlus { get; set; }
        public double RimThicknessToleranceMinus { get; set; }

        public double Load { get; set; }
        public bool LoadInSpec { get; set; }
        public double StandardLoad { get; set; }
        public double LoadTolerancePlus { get; set; }
        public double LoadToleranceMinus { get; set; }
    }
}