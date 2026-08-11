namespace ContextEngineering.MemoryPOC.Service;

public class MemoryExtractor : IMemoryExtractor
{
    public bool ShouldPersist(string message)
    {
        // Simple rule-based extraction for the POC
        var msgLower = message.ToLower();
        
        // Don't persist simple greetings
        if (msgLower == "hi" || msgLower == "hello" || msgLower == "hey" || msgLower.Length < 10)
        {
            return false;
        }

        // Keywords that suggest facts, events, or procedures
        var keywords = new[] { "my", "project", "use", "review", "pr", "issue", "check", "always", "during", "found" };
        
        return keywords.Any(k => msgLower.Contains(k));
    }
}
