namespace Sentinel.Application.Interfaces;

public interface ILoginAttemptService
{
    Task RecordFailedAttemptAsync(string email);
    Task<bool> IsBlockedAsync(string email);
    Task ResetAttemptsAsync(string email);
}