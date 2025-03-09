using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlasticQC.Models;
using PlasticQC.Services;
using System.Threading.Tasks;

namespace PlasticQC.ViewModels
{
    public partial class ProductManagementViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly AuthService _authService;

        [ObservableProperty]
        private ObservableCollection<Product> _products;

        [ObservableProperty]
        private Product _selectedProduct;

        [ObservableProperty]
        private ProductStandard _selectedStandard;

        [ObservableProperty]
        private ObservableCollection<ProductStandard> _productStandards;

        [ObservableProperty]
        private bool _isNewProduct;

        [ObservableProperty]
        private bool _isNewStandard;

        [ObservableProperty]
        private bool _isEditingProduct;

        [ObservableProperty]
        private bool _isEditingStandard;

        [ObservableProperty]
        private bool _isViewingStandards;

        // Product properties
        [ObservableProperty]
        private string _productName;

        [ObservableProperty]
        private string _productNumber;

        // Standard properties
        [ObservableProperty]
        private string _machineNumber;

        [ObservableProperty]
        private int _quantityPerCycle;

        [ObservableProperty]
        private double _standardWeight;

        [ObservableProperty]
        private double _weightTolerancePlus;

        [ObservableProperty]
        private double _weightToleranceMinus;

        [ObservableProperty]
        private double _standardHeight;

        [ObservableProperty]
        private double _heightTolerancePlus;

        [ObservableProperty]
        private double _heightToleranceMinus;

        [ObservableProperty]
        private double _standardRimThickness;

        [ObservableProperty]
        private double _rimThicknessTolerancePlus;

        [ObservableProperty]
        private double _rimThicknessToleranceMinus;

        [ObservableProperty]
        private double _standardLoad;

        [ObservableProperty]
        private double _loadTolerancePlus;

        [ObservableProperty]
        private double _loadToleranceMinus;

        [ObservableProperty]
        private string _errorMessage;

        public ProductManagementViewModel(DatabaseService databaseService, AuthService authService)
        {
            _databaseService = databaseService;
            _authService = authService;
            Title = "Product Management";
            Products = new ObservableCollection<Product>();
            ProductStandards = new ObservableCollection<ProductStandard>();
        }

