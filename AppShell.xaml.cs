namespace PlasticQC
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes with proper namespaces
            Routing.RegisterRoute("UserManagement", typeof(PlasticQC.Views.UserManagementPage));
            Routing.RegisterRoute("ProductManagement", typeof(PlasticQC.Views.ProductManagementPage));
            Routing.RegisterRoute("Records", typeof(PlasticQC.Views.RecordsPage));

            // These should match the string used in navigation
            System.Diagnostics.Debug.WriteLine("AppShell: Routes registered");
        }
    }
}