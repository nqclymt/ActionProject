using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PKC.ActionEditor.Events
{
    public interface IDragBeginHandler
    {
        void OnDragBegin(PointerEventData eventData);
    }
}
