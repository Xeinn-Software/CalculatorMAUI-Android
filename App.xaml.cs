namespace CalculatorMAUI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new NavigationPage(new MainPage())
            {
                BarBackgroundColor = Color.FromArgb("#050D1A"),
                BarTextColor = Color.FromArgb("#5C9AE0")
            });
        }
    }
}
