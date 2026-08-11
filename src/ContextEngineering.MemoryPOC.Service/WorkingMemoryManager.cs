namespace ContextEngineering.MemoryPOC.Service;

using ContextEngineering.MemoryPOC.Entity;

public class WorkingMemoryManager : IWorkingMemoryManager
{
    private readonly List<WorkingMemoryTurn> _turns = new();
    private const int MaxTurns = 5;

    public void AddTurn(WorkingMemoryTurn turn)
    {
        _turns.Add(turn);
        if (_turns.Count > MaxTurns)
        {
            _turns.RemoveAt(0);
        }
    }

    public List<WorkingMemoryTurn> GetContext()
    {
        return _turns.ToList();
    }

    public void Clear()
    {
        _turns.Clear();
    }
}
