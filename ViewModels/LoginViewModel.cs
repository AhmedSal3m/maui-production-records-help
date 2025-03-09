using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlasticQC.Services;
using System.Threading.Tasks;

namespace PlasticQC.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _username;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private string _errorMessage;

        public LoginViewModel(AuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;
            Title = "Login";
        }

        [RelayCommand]
        async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Username and password are required";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var result = await _authService.LoginAsync(Username, Password);

                if (result)
                {
                    // Navigate based on user role
                    if (_authService.IsAdmin)
                    {
                        await _navigationService.NavigateToAsync("//AdminPanel");
                    }
                    else
                    {
                        await _navigationService.NavigateToAsync("//DailyEntry");
                    }
                }
                else
                {
                    ErrorMessage = "Invalid username or password";
                }
            }
            catch (System.Exception)
            {
                ErrorMessage = "An error occurred during login";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}