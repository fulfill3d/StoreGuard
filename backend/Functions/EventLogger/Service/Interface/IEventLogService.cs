using StoreGuard.Functions.EventLogger.Data;

namespace StoreGuard.Functions.EventLogger.Service.Interface
{
    public interface IEventLogService
    {
        Task AddEventLogAsync(EventLog eventLog);
    }
}