namespace MailSdk.Models;

public class AnsweredQuestion
{
    public required long QuestionId { get; set; }
    public required Question Question { get; set; }
    public required string Answer { get; set; }
}
