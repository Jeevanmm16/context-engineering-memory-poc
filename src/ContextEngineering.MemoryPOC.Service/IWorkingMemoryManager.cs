namespace ContextEngineering.MemoryPOC.Service;

using ContextEngineering.MemoryPOC.Entity;

public interface IWorkingMemoryManager
{
    void AddTurn(WorkingMemoryTurn turn);
    List<WorkingMemoryTurn> GetContext();
    void Clear();
}
