using System.Net.Http.Json;
using System.Text.Json;
using VinhThucAudioGuide.Models;

namespace VinhThucAudioGuide.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    // Thay đổi base URL theo địa chỉ IP của server.
    // Nếu chạy máy ảo Android (Emulator), localhost của máy host là 10.0.2.2
    private const string BaseUrl = "http://10.0.2.2:5000/api/"; 

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    public async Task<SyncDataResponse> GetSyncDataAsync()
    {
        try
        {
            // Kiểm tra kết nối mạng trước khi gọi API
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                return null;
            }

            var response = await _httpClient.GetAsync("audiocontent/sync");
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
}
