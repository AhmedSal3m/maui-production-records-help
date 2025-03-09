using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlasticQC.Services;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PlasticQC.ViewModels
{
    public partial class AdminPanelViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly INavigationService _navigationService;

        public AdminPanelViewModel(AuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;
            Title = "Admin Panel";
            Debug.WriteLine("AdminPanelViewModel initialized");
        }

        [RelayCommand]
        async Task ManageUsers()
        {
            Debug.WriteLine("ManageUsers clicked");
            await _navigationService.NavigateToAsync("UserManagement");
        }

        [RelayCommand]
        async Task ManageProducts()
        {
            Debug.WriteLine("ManageProducts clicked");
            await _navigationService.NavigateToAsync("ProductManagement");
        }

        [RelayCommand]
        async Task ViewRecords()
        {
            Debug.WriteLine("ViewRecords clicked");
            await _navigationService.NavigateToAsync("Records");
        }

        [RelayCommand]
        async Task Logout()
        {
            Debug.WriteLine("Logout clicked");
            _authService.Logout();
            await _navigationService.NavigateToAsync("//Login");
        }
    }
}