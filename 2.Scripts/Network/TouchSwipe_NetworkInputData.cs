using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct TouchSwipe_NetworkInputData : INetworkInput
{
    public enum EventType { None, StartDrag, Drag, EndDrag}
    public EventType currentEventType;

    public Vector3 position;
}
