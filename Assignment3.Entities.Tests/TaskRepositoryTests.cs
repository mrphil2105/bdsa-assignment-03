using Assignment3.Core;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Assignment3.Entities.Tests;

public class TaskRepositoryTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private KanbanContext _context;
    public TaskRepositoryTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        var optionsBuilder = new DbContextOptionsBuilder<KanbanContext>();
        //optionsBuilder.UseInMemoryDatabase("KanbanDb");
        //optionsBuilder.UseSqlite("DataSource=:memory:");

        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        optionsBuilder.UseSqlite(connection);
        _context = new KanbanContext(optionsBuilder.Options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public void Create_CreatesTaskEntity_WhenGivenTaskDTO()
    {
        // Arrange
        var tagRepo = new TagRepository(_context);
        var tags = new[] { "ASAP", "Whenever", "Important", "Inspiration" };
        foreach (var tag in tags)
        {
            tagRepo.Create(new TagCreateDTO(tag));
        }
        
        var taskRepo = new TaskRepository(_context);
        var r1 = taskRepo.Create(new TaskCreateDTO("cool title", null, "man, i cant wait to do this task", tags));

        r1.ToTuple().Should().BeEquivalentTo((Response.Created, 1));
    }
}
