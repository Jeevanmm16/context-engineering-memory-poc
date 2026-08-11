namespace ContextEngineering.MemoryPOC.Service;

using ContextEngineering.MemoryPOC.Entity;
using ContextEngineering.MemoryPOC.Repository;

public class MemoryManager : IMemoryManager
{
    private readonly IMemoryExtractor _extractor;
    private readonly IMemoryClassifier _classifier;
    private readonly IMemoryRepository _repository;

    public MemoryManager(IMemoryExtractor extractor, IMemoryClassifier classifier, IMemoryRepository repository)
    {
        _extractor = extractor;
        _classifier = classifier;
        _repository = repository;
    }

    public async Task ProcessAndStoreAsync(string message, string source)
    {
        if (_extractor.ShouldPersist(message))
        {
            var type = _classifier.Classify(message);
            
            var memory = new LongTermMemory
            {
                MemoryType = type,
                Content = message,
                Source = source
            };

            await _repository.AddMemoryAsync(memory);
        }
    }

    public async Task<List<LongTermMemory>> RetrieveRelevantMemoriesAsync(string query)
    {
        return await _repository.SearchMemoriesAsync(query);
    }
}
