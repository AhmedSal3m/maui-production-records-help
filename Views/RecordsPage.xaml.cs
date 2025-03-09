using PlasticQC.ViewModels;
using System.Diagnostics;

namespace PlasticQC.Views
{
    public partial class RecordsPage : ContentPage
    {
        private readonly RecordsViewModel _viewModel;

        public RecordsPage(RecordsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
            Debug.WriteLine("RecordsPage initialized");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("RecordsPage appeared");
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