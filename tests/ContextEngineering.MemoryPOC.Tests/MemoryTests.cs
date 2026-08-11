namespace ContextEngineering.MemoryPOC.Tests;

using ContextEngineering.MemoryPOC.Entity;
using ContextEngineering.MemoryPOC.Repository;
using ContextEngineering.MemoryPOC.Service;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

public class MemoryTests
{
    private MemoryDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<MemoryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new MemoryDbContext(options);
    }

    [Fact]
    public void Test1_WorkingMemoryLimit()
    {
        // Arrange
        var workingMemoryManager = new WorkingMemoryManager();

        // Act
        for (int i = 1; i <= 10; i++)
        {
            workingMemoryManager.AddTurn(new WorkingMemoryTurn("User", $"Turn {i}"));
        }

        // Assert
        var context = workingMemoryManager.GetContext();
        Assert.Equal(5, context.Count);
        Assert.Equal("Turn 6", context.First().Content);
        Assert.Equal("Turn 10", context.Last().Content);
    }

    [Fact]
    public async Task Test2_SemanticMemory()
    {
        // Arrange
        var dbContext = GetDbContext();
        var repository = new MemoryRepository(dbContext);
        var manager = new MemoryManager(new MemoryExtractor(), new MemoryClassifier(), repository);

        // Act
        await manager.ProcessAndStoreAsync("My project uses SQL Server.", "User");
        var retrieved = await manager.RetrieveRelevantMemoriesAsync("sql server");

        // Assert
        Assert.Single(retrieved);
        Assert.Equal(MemoryType.Semantic, retrieved[0].MemoryType);
        Assert.Contains("SQL Server", retrieved[0].Content);
    }

    [Fact]
    public async Task Test3_EpisodicMemory()
    {
        // Arrange
        var dbContext = GetDbContext();
        var repository = new MemoryRepository(dbContext);
        var manager = new MemoryManager(new MemoryExtractor(), new MemoryClassifier(), repository);

        // Act
        await manager.ProcessAndStoreAsync("PR #389 was reviewed and 2 issues were found.", "User");
        var retrieved = await manager.RetrieveRelevantMemoriesAsync("pr #389");

        // Assert
        Assert.Single(retrieved);
        Assert.Equal(MemoryType.Episodic, retrieved[0].MemoryType);
        Assert.Contains("2 issues", retrieved[0].Content);
    }

    [Fact]
    public async Task Test4_ProceduralMemory()
    {
        // Arrange
        var dbContext = GetDbContext();
        var repository = new MemoryRepository(dbContext);
        var manager = new MemoryManager(new MemoryExtractor(), new MemoryClassifier(), repository);

        // Act
        await manager.ProcessAndStoreAsync("During code review, always check JWT validation.", "User");
        var retrieved = await manager.RetrieveRelevantMemoriesAsync("jwt");

        // Assert
        Assert.Single(retrieved);
        Assert.Equal(MemoryType.Procedural, retrieved[0].MemoryType);
        Assert.Contains("check JWT validation", retrieved[0].Content);
    }

    [Fact]
    public async Task Test5_LongTermRecall()
    {
        // Arrange
        var dbContext = GetDbContext();
        var repository = new MemoryRepository(dbContext);
        var memoryManager = new MemoryManager(new MemoryExtractor(), new MemoryClassifier(), repository);
        var workingMemory = new WorkingMemoryManager();

        var mockLlm = new Mock<ILLMService>();
        mockLlm.Setup(l => l.GenerateResponseAsync(It.IsAny<string>(), It.IsAny<List<WorkingMemoryTurn>>(), It.IsAny<List<LongTermMemory>>()))
               .ReturnsAsync("Your project uses SQL Server.");

        // Act & Assert

        // Turn 1
        var initialFact = "My project uses SQL Server.";
        workingMemory.AddTurn(new WorkingMemoryTurn("User", initialFact));
        await memoryManager.ProcessAndStoreAsync(initialFact, "User");

        // Turns 2-10 (Pushing initial fact out of working memory)
        for (int i = 2; i <= 10; i++)
        {
            workingMemory.AddTurn(new WorkingMemoryTurn("User", $"Chit chat {i}"));
        }

        // Verify working memory no longer has the fact
        var currentContext = workingMemory.GetContext();
        Assert.DoesNotContain(currentContext, t => t.Content.Contains("SQL Server"));

        // Turn 21 (Retrieval)
        var question = "Which database does my project use?";
        var relevantMemories = await memoryManager.RetrieveRelevantMemoriesAsync(question);
        
        Assert.Single(relevantMemories); // The fact is recalled from Long-Term Memory
        
        var response = await mockLlm.Object.GenerateResponseAsync(question, currentContext, relevantMemories);

        Assert.Equal("Your project uses SQL Server.", response);
    }
}
