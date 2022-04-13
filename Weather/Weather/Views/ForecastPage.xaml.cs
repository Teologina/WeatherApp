using System.Linq;
using System.Threading.Tasks;
using Weather.Models;
using Weather.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Weather.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

    public partial class ForecastPage : ContentPage
    {
        OpenWeatherService service;
        GroupedForecast groupedforecast;

        public ForecastPage()
        {
            InitializeComponent();

            service = new OpenWeatherService();
            groupedforecast = new GroupedForecast();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            //Code here will run right before the screen appears
            //You want to set the Title or set the City

            //This is making the first load of data
            MainThread.BeginInvokeOnMainThread(async () => { await LoadForecast(); });
        }

        private async Task LoadForecast()
        {

            await Task.Run(() =>
            {
                Task<Forecast> t1 = service.GetForecastAsync(Title);

                Device.BeginInvokeOnMainThread(() =>
                {
                    var items = t1.Result.Items;
                    var groupedItems = items.GroupBy(x => x.DateTime.ToString("dddd, MMMM dd, yyyy"));
                    CustomList.ItemsSource = groupedItems;
                });

            });

        }

        private void RefreshButton_Clicked(object sender, System.EventArgs e)
        {

                     Task<Forecast> t1 = service.GetForecastAsync(Title);

             
                    var items = t1.Result.Items;
                    var groupedItems = items.GroupBy(x => x.DateTime.ToString("dddd, MMMM dd, yyyy"));
                     CustomList.ItemsSource = groupedItems;
              

        }
    }
}