using Assignment3.Core;

namespace Assignment3.Entities;

public class Task
{
    public int Id { get; set; }
    public string Title { get; set; }
    
    // made nullable, since we didn't implement UserRepository (assignment asked for either Tag or User)
    public User? AssignedTo { get; set; }
    public string Description { get; set; }
    public State State { get; set; }
    public ICollection<Tag> Tags { get; set; }
}
