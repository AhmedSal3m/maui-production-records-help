using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PlasticQC.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private bool _isRefreshing;

        [RelayCommand]
        async Task RefreshAsync()
        {
            IsRefreshing = true;
            await LoadDataAsync();
            IsRefreshing = false;
        }

        public virtual Task LoadDataAsync()
        {
            return Task.CompletedTask;
        }
    }
}