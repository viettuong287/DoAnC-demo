namespace VinhThucAudioGuide;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var localDb = IPlatformApplication.Current?.Services.GetService<Services.LocalDbService>();
        var apiService = IPlatformApplication.Current?.Services.GetService<Services.ApiService>();

        // Chạy đồng bộ ngầm và chờ 3 giây (hoặc chờ đồng bộ xong tuỳ thời gian)
        var syncTask = Task.Run(async () => 
        {
            if (localDb != null && apiService != null)
            {
                await localDb.SyncWithServerAsync(apiService);
            }
        });

        // Đảm bảo hiển thị splash ít nhất 3 giây
        await Task.WhenAll(Task.Delay(3000), syncTask);

        bool isUnlocked = Preferences.Default.Get("IsAppUnlocked", false);
        Application.Current!.MainPage = isUnlocked ? new AppShell() : new QRScannerPage();
    }
}