        public override async Task LoadDataAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var products = await _databaseService.GetProductsAsync();
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
        async Task LoadStandardsAsync(Product product)
        {
            if (product == null)
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;
            IsViewingStandards = true;
            SelectedProduct = product;

            try
            {
                var standards = await _databaseService.GetProductStandardsAsync(product.Id);
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
        void AddNewProduct()
        {
            SelectedProduct = null;
            IsNewProduct = true;
            IsEditingProduct = true;

            ProductName = string.Empty;
            ProductNumber = string.Empty;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        void EditProduct(Product product)
        {
            if (product == null)
                return;

            SelectedProduct = product;
            IsNewProduct = false;
            IsEditingProduct = true;

            ProductName = product.Name;
            ProductNumber = product.ProductNumber;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        async Task SaveProductAsync()
        {
            if (string.IsNullOrWhiteSpace(ProductName) || string.IsNullOrWhiteSpace(ProductNumber))
            {
                ErrorMessage = "Please fill in all required fields";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                // Check for duplicate product number
                var products = await _databaseService.GetProductsAsync();
                if (products.Any(p => p.ProductNumber == ProductNumber &&
                                     (IsNewProduct || p.Id != SelectedProduct.Id)))
                {
                    ErrorMessage = "Product number already exists";
                    IsBusy = false;
                    return;
                }

                Product product;

                if (IsNewProduct)
                {
                    product = new Product
                    {
                        Name = ProductName,
                        ProductNumber = ProductNumber,
                        CreatedAt = DateTime.Now,
                        CreatedById = _authService.CurrentUser.Id
                    };
                }
                else
                {
                    product = SelectedProduct;
                    product.Name = ProductName;
                    product.ProductNumber = ProductNumber;
                }

                await _databaseService.SaveProductAsync(product);

                // Refresh the list
                await LoadDataAsync();

                IsEditingProduct = false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving product: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        void CancelProductEdit()
        {
            IsEditingProduct = false;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        async Task DeleteProductAsync(Product product)
        {
            if (product == null)
                return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                $"Are you sure you want to delete {product.Name}? This will also delete all standards and records associated with this product.",
                "Yes", "No");

            if (!confirm)
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                // Delete all standards for this product first
                var standards = await _databaseService.GetProductStandardsAsync(product.Id);
                foreach (var standard in standards)
                {
                    await _databaseService.DeleteProductStandardAsync(standard);
                }

                // Then delete the product
                await _databaseService.DeleteProductAsync(product);
                Products.Remove(product);

                // If we're viewing standards for this product, go back to product list
                if (IsViewingStandards && SelectedProduct?.Id == product.Id)
                {
                    IsViewingStandards = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting product: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        void AddNewStandard()
        {
            if (SelectedProduct == null)
                return;

            SelectedStandard = null;
            IsNewStandard = true;
            IsEditingStandard = true;

            MachineNumber = string.Empty;
            QuantityPerCycle = 18; // Default value

            StandardWeight = 0;
            WeightTolerancePlus = 0;
            WeightToleranceMinus = 0;

            StandardHeight = 0;
            HeightTolerancePlus = 0;
            HeightToleranceMinus = 0;

            StandardRimThickness = 0;
            RimThicknessTolerancePlus = 0;
            RimThicknessToleranceMinus = 0;

            StandardLoad = 0;
            LoadTolerancePlus = 0;
            LoadToleranceMinus = 0;

            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        void EditStandard(ProductStandard standard)
        {
            if (standard == null)
                return;

            SelectedStandard = standard;
            IsNewStandard = false;
            IsEditingStandard = true;

            MachineNumber = standard.MachineNumber;
            QuantityPerCycle = standard.QuantityPerCycle;

            StandardWeight = standard.StandardWeight;
            WeightTolerancePlus = standard.WeightTolerancePlus;
            WeightToleranceMinus = standard.WeightToleranceMinus;

            StandardHeight = standard.StandardHeight;
            HeightTolerancePlus = standard.HeightTolerancePlus;
            HeightToleranceMinus = standard.HeightToleranceMinus;

            StandardRimThickness = standard.StandardRimThickness;
            RimThicknessTolerancePlus = standard.RimThicknessTolerancePlus;
            RimThicknessToleranceMinus = standard.RimThicknessToleranceMinus;

            StandardLoad = standard.StandardLoad;
            LoadTolerancePlus = standard.LoadTolerancePlus;
            LoadToleranceMinus = standard.LoadToleranceMinus;

            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        async Task SaveStandardAsync()
        {
            if (string.IsNullOrWhiteSpace(MachineNumber) || QuantityPerCycle <= 0)
            {
                ErrorMessage = "Please fill in all required fields with valid values";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                ProductStandard standard;

                if (IsNewStandard)
                {
                    standard = new ProductStandard
                    {
                        ProductId = SelectedProduct.Id,
                        MachineNumber = MachineNumber,
                        QuantityPerCycle = QuantityPerCycle,

                        StandardWeight = StandardWeight,
                        WeightTolerancePlus = WeightTolerancePlus,
                        WeightToleranceMinus = WeightToleranceMinus,

                        StandardHeight = StandardHeight,
                        HeightTolerancePlus = HeightTolerancePlus,
                        HeightToleranceMinus = HeightToleranceMinus,

                        StandardRimThickness = StandardRimThickness,
                        RimThicknessTolerancePlus = RimThicknessTolerancePlus,
                        RimThicknessToleranceMinus = RimThicknessToleranceMinus,

                        StandardLoad = StandardLoad,
                        LoadTolerancePlus = LoadTolerancePlus,
                        LoadToleranceMinus = LoadToleranceMinus,

                        CreatedAt = DateTime.Now,
                        CreatedById = _authService.CurrentUser.Id
                    };
                }
                else
                {
                    standard = SelectedStandard;

                    standard.MachineNumber = MachineNumber;
                    standard.QuantityPerCycle = QuantityPerCycle;

                    standard.StandardWeight = StandardWeight;
                    standard.WeightTolerancePlus = WeightTolerancePlus;
                    standard.WeightToleranceMinus = WeightToleranceMinus;

                    standard.StandardHeight = StandardHeight;
                    standard.HeightTolerancePlus = HeightTolerancePlus;
                    standard.HeightToleranceMinus = HeightToleranceMinus;

                    standard.StandardRimThickness = StandardRimThickness;
                    standard.RimThicknessTolerancePlus = RimThicknessTolerancePlus;
                    standard.RimThicknessToleranceMinus = RimThicknessToleranceMinus;

                    standard.StandardLoad = StandardLoad;
                    standard.LoadTolerancePlus = LoadTolerancePlus;
                    standard.LoadToleranceMinus = LoadToleranceMinus;
                }

                await _databaseService.SaveProductStandardAsync(standard);

                // Refresh the standards list
                await LoadStandardsAsync(SelectedProduct);

                IsEditingStandard = false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving standard: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        void CancelStandardEdit()
        {
            IsEditingStandard = false;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        async Task DeleteStandardAsync(ProductStandard standard)
        {
            if (standard == null)
                return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                $"Are you sure you want to delete this standard for machine {standard.MachineNumber}?",
                "Yes", "No");

            if (!confirm)
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                await _databaseService.DeleteProductStandardAsync(standard);
                ProductStandards.Remove(standard);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting standard: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        void BackToProducts()
        {
            IsViewingStandards = false;
            ProductStandards.Clear();
            SelectedProduct = null;
        }
    }
}