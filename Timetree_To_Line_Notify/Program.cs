using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;
using TimeTreeModel;
using CalendarModel;
using RestSharp;

// GetShortUrl("https://google.com.tw");
IConfigurationRoot _config;
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json");
_config = builder.Build();
var timeTreeType = await GetTimeTreeTypeAsync(_config);
await GetTimeTreeData(_config, timeTreeType);


static async Task<List<Classification>> GetTimeTreeTypeAsync(IConfiguration _configuration)
{
    HttpClient client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
        _configuration["TimeTreeToken"]);
    HttpResponseMessage response =
        await client.GetAsync("https://timetreeapis.com/calendars");
    string responseBody = await response.Content.ReadAsStringAsync();
    var data = JsonSerializer.Deserialize<Calendar>(responseBody);
    List<Classification> results = new List<Classification>();
    if (data?.Data != null)
    {
        foreach (var datum in data.Data)
        {
            Classification s = new Classification()
            {
                ID = datum.Id,
                Name = datum.Attributes.Name,
            };
            results.Add(s);
        }
    }

    return results;
}

static async Task GetTimeTreeData(IConfiguration _configuration, List<Classification> types)
{
    string message = "\n=============\n";
    HttpClient client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
        _configuration["TimeTreeToken"]);
    foreach (var type in types)
    {
        HttpResponseMessage response =
            await client.GetAsync(
                $"https://timetreeapis.com/calendars/{type.ID}/upcoming_events?timezone=Asia/Taipei&days=7");
        string responseBody = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<Data>(responseBody);
        if (data?.DataData is { Length: > 0 })
        {
            message += $"{type.Name}\n";
            foreach (var datum in data.DataData)
            {
                // 將起始時間轉換為台灣時間
                var startAt = TimeZoneInfo.ConvertTimeFromUtc(datum.Attributes.StartAt.DateTime,
                    TimeZoneInfo
                        .FindSystemTimeZoneById("Taipei Standard Time"));
                // 將結束時間轉換為台灣時間
                var endAt = TimeZoneInfo.ConvertTimeFromUtc(datum.Attributes.EndAt.DateTime,
                    TimeZoneInfo
                        .FindSystemTimeZoneById("Taipei Standard Time"));
                message += $"標題：{datum.Attributes.Title}\n";
                message += $"起始日期：{startAt:yyyy/MM/dd}\n";
                message += $"起始時間：{startAt:tt hh:mm}\n";
                message += $"結束日期：{endAt:yyyy/MM/dd}\n";
                message += $"結束時間：{endAt:tt hh:mm}\n";
                message += "\n";
            }

            message += $"=============\n";
        }
    }

    await SendMessageToLine(_configuration, message);
}

static async Task SendMessageToLine(IConfiguration configuration, string message)
{
    HttpClient httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
    httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", configuration["LineNotifyToken"]);
    var content = new Dictionary<string, string> { { "message", message } };
    await httpClient.PostAsync("https://notify-api.line.me/api/notify",
        new FormUrlEncodedContent(content));
}


/*
 *             curl -X POST https://api.reurl.cc/shorten \
              -H 'Content-Type: application/json' \
              -H 'reurl-api-key: 4070ff49d794e13311503b663c974755ecd1b638999304df8a38b58d65165567c4f5d6' \
              -d '{ "url" : "https://www.google.com", "utm_source" : "FB_AD" }'
 */


static string GetShortUrl(string longUrl)
{
    //            curl -X POST https://api.reurl.cc/shorten \
    // -H 'Content-Type: application/json' \
    // -H 'reurl-api-key: 4070ff49d794e13311503b663c974755ecd1b638999304df8a38b58d65165567c4f5d6' \
    // -d '{ "url" : "https://www.google.com", "utm_source" : "FB_AD" }'
    var client = new RestClient("https://api.reurl.cc/shorten");
    var request = new RestRequest();
    request.AddHeader("Content-Type", "application/json");
    request.AddHeader("reurl-api-key",
        "4070ff49d794e13311503b663c974755ecd1b638999304df8a38b58d65165567c4f5d6");
    request.AddJsonBody(new { url = longUrl, utm_source = "" });
    var response = client.Post(request);
    var content = response.Content; // raw content as string
    return content;

    // HttpClient httpClient = new HttpClient();
    // httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
    // httpClient.DefaultRequestHeaders.Add("reurl-api-key",
    //     "4070ff49d794e13311503b663c974755ecd1b638999304df8a38b58d65165567c4f5d6/");
    // var content = new FormUrlEncodedContent(new[]
    // {
    //     new KeyValuePair<string, string>("Content-Type", "application/json"),
    //     new KeyValuePair<string, string>("reurl-api-key",
    //         "4070ff49d794e13311503b663c974755ecd1b638999304df8a38b58d65165567c4f5d6")
    // });
    // var response = await httpClient.PostAsync("https://api.reurl.cc/shorten", content);
    // var responseString = await response.Content.ReadAsStringAsync();
}


public class Classification
{
    public string ID;
    public string Name;
}