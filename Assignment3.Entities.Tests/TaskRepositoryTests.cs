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
        
        // Act
        var taskRepo = new TaskRepository(_context);
        var r1 = taskRepo.Create(new TaskCreateDTO("cool title", null, "man, i cant wait to do this task", tags));
        var r2 = taskRepo.Create(new TaskCreateDTO("ddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd", null, "man, i cant wait to do this task", tags));

        // Assert
        r1.ToTuple().Should().BeEquivalentTo((Response.Created, 1));
        r2.ToTuple().Should().BeEquivalentTo((Response.BadRequest, 0));
    }
    
    [Fact]
    public void ReadAll_ReturnsTaskDTO()
    {
        // Arrange
        var tagRepo = new TagRepository(_context);
        var tags = new[] { "ASAP", "Whenever", "Important", "Inspiration" };
        foreach (var tag in tags)
        {
            tagRepo.Create(new TagCreateDTO(tag));
        }
        
        var taskRepo = new TaskRepository(_context);
        var (_, t1) = taskRepo.Create(new TaskCreateDTO("cool title", null, "man, i cant wait to do this task", tags));

        var r = taskRepo.ReadAll().FirstOrDefault();
        
        Assert.Equal(1, r.Id);
        Assert.Equal("cool title", r.Title);
        Assert.Equal(State.New, r.State);
        Assert.Equal(tags, r.Tags);
    }
    
    [Fact]
    public void Update_UpdatesStateOfTask()
    {
        // Arrange
        var tagRepo = new TagRepository(_context);
        var tags = new[] { "ASAP", "Whenever", "Important", "Inspiration" };
        foreach (var tag in tags)
        {
            tagRepo.Create(new TagCreateDTO(tag));
        }
        
        var taskRepo = new TaskRepository(_context);
        var (_, t1) = taskRepo.Create(new TaskCreateDTO("cool title", null, "man, i cant wait to do this task", tags));
        var r = taskRepo.Update(new TaskUpdateDTO(t1, "updated title", null, "man, i cant wait to do this task", tags, State.Active));
        Assert.Equal(Response.Updated, r);
        
        var task = taskRepo.ReadAll().FirstOrDefault();
        
        Assert.Equal(1, task.Id);
        Assert.Equal("updated title", task.Title);
        Assert.Equal(State.Active, task.State);
    }
    
    [Fact]
    public void Delete_RemovesTaskEntity()
    {
        // Arrange
        var tagRepo = new TagRepository(_context);
        var tags = new[] { "ASAP", "Whenever", "Important", "Inspiration" };
        foreach (var tag in tags)
        {
            tagRepo.Create(new TagCreateDTO(tag));
        }
        
        var taskRepo = new TaskRepository(_context);
        var (_, t1) = taskRepo.Create(new TaskCreateDTO("cool title", null, "man, i cant wait to do this task", tags));
        Assert.Single(taskRepo.ReadAll());
        taskRepo.Delete(t1);
        Assert.Empty(taskRepo.ReadAll());
    }
    
    [Fact]
    public void ReadAllByTag_ReturnsOne()
    {
        // Arrange
        var tagRepo = new TagRepository(_context);
        var tags = new[] { "ASAP", "Whenever", "Important", "Inspiration" };
        foreach (var tag in tags)
        {
            tagRepo.Create(new TagCreateDTO(tag));
        }
        
        var taskRepo = new TaskRepository(_context);
        foreach (var tag in tags)
        {
            taskRepo.Create(new TaskCreateDTO("cool title", null, "someone do this", new []{tag}));
        }
        Assert.Single(taskRepo.ReadAllByTag("ASAP"));
        Assert.Single(taskRepo.ReadAllByTag("Whenever"));
        Assert.Single(taskRepo.ReadAllByTag("Important"));
        Assert.Single(taskRepo.ReadAllByTag("Inspiration"));
    }
    
    [Fact]
    public void Read_ReturnsSpecifiedID()
    {
        // Arrange
        var tagRepo = new TagRepository(_context);
        var tags = new[] { "ASAP", "Whenever", "Important", "Inspiration" };
        foreach (var tag in tags)
        {
            tagRepo.Create(new TagCreateDTO(tag));
        }
        
        var taskRepo = new TaskRepository(_context);
        var i = 0;
        foreach (var tag in tags)
        {
            i++;
            taskRepo.Create(new TaskCreateDTO("cool title " + i, null, "someone do this", new []{tag}));
        }

        var r = taskRepo.Read(2);
        
        Assert.Equal("cool title 2", r.Title);
        Assert.Equal(2, r.Id);
        Assert.Single(r.Tags);
        Assert.Equal("Whenever", r.Tags.First());
    }
    
    [Fact]
    public void ReadAllRemoved_ReturnsTaskDTO()
    {
        // Arrange
        var tagRepo = new TagRepository(_context);
        var tags = new[] { "ASAP", "Whenever", "Important", "Inspiration" };
        foreach (var tag in tags)
        {
            tagRepo.Create(new TagCreateDTO(tag));
        }
        
        var taskRepo = new TaskRepository(_context);
        var (_, t1) = taskRepo.Create(new TaskCreateDTO("cool title", null, "man, i cant wait to do this task", tags));
        taskRepo.Update(new TaskUpdateDTO(t1, "updated title", null, "man, i cant wait to do this task", tags, State.Active));
        taskRepo.Delete(t1);
        var r = taskRepo.ReadAll().FirstOrDefault();
        
        Assert.Equal(1, r.Id);
        Assert.Equal(State.Removed, r.State);
    }
}
