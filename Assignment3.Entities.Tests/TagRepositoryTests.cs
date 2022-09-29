using Assignment3.Core;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Assignment3.Entities.Tests;

public class TagRepositoryTests : IDisposable
{
    private readonly KanbanContext _context;
    private readonly ITestOutputHelper _testOutputHelper;

    public TagRepositoryTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        var optionsBuilder = new DbContextOptionsBuilder<KanbanContext>();
        //optionsBuilder.UseInMemoryDatabase("KanbanDb");
        //optionsBuilder.UseSqlite("DataSource=:memory:");

        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        optionsBuilder.UseSqlite(connection);
        _context = new KanbanContext(optionsBuilder.Options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public void Create_CreatesTagEntity_WhenGivenTagDTO()
    {
        var tagRepo = new TagRepository(_context);
        var r1 = tagRepo.Create(new TagCreateDTO("cool tag"));
        var r2 = tagRepo.Create(new TagCreateDTO("cool tag"));
        var r3 = tagRepo.Create(new TagCreateDTO(
            "kkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkz"));
        var r4 = tagRepo.Create(new TagCreateDTO("bad tag"));

        r1.ToTuple()
            .Should()
            .BeEquivalentTo((Response.Created, 1));
        r2.ToTuple()
            .Should()
            .BeEquivalentTo((Response.Conflict, 0));
        r3.ToTuple()
            .Should()
            .BeEquivalentTo((Response.BadRequest, 0));
        r4.ToTuple()
            .Should()
            .BeEquivalentTo((Response.Created, 2));
    }

    [Fact]
    public void Read_ReturnsTagDTO_WhenGivenId()
    {
        // Arrange
        var tagRepo = new TagRepository(_context);
        var tagName1 = "cool tag";
        var tagName2 = "cooler tag";

        for (var i = 0; i < 100; i++)
        {
            var _ = tagRepo.Create(new TagCreateDTO("tag" + i));
        }

        var r1 = tagRepo.Create(new TagCreateDTO(tagName1));
        var r2 = tagRepo.Create(new TagCreateDTO(tagName2));

        // Act
        var tag1 = tagRepo.Read(r1.TagId);
        var tag2 = tagRepo.Read(r2.TagId);

        // Assert
        tag1.Name.Should()
            .BeEquivalentTo(tagName1);
        tag2.Name.Should()
            .BeEquivalentTo(tagName2);
    }

    [Fact]
    public void ReadAll_ReturnsAllTagDTO()
    {
        // Arrange
        var tagRepo = new TagRepository(_context);
        var localTags = new List<TagDTO>();

        for (var i = 0; i < 100; i++)
        {
            var t = tagRepo.Create(new TagCreateDTO("tag" + i));
            localTags.Add(new TagDTO(t.TagId, "tag" + i));
        }

        // Act
        var retrievedTags = tagRepo.ReadAll();

        // Assert
        localTags.Should()
            .BeEquivalentTo(retrievedTags);
    }

    [Fact]
    public void Update_UpdatesTagEntity_WhenGivenTagUpdateDTO()
    {
        // Arrange
        var tagRepo = new TagRepository(_context);
        var (_, t1) = tagRepo.Create(new TagCreateDTO("cool tag"));
        var (_, t2) = tagRepo.Create(new TagCreateDTO("awesome tag"));
        var (_, t3) = tagRepo.Create(new TagCreateDTO("bad tag"));

        // Act
        var r1 = tagRepo.Update(new TagUpdateDTO(t1, "coolest tag"));
        var r2 = tagRepo.Update(new TagUpdateDTO(t2, "coolest tag"));
        var r3 = tagRepo.Update(new TagUpdateDTO(t3, "cool tag"));

        var n1 = tagRepo.Read(t1)
            .Name;
        var n2 = tagRepo.Read(t2)
            .Name;
        var n3 = tagRepo.Read(t3)
            .Name;

        // Assert
        Assert.Equal(Response.Updated, r1);
        Assert.Equal(Response.Conflict, r2);
        Assert.Equal(Response.Updated, r3);

        n1.Should()
            .BeEquivalentTo("coolest tag");
        n2.Should()
            .BeEquivalentTo("awesome tag");
        n3.Should()
            .BeEquivalentTo("cool tag");
    }

    [Fact]
    public void Delete_ReturnsDeleted_WhenGivenId()
    {
        // Arrange
        var tagRepo = new TagRepository(_context);
        var tagName1 = "cool tag";
        var (_, id) = tagRepo.Create(new TagCreateDTO(tagName1));

        // Act
        var r1 = tagRepo.Delete(id);
        var r2 = tagRepo.Delete(id);

        // Assert
        Assert.Equal(Response.Deleted, r1);
        Assert.Equal(Response.NotFound, r2);
    }
}
