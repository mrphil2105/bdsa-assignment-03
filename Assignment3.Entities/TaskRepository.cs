using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using Assignment3.Core;
using Microsoft.EntityFrameworkCore;

namespace Assignment3.Entities;

public class TaskRepository : ITaskRepository
{
    private KanbanContext _context;
    public TaskRepository(KanbanContext context)
    {
        _context = context;
    }
    public (Response Response, int TaskId) Create(TaskCreateDTO task)
    {
        // not ideal solution, since hardcoded constant that doesnt follow migration
        // -- however, database seems to not give a * about constraint...
        if (task.Title.Length > 100)
        {
            return (Response.BadRequest, 0);
        }
        // assignment gave choice of implementing either user or tag, and since we implemented tag, we can't follow requirement 7 for task
        // however, this is how the code would look if we had implemented user instead
        /*var userEntity = _context.Users.SingleOrDefault(t => t.Id == task.AssignedToId);
        if (userEntity == null)
        {
            return (Response.BadRequest, 0);
        }*/
        
        Task taskEntity = new Task();
        taskEntity.Title = task.Title;
        taskEntity.Description = task.Description!;
        taskEntity.State = State.New;
        var tagEntities = _context.Tags.Where(tE => task.Tags.Any(tS => tE.Name == tS)).ToList();
        taskEntity.Tags = tagEntities;
        // requirement 4 mentions Created/StateUpdated - but these are completely new fields
        // that have not been mentioned anywhere. under the assumption that they are long properties/fields on taskEntity
        // this is how the code would have looked
        /*var currentTime = DateTime.UtcNow;
        taskEntity.Created = currentTime;
        taskEntity.StateUpdated = currentTime;*/

        var _ = _context.Tasks.Add(taskEntity);
        try {
            _context.SaveChanges();
            return (Response.Created, taskEntity.Id);
        } catch (DbUpdateException e) {
            _context.ChangeTracker.Clear();
            Console.WriteLine(e.InnerException?.Message);
            return (Response.BadRequest, 0);
        }
    }

    public IReadOnlyCollection<TaskDTO> ReadAll()
    {
        // maybe there's a better way then .Where(...) to get every item
        var r = _context.Tasks.Include(t => t.Tags).Where(p => p.Id != 0).Select(p => new TaskDTO(p.Id, p.Title, "John Doe", p.Tags.Select(t => t.Name).ToImmutableList(), p.State)).ToImmutableList();
        return r;
    }

    public IReadOnlyCollection<TaskDTO> ReadAllRemoved()
    {
        //"John Doe" would instead be p.AssignedTo.Name
        var r = _context.Tasks.Include(t => t.Tags).Where(p => p.State == State.Removed).Select(p => new TaskDTO(p.Id, p.Title, "John Doe", p.Tags.Select(t => t.Name).ToImmutableList(), p.State)).ToImmutableList();
        return r;
    }

    public IReadOnlyCollection<TaskDTO> ReadAllByTag(string tag)
    {
        //"John Doe" would instead be p.AssignedTo.Name
        var r = _context.Tags.Include(t => t.Tasks).FirstOrDefault(t => t.Name == tag)?.Tasks.Select(p => new TaskDTO(p.Id, p.Title, "John Doe", p.Tags.Select(t => t.Name).ToImmutableList(), p.State)).ToImmutableList();
        //var r = _context.Tasks.Include(t => t.Tags).Where(p => p.Tags.Select(x => x.Name).Contains(tag)).Select(p => new TaskDTO(p.Id, p.Title, "John Doe", p.Tags.Select(t => t.Name).ToImmutableList(), p.State)).ToImmutableList();
        return r;
    }

