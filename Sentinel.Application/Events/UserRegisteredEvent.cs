namespace Sentinel.Application.Events;

public record UserRegisteredEvent(
    Guid AccountId,
    string Username,
    string Email,
    DateTime RegisteredAt);