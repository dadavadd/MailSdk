
namespace MailSdk.Models;

public record Question(
    string Title,
    string Description,
    List<Image> Images);

public record Image(
    string Url);