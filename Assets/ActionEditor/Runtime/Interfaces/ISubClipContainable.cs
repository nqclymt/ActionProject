using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PKC.ActionEditor
{
    public interface ISubClipContainable : IDirectable
    {
        float SubClipOffset { get; set; }
        float SubClipSpeed { get; }
        
        float SubClipLength { get; }
    }
}
