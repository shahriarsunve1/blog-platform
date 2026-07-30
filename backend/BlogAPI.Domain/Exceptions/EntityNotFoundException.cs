namespace BlogAPI.Domain.Exceptions;

/// <summary>
/// Exception thrown when an entity is not found
/// </summary>
public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message) : base(message) { }
    
    public EntityNotFoundException(string entityName, Guid id) 
        : base($"{entityName} with ID '{id}' was not found.") { }
}
