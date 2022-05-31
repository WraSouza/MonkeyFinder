using MonkeyFinder.Services;

namespace MonkeyFinder.ViewModel;

public partial class MonkeysViewModel : BaseViewModel
{
    [ObservableProperty]
    bool isRefreshing;
    public ObservableCollection<Monkey> Monkeys { get; } = new();
    
    MonkeyService monkeyService;
    IGeolocation geolocation;

    public MonkeysViewModel(MonkeyService monkeyService, IGeolocation geolocation)
    {
        Title = "Monkey Finder";
        this.monkeyService = monkeyService;
        this.geolocation = geolocation;
        //GetMonkeysCommand = new Command(async () => await GetMonkeysAsync());
    }

    [ICommand]
    async Task GetMonkeysAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            var monkeys = await monkeyService.GetMonkeys();

            if (Monkeys.Count != 0)
                Monkeys.Clear();

            foreach (var monkey in monkeys)
                Monkeys.Add(monkey);

        }catch(Exception ex)
        {
            Debug.WriteLine($"Unable to get monkeys: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Erro", ex.Message, "OK");

        }
        finally
        {
            IsBusy = false;
            isRefreshing = false;
        }
    }

    [ICommand]
    async Task GetClosestMonkey()
    {
        if (IsBusy || Monkeys.Count == 0)
            return;

        try
        {
            var location = await geolocation.GetLastKnownLocationAsync();

            if(location == null)
            {
                location = await geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(30)
                });
            }

            var first = Monkeys.OrderBy(m => location.CalculateDistance(
                new Location(m.Latitude, m.Longitude), DistanceUnits.Miles)).FirstOrDefault();

            await Application.Current.MainPage.DisplayAlert("", first.Name + " " + first.Location, "OK");
        }catch(Exception ex)
        {
            Debug.WriteLine($"Unable to query location: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Error!", ex.Message, "OK");
        }
    }
}

