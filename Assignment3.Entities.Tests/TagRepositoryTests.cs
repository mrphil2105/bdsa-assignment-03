using Microsoft.EntityFrameworkCore;

namespace Assignment3.Entities.Tests;

public class TagRepositoryTests : IDisposable
{
    private KanbanContext _context;
    public TagRepositoryTests()
    {
        var optionsBuilder = new DbContextOptionsBuilder<KanbanContext>();
        optionsBuilder.UseInMemoryDatabase("KanbanDb");

        _context = new KanbanContext(optionsBuilder.Options);
    }
    
    [Fact]
    public void Create_CreatesTagEntity_WhenGivenTagDTO()
    {
        
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
