namespace ContextEngineering.MemoryPOC.Service;

using ContextEngineering.MemoryPOC.Entity;
using OllamaSharp;
using OllamaSharp.Models.Chat;

public class LLMService : ILLMService
{
    private readonly OllamaApiClient _ollama;

    public LLMService(OllamaApiClient ollama)
    {
        _ollama = ollama;
    }

    public async Task<string> GenerateResponseAsync(string query, List<WorkingMemoryTurn> workingMemory, List<LongTermMemory> retrievedMemories)
    {
        var contextBuilder = new System.Text.StringBuilder();
        
        contextBuilder.AppendLine("## Response Instructions");
        contextBuilder.AppendLine();
        contextBuilder.AppendLine("Answer the user's current question directly using the available context and retrieved memories.");
        contextBuilder.AppendLine();
        contextBuilder.AppendLine("### Rules");
        contextBuilder.AppendLine("1. **Answer the question first.**");
        contextBuilder.AppendLine("   * Do not explain how you found the answer.");
        contextBuilder.AppendLine("   * Do not discuss the conversation history unless the user explicitly asks about it.");
        contextBuilder.AppendLine("2. **Use retrieved memories as factual context.**");
        contextBuilder.AppendLine("   * If a relevant memory contains the answer, use it directly.");
        contextBuilder.AppendLine("   * Do not tell the user that the information came from memory.");
        contextBuilder.AppendLine("3. **Do not repeat or criticize the user's question.**");
        contextBuilder.AppendLine("   * Never say things like: 'You've already answered this question yourself.', 'You mentioned earlier...', 'You're just repeating...', 'What's on your mind?', 'I need a specific question...'");
        contextBuilder.AppendLine("   * Do not make comments about whether the user's question is valid, repetitive, obvious, or already answered.");
        contextBuilder.AppendLine("4. **Be concise and relevant.**");
        contextBuilder.AppendLine("   * Return only the information necessary to answer the current question.");
        contextBuilder.AppendLine("   * Do not add unrelated explanations, suggestions, or follow-up questions unless they are necessary to answer the request.");
        contextBuilder.AppendLine("5. **When the answer is available in memory, answer confidently.**");
        contextBuilder.AppendLine("   * Example:");
        contextBuilder.AppendLine("     * User: 'Which database do I use for my project?'");
        contextBuilder.AppendLine("     * Memory: 'My project uses SQL Server.'");
        contextBuilder.AppendLine("     * Correct response: **'You use SQL Server.'**");
        contextBuilder.AppendLine("6. **Do not expose internal memory or context processing.**");
        contextBuilder.AppendLine("   * Do not mention retrieved memories, working memory, context assembly, memory IDs, retrieval, prompts, or internal processing unless explicitly asked.");
        contextBuilder.AppendLine("7. **If the answer cannot be determined from the available context, say so briefly.**");
        contextBuilder.AppendLine("   * Example: **'I don't have enough information to determine that.'**");
        contextBuilder.AppendLine("   * Do not invent an answer.");
        contextBuilder.AppendLine();
        contextBuilder.AppendLine("### Response Style");
        contextBuilder.AppendLine("The response should be: Direct, Concise, Factual, Focused only on the user's current question.");
        contextBuilder.AppendLine("**Primary rule: Answer what the user asked, not what happened in the conversation.**");
        
        var systemPrompt = contextBuilder.ToString();
        var chat = new Chat(_ollama);

        var messages = new List<Message> { new Message(ChatRole.System, systemPrompt) };
        
        foreach (var turn in workingMemory)
        {
            var role = turn.Role.Equals("User", StringComparison.OrdinalIgnoreCase) ? ChatRole.User : ChatRole.Assistant;
            messages.Add(new Message(role, turn.Content));
        }
        
        var finalUserPrompt = new System.Text.StringBuilder();
        if (retrievedMemories.Any())
        {
            finalUserPrompt.AppendLine("<memory>");
            foreach (var memory in retrievedMemories)
            {
                finalUserPrompt.AppendLine($"- [{memory.MemoryType}] {memory.Content}");
            }
            finalUserPrompt.AppendLine("</memory>");
            finalUserPrompt.AppendLine();
        }
        finalUserPrompt.AppendLine($"Question: {query}");

        messages.Add(new Message(ChatRole.User, finalUserPrompt.ToString()));

        var responseContent = string.Empty;
        
        // Use Langfuse here ideally, but OllamaSharp returns an IAsyncEnumerable
        // We will just process it and let Langfuse HTTP middleware trace it if configured
        var responseStream = _ollama.ChatAsync(new ChatRequest
        {
            Messages = messages
        });

        await foreach (var chunk in responseStream)
        {
            if (chunk?.Message?.Content != null)
            {
                responseContent += chunk.Message.Content;
            }
        }

        return string.IsNullOrWhiteSpace(responseContent) ? "No response generated." : responseContent;
    }
}
