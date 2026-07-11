using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerEventData 
{
    private Event _event;

    public bool HasRect;
    
    public Vector3 MousePosition => _event != null ? _event.mousePosition : Vector3.zero;

    public PointerEventData()
    {
    }

    public PointerEventData(Event e)
    {
        SetEvent(e);
    }

    public void SetEvent(Event e)
    {
        _event = e;
    }

    public void StopPropagation()
    {
        _event.Use();
    }

    public bool IsLeft()
    {
        return _event.button == 0;
    }
    
    public bool IsRight()
    {
        return _event.button == 1;
    }
    

    public bool IsMiddle()
    {
        return _event.button == 2;
    }
    
}
