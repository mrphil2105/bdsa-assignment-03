using System.Diagnostics;
using Assignment3.Core;
using Microsoft.EntityFrameworkCore;

namespace Assignment3.Entities;

public class TagRepository : ITagRepository
{
    private KanbanContext _context;
    public TagRepository(KanbanContext context)
    {
        _context = context;
    }
    public (Response Response, int TagId) Create(TagCreateDTO tag)
    {
        // not ideal solution, since hardcoded constant that doesnt follow migration
        // -- however, database seems to not give a * about constraint...
        if (tag.Name.Length > 50)
        {
            return (Response.BadRequest, 0);
        }
        Tag tagEntity = new Tag();
        tagEntity.Name = tag.Name;
        
        var _ = _context.Tags.Add(tagEntity);
        Response result;
        try {
            _context.SaveChanges();
            result = Response.Created;
        } catch (DbUpdateException e) {
            Console.WriteLine(e.InnerException.Message.ToString());
            if (e.InnerException.Message.StartsWith("SQLite Error 19:"))
            {
                result = Response.Conflict;
            } else
            {
                result = Response.BadRequest;
            }
            // for some reason, it doesnt clear bad changes upon trying to save... 
            _context.Tags.Remove(tagEntity);
        }
        return (result, tagEntity.Id);
    }

    public IReadOnlyCollection<TagDTO> ReadAll()
    {
        throw new NotImplementedException();
    }

    public TagDTO Read(int tagId)
    {
        throw new NotImplementedException();
    }

    public Response Update(TagUpdateDTO tag)
    {
        throw new NotImplementedException();
    }

    public Response Delete(int tagId, bool force = false)
    {
        throw new NotImplementedException();
    }
}
