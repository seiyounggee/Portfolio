using UnityEngine.EventSystems;

public interface IInfiniteScrollContent : IEventSystemHandler
{
    bool Update(int index);
}