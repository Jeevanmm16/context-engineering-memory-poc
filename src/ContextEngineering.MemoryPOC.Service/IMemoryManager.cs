namespace ContextEngineering.MemoryPOC.Service;

using ContextEngineering.MemoryPOC.Entity;

public interface IMemoryManager
{
    Task ProcessAndStoreAsync(string message, string source);
    Task<List<LongTermMemory>> RetrieveRelevantMemoriesAsync(string query);
}
