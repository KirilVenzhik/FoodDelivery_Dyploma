using System.Collections.Generic;

namespace Shared.Primitives;

public abstract class Entity
{
    public IReadOnlyCollection<object> DomainEvents => _domainEvents;

    private readonly List<object> _domainEvents = [];

    protected void RaiseDomainEvent(object domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}