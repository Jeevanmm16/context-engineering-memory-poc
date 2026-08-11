namespace ContextEngineering.MemoryPOC.Service;

using ContextEngineering.MemoryPOC.Entity;

public interface IMemoryClassifier
{
    MemoryType Classify(string message);
}
