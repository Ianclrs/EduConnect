namespace Ciclo.Core.Entities;

public class StudentParent
{
    public Guid StudentId { get; set; }
    public Guid ParentId { get; set; }

    // Navigation
    public Student Student { get; set; } = null!;
    public User Parent { get; set; } = null!;
}
