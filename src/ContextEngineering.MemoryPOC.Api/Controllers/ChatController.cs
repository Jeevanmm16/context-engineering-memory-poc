namespace ContextEngineering.MemoryPOC.Api.Controllers;

using ContextEngineering.MemoryPOC.Dto;
using ContextEngineering.MemoryPOC.Entity;
using ContextEngineering.MemoryPOC.Service;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IMemoryManager _memoryManager;
    private readonly IWorkingMemoryManager _workingMemoryManager;
    private readonly ILLMService _llmService;

    public ChatController(IMemoryManager memoryManager, IWorkingMemoryManager workingMemoryManager, ILLMService llmService)
    {
        _memoryManager = memoryManager;
        _workingMemoryManager = workingMemoryManager;
        _llmService = llmService;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponseDto>> Chat([FromBody] ChatRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("Message cannot be empty.");
        }

        // 1. Store the user's message in working memory
        _workingMemoryManager.AddTurn(new WorkingMemoryTurn("User", request.Message));

        // 2. Retrieve relevant memories based on the current query
        var retrievedMemories = await _memoryManager.RetrieveRelevantMemoriesAsync(request.Message);

        // 3. Get the current working memory context
        var workingMemoryContext = _workingMemoryManager.GetContext();

        // 4. Generate LLM response
        var llmResponse = await _llmService.GenerateResponseAsync(request.Message, workingMemoryContext, retrievedMemories);

        // 5. Store the user's message in long-term memory if applicable
        await _memoryManager.ProcessAndStoreAsync(request.Message, "User");

        // 6. Store the LLM's response in working memory
        _workingMemoryManager.AddTurn(new WorkingMemoryTurn("Assistant", llmResponse));
        
        // 7. Process and store the LLM's response in long-term memory
        await _memoryManager.ProcessAndStoreAsync(llmResponse, "Assistant");

        return Ok(new ChatResponseDto
        {
            Reply = llmResponse,
            RetrievedMemories = retrievedMemories,
            WorkingMemoryContext = workingMemoryContext
        });
    }

    [HttpDelete("working-memory")]
    public IActionResult ClearWorkingMemory()
    {
        _workingMemoryManager.Clear();
        return Ok("Working memory cleared.");
    }
}
