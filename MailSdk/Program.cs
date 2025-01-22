using MailSdk.Models;
using MailSdk.Sdk;
using Spectre.Console;


Console.Title = "Rudes YT | MailRu Sdk";

var exitCode = Microsoft.Playwright.Program.Main(["install", "chromium"]);

if (exitCode != 0)
    throw new Exception($"Playwright installation failed with exit code {exitCode}");

await using var auth = new SdkAuthorization();
var account = await auth.AuthInitialize();

AccountAnswersProcessor accountAnswers = new(account);

accountAnswers.isCompleted += static (message) =>
    Console.WriteLine(message);

accountAnswers.isAnswered += static (answeredQuestion) =>
    DisplayQuestionWithAnswer(answeredQuestion.QuestionId,
                              answeredQuestion.Question,
                              answeredQuestion.Answer); 

await accountAnswers.StartAnswering();


static void DisplayQuestionWithAnswer(long questionId, Question question, string answer)
{
    var panel = new Panel($"""
        [bold blue]ID:[/] {questionId}

        [bold green]Название:[/]
        {question.Title}

        [bold yellow]Описание:[/]
        {question.Description}

        [bold magenta]Ответ:[/]
        {answer}
    """) { Border = BoxBorder.Double, Padding = new(2, 1, 2, 1), };

    AnsiConsole.Write(panel);

    if (question.Images.Count > 0)
    {
        var table = new Table()
            .AddColumn(new TableColumn("[blue]Изображения[/]").Centered());

        foreach (var img in question.Images)
            table.AddRow(new Markup($"[link]{img.Url}[/]"));

        table.Border = TableBorder.Rounded;
        AnsiConsole.Write(table);
    }

    AnsiConsole.WriteLine();
    AnsiConsole.Write(new Rule { Style = Style.Parse("blue dim") });
    AnsiConsole.WriteLine();
}
