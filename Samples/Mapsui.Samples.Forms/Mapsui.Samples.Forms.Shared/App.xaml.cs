using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace Mapsui.Samples.Forms
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            if (Device.Idiom == TargetIdiom.Phone)
                MainPage = new NavigationPage(new Mapsui.Samples.Forms.MainPage());
            else
                MainPage = new Mapsui.Samples.Forms.MainPageLarge();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
