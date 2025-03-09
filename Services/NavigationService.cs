using System.Diagnostics;

namespace PlasticQC.Services
{
    public interface INavigationService
    {
        Task NavigateToAsync(string route);
        Task GoBackAsync();
    }

    public class NavigationService : INavigationService
    {
        public async Task NavigateToAsync(string route)
        {
            Debug.WriteLine($"Navigating to: {route}");
            try
            {
                await MainThread.InvokeOnMainThreadAsync(async () => {
                    await Shell.Current.GoToAsync(route);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation error: {ex.Message}");
                // Don't use the fallback navigation as it requires constructors with specific parameters
            }
        }

        public async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}