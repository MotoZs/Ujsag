namespace Client
{
    public partial class App : Application
    {
        public App()
        {
            Routing.RegisterRoute("article", typeof(DetailsPage));

            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}