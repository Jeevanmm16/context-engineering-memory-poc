namespace ContextEngineering.MemoryPOC.Dto;

using ContextEngineering.MemoryPOC.Entity;

public class ChatResponseDto
{
    public string Reply { get; set; } = string.Empty;
    public List<LongTermMemory> RetrievedMemories { get; set; } = new();
    public List<WorkingMemoryTurn> WorkingMemoryContext { get; set; } = new();
}
