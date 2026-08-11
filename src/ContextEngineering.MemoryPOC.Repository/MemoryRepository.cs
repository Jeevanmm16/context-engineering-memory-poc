namespace ContextEngineering.MemoryPOC.Repository;

using ContextEngineering.MemoryPOC.Entity;
using Microsoft.EntityFrameworkCore;

public class MemoryRepository : IMemoryRepository
{
    private readonly MemoryDbContext _dbContext;

    public MemoryRepository(MemoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddMemoryAsync(LongTermMemory memory)
    {
        memory.CreatedAt = DateTime.UtcNow;
        memory.LastAccessedAt = DateTime.UtcNow;
        _dbContext.LongTermMemories.Add(memory);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<LongTermMemory>> SearchMemoriesAsync(string query)
    {
        // Simple search for the POC: looks for matching keywords in Content
        var keywords = query.ToLower().Split(new[] { ' ', '?', '.', ',' }, StringSplitOptions.RemoveEmptyEntries);
        
        var allMemories = await _dbContext.LongTermMemories.ToListAsync();
        
        // Basic relevance scoring
        var relevantMemories = allMemories
            .Where(m => keywords.Any(k => m.Content.ToLower().Contains(k)))
            .OrderByDescending(m => keywords.Count(k => m.Content.ToLower().Contains(k)))
            .Take(3)
            .ToList();

        foreach (var memory in relevantMemories)
        {
            memory.LastAccessedAt = DateTime.UtcNow;
        }

        if (relevantMemories.Any())
        {
            await _dbContext.SaveChangesAsync();
        }

        return relevantMemories;
    }
}
