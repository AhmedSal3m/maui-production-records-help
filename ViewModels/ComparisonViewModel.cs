using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlasticQC.Models;
using PlasticQC.Services;
using System.Threading.Tasks;
using System.Linq;

namespace PlasticQC.ViewModels
{
    public partial class ComparisonViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly ProductService _productService;
        private readonly RecordService _recordService;

        [ObservableProperty]
        private ObservableCollection<Product> _products;

        [ObservableProperty]
        private Product _selectedProduct;

        [ObservableProperty]
        private ObservableCollection<ProductStandard> _productStandards;

        [ObservableProperty]
        private ProductStandard _selectedStandard;

        [ObservableProperty]
        private DateTime _startDate = DateTime.Now.AddDays(-30);

        [ObservableProperty]
        private DateTime _endDate = DateTime.Now;

        [ObservableProperty]
        private ObservableCollection<ComparisonData> _comparisonData;

        [ObservableProperty]
        private ObservableCollection<ProductionRecordSummary> _recordSummaries;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private bool _hasComparisonData;

        public ComparisonViewModel(DatabaseService databaseService, ProductService productService, RecordService recordService)
        {
            _databaseService = databaseService;
            _productService = productService;
            _recordService = recordService;
            Title = "Quality Comparison";
            Products = new ObservableCollection<Product>();
            ProductStandards = new ObservableCollection<ProductStandard>();
            ComparisonData = new ObservableCollection<ComparisonData>();
            RecordSummaries = new ObservableCollection<ProductionRecordSummary>();
        }

        public override async Task LoadDataAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var products = await _productService.GetProductsAsync();
                Products.Clear();

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
        async Task ProductSelectedAsync()
        {
            if (SelectedProduct == null)
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;
            SelectedStandard = null;

            try
            {
                var standards = await _productService.GetProductStandardsAsync(SelectedProduct.Id);
                ProductStandards.Clear();

                foreach (var standard in standards)
                {
                    ProductStandards.Add(standard);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading standards: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task GenerateComparisonAsync()
        {
            if (SelectedProduct == null)
            {
                ErrorMessage = "Please select a product";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;
            ComparisonData.Clear();
            RecordSummaries.Clear();

            try
            {
                // Get all records for this product within the date range
                var allRecords = await _recordService.GetRecordsByProductAsync(SelectedProduct.Id);
                var filteredRecords = allRecords.Where(r => r.RecordDate.Date >= StartDate.Date &&
                                                         r.RecordDate.Date <= EndDate.Date).ToList();

                if (filteredRecords.Count == 0)
                {
                    ErrorMessage = "No records found for the selected criteria";
                    HasComparisonData = false;
                    IsBusy = false;
                    return;
                }

                // If a specific standard is selected, filter for that standard only
                if (SelectedStandard != null)
                {
                    filteredRecords = filteredRecords.Where(r => r.StandardId == SelectedStandard.Id).ToList();
                }

                var productName = SelectedProduct.Name;
                var standardName = SelectedStandard?.MachineNumber ?? "All Machines";

                // Generate summary for each record
                foreach (var record in filteredRecords)
                {
                    var summary = await _recordService.GetRecordSummaryAsync(record.Id);
                    if (summary != null)
                    {
                        RecordSummaries.Add(summary);
                    }
                }

                // Generate comparison data
                var totalMeasurements = RecordSummaries.Sum(s => s.TotalMeasurements);
                var totalRecords = RecordSummaries.Count;

                // Calculate percentages
                var weightIssuePercent = CalculatePercentage(RecordSummaries.Sum(s => s.WeightOutOfSpec), totalMeasurements);
                var heightIssuePercent = CalculatePercentage(RecordSummaries.Sum(s => s.HeightOutOfSpec), totalMeasurements);
                var rimIssuePercent = CalculatePercentage(RecordSummaries.Sum(s => s.RimThicknessOutOfSpec), totalMeasurements);
                var loadIssuePercent = CalculatePercentage(RecordSummaries.Sum(s => s.LoadOutOfSpec), totalMeasurements);
                var visualIssuePercent = CalculatePercentage(RecordSummaries.Sum(s => s.VisualIssues), totalMeasurements);

                // Add comparison data
                ComparisonData.Add(new ComparisonData
                {
                    ProductName = productName,
                    StandardName = standardName,
                    DateRange = $"{StartDate:d MMM yyyy} - {EndDate:d MMM yyyy}",
                    TotalRecords = totalRecords,
                    TotalMeasurements = totalMeasurements,

                    WeightIssueCount = RecordSummaries.Sum(s => s.WeightOutOfSpec),
                    WeightIssuePercent = weightIssuePercent,

                    HeightIssueCount = RecordSummaries.Sum(s => s.HeightOutOfSpec),
                    HeightIssuePercent = heightIssuePercent,

                    RimIssueCount = RecordSummaries.Sum(s => s.RimThicknessOutOfSpec),
                    RimIssuePercent = rimIssuePercent,

                    LoadIssueCount = RecordSummaries.Sum(s => s.LoadOutOfSpec),
                    LoadIssuePercent = loadIssuePercent,

                    VisualIssueCount = RecordSummaries.Sum(s => s.VisualIssues),
                    VisualIssuePercent = visualIssuePercent
                });

                HasComparisonData = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error generating comparison: {ex.Message}";
                HasComparisonData = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private double CalculatePercentage(int count, int total)
        {
            if (total == 0)
                return 0;

            return Math.Round((double)count / total * 100, 2);
        }
    }

    public class ComparisonData
    {
        public string ProductName { get; set; }
        public string StandardName { get; set; }
        public string DateRange { get; set; }
        public int TotalRecords { get; set; }
        public int TotalMeasurements { get; set; }

        public int WeightIssueCount { get; set; }
        public double WeightIssuePercent { get; set; }

        public int HeightIssueCount { get; set; }
        public double HeightIssuePercent { get; set; }

        public int RimIssueCount { get; set; }
        public double RimIssuePercent { get; set; }

        public int LoadIssueCount { get; set; }
        public double LoadIssuePercent { get; set; }

        public int VisualIssueCount { get; set; }
        public double VisualIssuePercent { get; set; }
    }
}