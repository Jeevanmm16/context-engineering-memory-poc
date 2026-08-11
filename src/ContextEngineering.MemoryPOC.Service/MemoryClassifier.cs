namespace ContextEngineering.MemoryPOC.Service;

using ContextEngineering.MemoryPOC.Entity;

public class MemoryClassifier : IMemoryClassifier
{
    public MemoryType Classify(string message)
    {
        var msgLower = message.ToLower();
        
        // Procedural
        if (msgLower.Contains("always") || msgLower.Contains("during") || msgLower.Contains("check") || msgLower.Contains("how to"))
        {
            return MemoryType.Procedural;
        }

        // Episodic
        var words = msgLower.Split(new[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Contains("pr") || msgLower.Contains("reviewed") || msgLower.Contains("found") || msgLower.Contains("yesterday") || msgLower.Contains("happened"))
        {
            return MemoryType.Episodic;
        }

        // Semantic (fallback for facts)
        return MemoryType.Semantic;
    }
}