    public IReadOnlyCollection<TaskDTO> ReadAllByUser(int userId)
    {
        //"John Doe" would instead be p.AssignedTo.Name
        var r = _context.Users.Include(t => t.Tasks).FirstOrDefault(u => u.Id == userId)?.Tasks.Select(p => new TaskDTO(p.Id, p.Title, "John Doe", p.Tags.Select(t => t.Name).ToImmutableList(), p.State)).ToImmutableList();
        //var r = _context.Tasks.Include(t => t.Tags).Where(p => p.AssignedTo == _context.Users.FirstOrDefault(u => u.Id == userId)).Select(p => new TaskDTO(p.Id, p.Title, "John Doe", p.Tags.Select(t => t.Name).ToImmutableList(), p.State)).ToImmutableList();
        return r;
    }

    public IReadOnlyCollection<TaskDTO> ReadAllByState(State state)
    {
        //"John Doe" would instead be p.AssignedTo.Name
        var r = _context.Tasks.Include(t => t.Tags).Where(p => p.State == state).Select(p => new TaskDTO(p.Id, p.Title, "John Doe", p.Tags.Select(t => t.Name).ToImmutableList(), p.State)).ToImmutableList();
        return r;
    }

    public TaskDetailsDTO Read(int taskId)
    {
        // these fields dont exist per definition of Task in assignment, so we initialize them to current time
        var dummy = DateTime.UtcNow;
        //"John Doe" would instead be p.AssignedTo.Name
        var r = _context.Tasks.Include(p => p.Tags).Where(t => t.Id == taskId).Select(t => new TaskDetailsDTO(t.Id, t.Title, t.Description, dummy, "John Doe", t.Tags.Select(t => t.Name).ToImmutableList(), t.State, dummy));
        throw new NotImplementedException();
    }

    public Response Update(TaskUpdateDTO task)
    {
        var taskEntity = _context.Tasks.Include(t => t.Tags).SingleOrDefault(t => t.Id == task.Id);
        if (taskEntity != null)
        {
            // not ideal solution, since hardcoded constant that doesnt follow migration
            // -- however, database seems to not give a * about constraint...
            if (task.Title.Length > 100)
            {
                return Response.BadRequest;
            }
            // assignment gave choice of implementing either user or tag, and since we implemented tag, we can't follow requirement 7 for task
            // however, this is how the code would look if we had implemented user instead
            /*var userEntity = _context.Users.SingleOrDefault(t => t.Id == task.AssignedToId);
            if (userEntity == null)
            {
                return Response.BadRequest;
            }
            taskEntity.AssignedTo = userEntity;
            */
            taskEntity.Title = task.Title;
            taskEntity.Description = task.Description!;
            taskEntity.State = task.State;
            var tagEntities = _context.Tags.Where(tE => task.Tags.Any(tS => tE.Name == tS)).ToList();
            taskEntity.Tags = tagEntities;
            
            // requirement 6 mentions StateUpdated - under the assumption that it is a property/field on taskEntity
            // this is how the code would have looked
            /*taskEntity.StateUpdated = DateTime.UtcNow;*/

            var _ = _context.Tasks.Add(taskEntity);
            try {
                _context.SaveChanges();
                return Response.Updated;
            } catch (DbUpdateException e) {
                _context.ChangeTracker.Clear();
                Console.WriteLine(e.InnerException?.Message);
                return Response.BadRequest;
            }

        }
        return Response.NotFound;
    }

    public Response Delete(int taskId)
    {
        var taskEntity = _context.Tasks.Include(t => t.Tags).SingleOrDefault(t => t.Id == taskId);
        if (taskEntity != null)
        {
            if (taskEntity.State is State.Resolved or State.Closed or State.Removed)
            {
                return Response.Conflict;
            }

            if (taskEntity.State == State.Active)
            {
                taskEntity.State = State.Removed;
            } else if (taskEntity.State == State.New)
            {
                _context.Tasks.Remove(taskEntity);
            }
            try {
                _context.SaveChanges();
                return taskEntity.State == State.New ? Response.Deleted : Response.Updated;
            } catch (DbUpdateException e) {
                _context.ChangeTracker.Clear();
                Console.WriteLine(e.InnerException?.Message);
                return Response.BadRequest;
            }
        }
        return Response.NotFound;
    }
}
