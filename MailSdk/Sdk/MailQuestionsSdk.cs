using System.Net;
using System.Text;
using System.Text.Json;
using MailSdk.Models;

namespace MailSdk.Sdk;

public class MailQuestionsSdk(
    Account account) : MailSdkBase(
        account.UserCookies,
        account.UserToken,
        account.UserSalt,
        "https://otvet.mail.ru/api/v1/")
{
    public async IAsyncEnumerable<long> GetUpdatedQuestionsIdsAsync(int count, int page)
    {
        var data = new Dictionary<string, string>
        {
            ["n"] = count.ToString(),
            ["p"] = page.ToString(),
            ["state"] = "A",
            ["salt"] = UserSalt,
            ["token"] = UserToken,
            ["platform"] = "web"
        };

        var json = await (await HttpClient
            .PostAsync("questlist", new FormUrlEncodedContent(data)))
            .EnsureSuccessStatusCode()
            .Content
            .ReadAsStringAsync();

        foreach (var id in ParseQuestionId(json))
            yield return id;
    }

    public async Task<Response<Question>> AnswerToQuestionAsync(long questionId, string answerText)
    {
        var data = new
        {
            data = new
            {
                content = new[]
                {
                    new { type = "text", text = answerText }
                }
            },
            salt = UserSalt,
            token = UserToken,
            platform = "web"
        };

        var response = await HttpClient.PostAsync($"questions/{questionId}/answers",
            new StringContent(JsonSerializer.Serialize(data),
            Encoding.UTF8,
            "application/json"));

        return response.StatusCode switch
        {
            (HttpStatusCode)AnswerStatusCode.AnswerLimitExceeded => new("The response limit has been exceeded."),
            HttpStatusCode.Unauthorized => new("You're not logged in."),
            _ => new() { Data = await GetQuestionBody(questionId) },
        };
    }

    public async Task<Question> GetQuestionBody(long questionId)
    {
        var response = await HttpClient.GetStringAsync($"questions/{questionId}?limit=20");
        return ParseQuestionBody(response);
    }

    public async Task<Response<int>> GetUserQuestionLimits()
    {
        var response = await HttpClient.GetAsync("user/limits?");

        return response.StatusCode switch
        {
            (HttpStatusCode)AnswerStatusCode.AnswerLimitExceeded => new("The response limit has been exceeded."),
            HttpStatusCode.Unauthorized => new("You're not logged in."),
            _ => new() { Data = ParseLimits(await response.Content.ReadAsStringAsync()) },
        };
    }

    private IEnumerable<long> ParseQuestionId(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (root.TryGetProperty("qst", out var questions))
        {
            foreach (var question in questions.EnumerateArray())
            {
                var id = long.Parse(question.GetProperty("id").GetString()!);
                yield return id;
            }
        }
    }

    private Question ParseQuestionBody(string json)
    {
        using var jsonElement = JsonDocument.Parse(json);
        var root = jsonElement.RootElement;

        var title = root.GetProperty("result")
                .GetProperty("question")
                .GetProperty("data")
                .GetProperty("title")
                .GetString()!;

        string content = string.Empty;
        var images = new List<Image>();

        var contentArray = root.GetProperty("result")
                              .GetProperty("question")
                              .GetProperty("data")
                              .GetProperty("content");

        foreach (var item in contentArray.EnumerateArray())
        {
            if (item.TryGetProperty("text", out var textElement))
                content = textElement.GetString() ?? string.Empty;

            if (item.TryGetProperty("images", out var imagesArray))
            {
                foreach (var image in imagesArray.EnumerateArray())
                {
                    var sizes = image.GetProperty("sizes");
                    images.Add(new(sizes.GetProperty("origin").GetString()!));
                }
            }
        }

        return new(title, content, images);
    }

    private int ParseLimits(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        var aaqIndex = root
            .GetProperty("result")[2]
            .GetProperty("current_limit")
            .GetInt32();

        return aaqIndex;
    }
}
