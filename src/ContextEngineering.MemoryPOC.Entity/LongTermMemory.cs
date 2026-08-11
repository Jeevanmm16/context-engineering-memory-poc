namespace ContextEngineering.MemoryPOC.Entity;

public class LongTermMemory
{
    public int Id { get; set; }
    public MemoryType MemoryType { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastAccessedAt { get; set; }
}
