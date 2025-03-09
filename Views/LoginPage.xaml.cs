using Microsoft.Maui.Controls.Xaml;
using PlasticQC.ViewModels;

namespace PlasticQC.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}