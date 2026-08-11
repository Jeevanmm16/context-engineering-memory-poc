namespace ContextEngineering.MemoryPOC.Repository;

using ContextEngineering.MemoryPOC.Entity;

public interface IMemoryRepository
{
    Task AddMemoryAsync(LongTermMemory memory);
    Task<List<LongTermMemory>> SearchMemoriesAsync(string query);
}
