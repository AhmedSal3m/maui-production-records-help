using Microsoft.Extensions.DependencyInjection;
using PlasticQC.Services;

namespace PlasticQC
{
    public partial class App : Application
    {
        private readonly DatabaseService _databaseService;

        public App(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService;
            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            await _databaseService.InitializeAsync();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}