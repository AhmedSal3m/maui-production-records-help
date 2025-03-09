using PlasticQC.ViewModels;
using System.Diagnostics;

namespace PlasticQC.Views
{
    public partial class AdminPanelPage : ContentPage
    {
        private readonly AdminPanelViewModel _viewModel;

        public AdminPanelPage(AdminPanelViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
            Debug.WriteLine("AdminPanelPage initialized with viewModel");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("AdminPanelPage appeared");
        }

        private async void OnUserManagementClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("User Management button clicked");
            try
            {
                await Shell.Current.GoToAsync("UserManagement");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                await DisplayAlert("Error", "Could not navigate to User Management", "OK");
            }
        }

        private async void OnProductManagementClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Product Management button clicked");
            try
            {
                await Shell.Current.GoToAsync("ProductManagement");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                await DisplayAlert("Error", "Could not navigate to Product Management", "OK");
            }
        }

        private async void OnViewRecordsClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("View Records button clicked");
            try
            {
                await Shell.Current.GoToAsync("Records");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                await DisplayAlert("Error", "Could not navigate to Records", "OK");
            }
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Logout button clicked");
            try
            {
                var authService = Handler.MauiContext.Services.GetService<PlasticQC.Services.AuthService>();
                authService?.Logout();
                await Shell.Current.GoToAsync("//Login");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Logout error: {ex.Message}");
                await DisplayAlert("Error", "Could not logout", "OK");
            }
        }
    }
}