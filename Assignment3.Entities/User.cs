namespace Assignment3.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public ICollection<Task> Tasks { get; set; }
}
