using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PlasticQC.Models;
using PlasticQC.Services;
using System.Threading.Tasks;

namespace PlasticQC.ViewModels
{
    public partial class UserManagementViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly AuthService _authService;

        [ObservableProperty]
        private ObservableCollection<User> _users;

        [ObservableProperty]
        private User _selectedUser;

        [ObservableProperty]
        private bool _isNewUser;

        [ObservableProperty]
        private string _username;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private string _fullName;

        [ObservableProperty]
        private bool _isAdmin;

        [ObservableProperty]
        private bool _isEditing;

        [ObservableProperty]
        private string _errorMessage;

        public UserManagementViewModel(DatabaseService databaseService, AuthService authService)
        {
            _databaseService = databaseService;
            _authService = authService;
            Title = "User Management";
            Users = new ObservableCollection<User>();
        }

        public override async Task LoadDataAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var users = await _databaseService.GetUsersAsync();
                Users.Clear();

                foreach (var user in users)
                {
                    Users.Add(user);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading users: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        void AddNew()
        {
            SelectedUser = null;
            IsNewUser = true;
            IsEditing = true;

            Username = string.Empty;
            Password = string.Empty;
            FullName = string.Empty;
            IsAdmin = false;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        void Edit(User user)
        {
            if (user == null)
                return;

            SelectedUser = user;
            IsNewUser = false;
            IsEditing = true;

            Username = user.Username;
            Password = string.Empty; // Don't display existing password
            FullName = user.FullName;
            IsAdmin = user.IsAdmin;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || (IsNewUser && string.IsNullOrWhiteSpace(Password)) || string.IsNullOrWhiteSpace(FullName))
            {
                ErrorMessage = "Please fill in all required fields";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                // Check for duplicate username
                var existingUser = await _databaseService.GetUserByUsernameAsync(Username);
                if (existingUser != null && (IsNewUser || SelectedUser.Id != existingUser.Id))
                {
                    ErrorMessage = "Username already exists";
                    IsBusy = false;
                    return;
                }

                User user;

                if (IsNewUser)
                {
                    user = new User
                    {
                        Username = Username,
                        Password = Password, // In production, use proper password hashing
                        FullName = FullName,
                        IsAdmin = IsAdmin,
                        CreatedAt = DateTime.Now
                    };
                }
                else
                {
                    user = SelectedUser;
                    user.Username = Username;
                    user.FullName = FullName;
                    user.IsAdmin = IsAdmin;

                    // Only update password if a new one was provided
                    if (!string.IsNullOrWhiteSpace(Password))
                    {
                        user.Password = Password; // In production, use proper password hashing
                    }
                }

                await _databaseService.SaveUserAsync(user);

                // Refresh the list
                await LoadDataAsync();

                IsEditing = false;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving user: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        void Cancel()
        {
            IsEditing = false;
            ErrorMessage = string.Empty;
        }

        [RelayCommand]
        async Task DeleteAsync(User user)
        {
            if (user == null)
                return;

            // Don't allow deleting the current user
            if (user.Id == _authService.CurrentUser.Id)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "You cannot delete your own account.", "OK");
                return;
            }

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                $"Are you sure you want to delete {user.FullName}?",
                "Yes", "No");

            if (!confirm)
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                await _databaseService.DeleteUserAsync(user);
                Users.Remove(user);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting user: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}