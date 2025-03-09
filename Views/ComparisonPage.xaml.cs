using PlasticQC.ViewModels;

namespace PlasticQC.Views
{
    public partial class ComparisonPage : ContentPage
    {
        private readonly ComparisonViewModel _viewModel;

        public ComparisonPage(ComparisonViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadDataAsync();
        }

        private void ProductPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            _viewModel.ProductSelectedCommand.Execute(null);
        }
    }
}