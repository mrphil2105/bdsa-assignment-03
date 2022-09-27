using Assignment3.Core;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Assignment3.Entities.Tests;

public class TagRepositoryTests : IDisposable
{
    private readonly ITestOutputHelper _testOutputHelper;
    private KanbanContext _context;
    public TagRepositoryTests(ITestOutputHelper testOutputHelper)
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
    public void Create_CreatesTagEntity_WhenGivenTagDTO()
    {
        var TagRepo = new TagRepository(_context);
        var r1 = TagRepo.Create(
            new TagCreateDTO("cool tag"));
        var r2 = TagRepo.Create(
            new TagCreateDTO("cool tag"));
        var r3 = TagRepo.Create(
            new TagCreateDTO("kkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkz"));
        var r4 = TagRepo.Create(
            new TagCreateDTO("bad tag"));
        
        r1.ToTuple().Should().BeEquivalentTo((Response.Created, 1));
        r2.ToTuple().Should().BeEquivalentTo((Response.Conflict, 0));
        r3.ToTuple().Should().BeEquivalentTo((Response.BadRequest, 0));
        r4.ToTuple().Should().BeEquivalentTo((Response.Created, 2));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
