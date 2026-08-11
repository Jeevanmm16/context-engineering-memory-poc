namespace ContextEngineering.MemoryPOC.Service;

public interface IMemoryExtractor
{
    bool ShouldPersist(string message);
}
