using System.Collections.Immutable;
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
            //_context.Tags.Remove(tagEntity);
            
            // same effect as above, just not dumb
            _context.ChangeTracker.Clear();
        }
        return (result, tagEntity.Id);
    }

    public IReadOnlyCollection<TagDTO> ReadAll()
    {
        // maybe there's a better way then .Where(...) to get every item
        var r = _context.Tags.Where(p => p.Id != 0).Select(p => new TagDTO(p.Id, p.Name)).ToImmutableList();
        return r;
    }

    public TagDTO Read(int tagId)
    {
        //  exception thrown if multiple ids found, but thats not possible with our use
        // so no need to worry
        var tagEntity = _context.Tags.SingleOrDefault(t => t.Id == tagId);
        if (tagEntity != null)
        {
            return new TagDTO(tagEntity.Id, tagEntity.Name);
        }
        return null;
    }

    public Response Update(TagUpdateDTO tag)
    {
        var tagEntity = _context.Tags.FirstOrDefault(t => t.Id == tag.Id);
        if (tagEntity != null)
        {
            // same as for create, database ignores length restriction
            // so do a manual check
            if (tag.Name.Length > 50)
            {
                return Response.BadRequest;
            }
            tagEntity.Name = tag.Name;
            try {
                _context.SaveChanges();
                return Response.Updated;
            } catch (DbUpdateException e)
            {
                _context.ChangeTracker.Clear();
                Console.WriteLine(e.InnerException.Message.ToString());
                if (e.InnerException.Message.StartsWith("SQLite Error 19:"))
                {
                    return Response.Conflict;
                } else
                {
                    return Response.BadRequest;
                }
            }
        }
        return Response.NotFound;
    }

    public Response Delete(int tagId, bool force = false)
    {
        throw new NotImplementedException();
    }
}
