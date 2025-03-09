using PlasticQC.ViewModels;
using System.Diagnostics;

namespace PlasticQC.Views
{
    public partial class UserManagementPage : ContentPage
    {
        private readonly UserManagementViewModel _viewModel;

        public UserManagementPage(UserManagementViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
            Debug.WriteLine("UserManagementPage initialized");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("UserManagementPage appeared");
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