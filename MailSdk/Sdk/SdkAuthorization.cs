using MailSdk.Models;
using Microsoft.Playwright;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace MailSdk.Sdk;

public class SdkAuthorization : IAsyncDisposable
{
    private const string RequestPattern = @"^https://otvet\.mail\.ru/api/v1/questions/\d+/answers$";
    private IPlaywright _playwright = null!;
    private IBrowserContext _browserContext = null!;
    private Account? _account = null!;

    public async Task<Account> AuthInitialize()
    {
        _playwright = await Playwright.CreateAsync();
        _browserContext = await _playwright.Chromium.LaunchPersistentContextAsync("user-data", new()
        {
            Headless = false
        });

        using (_playwright)
        {
            await using (_browserContext)
            {
                var page = _browserContext.Pages[0];
                await page.GotoAsync("https://otvet.mail.ru");
                page.Request += OnRequest;

                while (_account is null)
                    await Task.Delay(100);

                page.Request -= OnRequest;

                await _browserContext.CloseAsync();

                return _account!;
            }
        }
    }

    private async void OnRequest(object? sender, IRequest request)
    {
        if (!Regex.IsMatch(request.Url, RequestPattern))
            return;

        var cookie = (await request.AllHeadersAsync()).Where(header => header.Key == "cookie").First();
        var postData = request.PostDataJSON()!;
        var answerData = await (await request.ResponseAsync())!.JsonAsync();
        _account = ParseAccount(postData, answerData, cookie.Value);
    }

    private Account ParseAccount(
        JsonElement? postData,
        JsonElement? answerData,
        string cookie)
    {
        var token = postData!.Value.GetProperty("token").GetString();
        var salt = postData.Value.GetProperty("salt").GetString();

        var author = answerData!.Value
            .GetProperty("result")
            .GetProperty("answer")
            .GetProperty("author");

        var authorId = author.GetProperty("id").GetInt64();
        var authorNick = author
            .GetProperty("data")
            .GetProperty("nick")
            .GetString();

        return new(authorId,
                   authorNick!,
                   token!,
                   salt!,
                   cookie!);
    }

    public async ValueTask DisposeAsync()
    {
        if (_browserContext is not null)
        {
            await _browserContext.CloseAsync();
            await _browserContext.DisposeAsync();
        }

        _playwright?.Dispose();
    }
}
