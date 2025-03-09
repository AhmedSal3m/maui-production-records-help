using PlasticQC.ViewModels;
using System.Diagnostics;

namespace PlasticQC.Views
{
    public partial class ProductManagementPage : ContentPage
    {
        private readonly ProductManagementViewModel _viewModel;

        public ProductManagementPage(ProductManagementViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
            Debug.WriteLine("ProductManagementPage initialized");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("ProductManagementPage appeared");
            try
            {
                await _viewModel.LoadDataAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading data: {ex.Message}");
                await DisplayAlert("Error", "Could not load data", "OK");
            }
        }
    }
}