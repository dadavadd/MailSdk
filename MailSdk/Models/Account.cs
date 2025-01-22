namespace MailSdk.Models;

public record Account(
    long Id,
    string Nickname,
    string UserToken,
    string UserSalt,
    string UserCookies);
