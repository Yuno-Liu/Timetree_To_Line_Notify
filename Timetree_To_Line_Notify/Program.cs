using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;
using TimeTreeModel;

IConfigurationRoot _config;
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json");
_config = builder.Build();
await GetTimeTreeData(_config);
await SendMessageToLine(_config);

static async Task GetTimeTreeData(IConfiguration _configuration)
{
    HttpClient client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
        _configuration["TimeTreeToken"]);
    HttpResponseMessage response =
        await client.GetAsync(
            "https://timetreeapis.com/calendars/7vxDF2gwAV-6/upcoming_events?timezone=Asia/Taipei&days=7");
    string responseBody = await response.Content.ReadAsStringAsync();
    var data = JsonSerializer.Deserialize<Data>(responseBody);
    Console.WriteLine(data?.DataData[0].Id);
}

static async Task SendMessageToLine(IConfiguration configuration)
{
    HttpClient httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
    httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", configuration["LineNotifyToken"]);
    var content = new Dictionary<string, string>();
    content.Add("message", "👿🥲😂🤣🤡🦄❤️😍😅👌😒😊");
    await httpClient.PostAsync("https://notify-api.line.me/api/notify", new FormUrlEncodedContent(content));
}