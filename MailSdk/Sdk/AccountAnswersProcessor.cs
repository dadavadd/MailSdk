using MailSdk.AI;
using MailSdk.Models;

namespace MailSdk.Sdk;

public class AccountAnswersProcessor(Account account)
{
    private readonly MailQuestionsSdk _mailRuSdk = new(account);
    private readonly TextGenerator _textGenerator = new();
    private readonly HashSet<long> _processedQuestionIds = new();

    private int _questionLimit;

    public delegate void AccountAnsweredCompleted(string message);
    public delegate void QuestionHasBeenAnswered(AnsweredQuestion answeredQuestion);
    public event AccountAnsweredCompleted? isCompleted;
    public event QuestionHasBeenAnswered? isAnswered;

    public async Task StartAnswering()
    {
        using (_mailRuSdk)
        {
            if (!await InitializeQuestionLimit())
                return;

            await ProcessQuestionsUntilLimitReached();

            isCompleted?.Invoke("Answering process complete.");
        }
    }

    private async Task<bool> InitializeQuestionLimit()
    {
        _questionLimit = (await _mailRuSdk.GetUserQuestionLimits()).Data;
        if (_questionLimit == 0)
        {
            isCompleted?.Invoke("You has been exceeded answers limit");
            return false;
        }
        return true;
    }

    private async Task ProcessQuestionsUntilLimitReached()
    {
        int answeredCount = 0;
        while (answeredCount < _questionLimit)
        {
            var hasNewQuestions = false;

            await foreach (var questionId in _mailRuSdk.GetUpdatedQuestionsIdsAsync(count: 1, page: 0))
            {
                if (_processedQuestionIds.Contains(questionId))
                    continue;

                hasNewQuestions = true;
                if (await ProcessSingleQuestion(questionId))
                    answeredCount++;
            }

            if (!hasNewQuestions)
                await Task.Delay(200);
        }
    }

    private async Task<bool> ProcessSingleQuestion(long questionId)
    {
        try
        {
            var question = await _mailRuSdk.GetQuestionBody(questionId);
            var questionString = $"""
                ID: {questionId}
                Название: {question.Title}
                Описание: {question.Description}
                """;

            var answer = await _textGenerator.GenerateAsnwerAsync(questionString, question.Images);
            var result = await _mailRuSdk.AnswerToQuestionAsync(questionId, answer);

            if (result.Success)
            {
                _processedQuestionIds.Add(questionId);
                isAnswered?.Invoke(new()
                {
                    QuestionId = questionId,
                    Question = question,
                    Answer = answer
                });
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"There was an exception: {ex.Message}");
        }
        return false;
    }
}
