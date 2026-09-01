namespace SEVPMS.Realtime.Dispatchers;

public interface IRealtimeDispatcher
{
    Task SendToGroupAsync(
        string groupName,
        string eventName,
        object payload,
        CancellationToken cancellationToken = default);
}
