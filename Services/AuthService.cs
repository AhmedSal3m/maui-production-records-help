using PlasticQC.Models;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PlasticQC.Services
{
    public class AuthService
    {
        private readonly DatabaseService _databaseService;
        private User _currentUser;

        public AuthService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Debug.WriteLine("AuthService initialized");
        }

        public User CurrentUser => _currentUser;

        public bool IsLoggedIn => _currentUser != null;

        public bool IsAdmin => _currentUser?.IsAdmin ?? false;

        public async Task<bool> LoginAsync(string username, string password)
        {
            Debug.WriteLine($"LoginAsync called for user: {username}");
            try
            {
                var user = await _databaseService.GetUserByUsernameAsync(username);

                if (user != null)
                {
                    Debug.WriteLine($"User found: {user.Username}, ID: {user.Id}, Admin: {user.IsAdmin}");
                    if (user.Password == password) // In production, use proper password hashing
                    {
                        _currentUser = user;
                        Debug.WriteLine("Login successful");
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine("Password incorrect");
                    }
                }
                else
                {
                    Debug.WriteLine("User not found");
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login error: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public void Logout()
        {
            Debug.WriteLine("User logged out");
            _currentUser = null;
        }
    }
}