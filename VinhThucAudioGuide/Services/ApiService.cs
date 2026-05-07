using System.Net.Http.Json;
using System.Text.Json;
using VinhThucAudioGuide.Models;

namespace VinhThucAudioGuide.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    // CHÚ Ý QUAN TRỌNG: 
    // 1. Dùng http://10.0.2.2:5299/api/ nếu chạy trên GIẢ LẬP (Emulator)
    // 2. Dùng IP máy tính (VD: http://192.168.1.5:5299/api/) nếu chạy trên MÁY THẬT (Redmi 10C)
    // Bạn hãy thay địa chỉ IP dưới đây cho đúng với IP máy tính của bạn:
    private const string BaseUrl = "http://192.168.1.13:5299/api/"; 

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    public async Task<SyncDataResponse> GetSyncDataAsync(DateTimeOffset? lastSync = null)
    {
        try
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                return null;
            }

            var url = "audiocontent/sync";
            if (lastSync.HasValue)
            {
                url += $"?lastSync={Uri.EscapeDataString(lastSync.Value.ToString("O"))}";
            }

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SyncDataResponse>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching data from API: {ex.Message}");
        }
        return null;
    }

    /// <summary>
    /// Đăng ký hoặc cập nhật thông tin thiết bị lên Server để Admin quản lý
    /// </summary>
    public async Task RegisterDeviceAsync(string languageId)
    {
        await SendHeartbeatAsync(languageId);
    }

    /// <summary>
    /// Gửi tín hiệu Heartbeat để Server biết thiết bị vẫn đang online (thời gian thực)
    /// </summary>
    public async Task SendHeartbeatAsync(string languageId = null)
    {
        try
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet) return;

            var deviceId = Preferences.Default.Get("UniqueDeviceId", string.Empty);
            if (string.IsNullOrEmpty(deviceId))
            {
                deviceId = Guid.NewGuid().ToString();
                Preferences.Default.Set("UniqueDeviceId", deviceId);
            }

            // Lấy languageId mặc định nếu không truyền vào
            if (string.IsNullOrEmpty(languageId))
            {
                languageId = "00000000-0000-0000-0000-000000000000"; // Placeholder nếu chưa có dữ liệu
            }

            // Gửi ĐẦY ĐỦ dữ liệu mà DevicePreferenceUpsertDto yêu cầu để tránh bị Server từ chối
            var deviceDto = new
            {
                DeviceId = deviceId,
                LanguageId = languageId,
                Platform = DeviceInfo.Current.Platform.ToString(),
                DeviceModel = DeviceInfo.Current.Model,
                Manufacturer = DeviceInfo.Current.Manufacturer,
                OsVersion = DeviceInfo.Current.VersionString,
                SpeechRate = 1.0,
                AutoPlay = true
            };

            await _httpClient.PostAsJsonAsync("device-preference", deviceDto);
        }
        catch { /* Bỏ qua lỗi heartbeat */ }
    }

    /// <summary>
    /// Thông báo cho Server biết thiết bị đã thoát App (để giảm số lượng Online ngay lập tức)
    /// </summary>
    public async Task NotifyOfflineAsync()
    {
        try
        {
            var deviceId = Preferences.Default.Get("UniqueDeviceId", string.Empty);
            if (string.IsNullOrEmpty(deviceId)) return;

            await _httpClient.PostAsync($"device-preference/{deviceId}/offline", null);
        }
        catch { }
    }
}

public class SyncDataResponse
{
    public List<SyncLocation> Locations { get; set; } = new();
    public List<SyncLanguage> Languages { get; set; } = new();
    public List<SyncScript> Scripts { get; set; } = new();
}

public class SyncLocation
{
    public string ServerId { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsActive { get; set; }
}

public class SyncLanguage
{
    public string ServerId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
}

public class SyncScript
{
    public string ServerId { get; set; }
    public string LocationId { get; set; }
    public string LanguageId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public bool IsActive { get; set; }
}
