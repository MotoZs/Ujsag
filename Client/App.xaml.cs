namespace Client
{
    public partial class App : Application
    {
        public App()
        {
            Routing.RegisterRoute("details", typeof(DetailsPage));

            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}