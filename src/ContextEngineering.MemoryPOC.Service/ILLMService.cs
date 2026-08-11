namespace ContextEngineering.MemoryPOC.Service;

using ContextEngineering.MemoryPOC.Entity;

public interface ILLMService
{
    Task<string> GenerateResponseAsync(string query, List<WorkingMemoryTurn> workingMemory, List<LongTermMemory> retrievedMemories);
}
