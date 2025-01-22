namespace MailSdk.Sdk;

public abstract class MailSdkBase : IDisposable
{
    protected HttpClient HttpClient { get; set; } = null!;

    protected string UserToken { get; set; }
    protected string UserSalt { get; set; }

    protected MailSdkBase(string cookies,
                          string token,
                          string salt,
                          string apiEndpoint)
    {
        var handler = new HttpClientHandler() { UseCookies = true, CookieContainer = new() };
        HttpClient = new(handler) { BaseAddress = new(apiEndpoint) };

        UserToken = token;
        UserSalt = salt;

        HttpClient.DefaultRequestHeaders.Add("accept", "*/*");
        HttpClient.DefaultRequestHeaders.Add("accept-language", "ru,en;q=0.9");
        HttpClient.DefaultRequestHeaders.Add("origin", "https://otvet.mail.ru");
        HttpClient.DefaultRequestHeaders.Add("referer", $"https://otvet.mail.ru/question/{Random.Shared.Next(2000000, 2999999)}");
        HttpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Chromium\";v=\"130\", \"YaBrowser\";v=\"24.12\", \"Not?A_Brand\";v=\"99\", \"Yowser\";v=\"1.5\"");
        HttpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
        HttpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
        HttpClient.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
        HttpClient.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
        HttpClient.DefaultRequestHeaders.Add("sec-fetch-site", "same-origin");
        HttpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 YaBrowser/24.12.0.0 Safari/537.36");
        HttpClient.DefaultRequestHeaders.Add("x-dwh-platform", "web");
        HttpClient.DefaultRequestHeaders.Add("cookie", cookies);
    }

    public void Dispose()
    {
        HttpClient?.Dispose();
    }
}
