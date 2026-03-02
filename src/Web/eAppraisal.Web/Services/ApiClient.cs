using System.Text;
using System.Text.Json;

namespace eAppraisal.Web.Services;

public class ApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string CookieSessionKey = "ApiAuthCookie";

    public ApiClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("eAppraisalApi");
        // Attach the stored API auth cookie from session
        var cookie = _httpContextAccessor.HttpContext?.Session.GetString(CookieSessionKey);
        if (!string.IsNullOrEmpty(cookie))
            client.DefaultRequestHeaders.Add("Cookie", cookie);
        return client;
    }

    /// <summary>
    /// Captures Set-Cookie from any API response and stores it in session.
    /// </summary>
    private void CaptureResponseCookies(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("Set-Cookie", out var setCookies))
        {
            // Extract just the cookie name=value portion (before the first ';')
            var cookieParts = new List<string>();
            foreach (var raw in setCookies)
            {
                var nameValue = raw.Split(';')[0].Trim();
                if (!string.IsNullOrEmpty(nameValue))
                    cookieParts.Add(nameValue);
            }

            if (cookieParts.Count > 0)
            {
                // Merge with existing cookies in session
                var existing = _httpContextAccessor.HttpContext?.Session.GetString(CookieSessionKey);
                var merged = MergeCookies(existing, cookieParts);
                _httpContextAccessor.HttpContext?.Session.SetString(CookieSessionKey, merged);
            }
        }
    }

    private static string MergeCookies(string? existing, List<string> newCookies)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Parse existing
        if (!string.IsNullOrEmpty(existing))
        {
            foreach (var part in existing.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                var eq = part.IndexOf('=');
                if (eq > 0)
                    dict[part[..eq].Trim()] = part;
            }
        }

        // Override with new
        foreach (var nv in newCookies)
        {
            var eq = nv.IndexOf('=');
            if (eq > 0)
                dict[nv[..eq].Trim()] = nv;
        }

        return string.Join("; ", dict.Values);
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        var client = CreateClient();
        var response = await client.GetAsync(url);
        CaptureResponseCookies(response);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string url, T data)
    {
        var client = CreateClient();
        var content = new StringContent(JsonSerializer.Serialize(data, JsonOptions), Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content);
        CaptureResponseCookies(response);
        return response;
    }

    public async Task<TResult?> PostAsync<T, TResult>(string url, T data)
    {
        var response = await PostAsync(url, data);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResult>(json, JsonOptions);
    }

    public async Task<HttpResponseMessage> PutAsync<T>(string url, T data)
    {
        var client = CreateClient();
        var content = new StringContent(JsonSerializer.Serialize(data, JsonOptions), Encoding.UTF8, "application/json");
        var response = await client.PutAsync(url, content);
        CaptureResponseCookies(response);
        return response;
    }

    public async Task<HttpResponseMessage> PostEmptyAsync(string url)
    {
        var client = CreateClient();
        var response = await client.PostAsync(url, null);
        CaptureResponseCookies(response);
        return response;
    }

    /// <summary>
    /// Login call - returns the deserialized result. The auth cookie is automatically stored in session.
    /// </summary>
    public async Task<TResult?> LoginAsync<T, TResult>(string url, T data)
    {
        var client = _httpClientFactory.CreateClient("eAppraisalApi");
        var content = new StringContent(JsonSerializer.Serialize(data, JsonOptions), Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content);
        CaptureResponseCookies(response);
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResult>(json, JsonOptions);
    }
}
