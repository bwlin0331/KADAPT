using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ACTION_TYPE
{
    FRONT,
    END,
    BOTH
}

public class ActionPoint {
    public Aff aff;
    public float trimmable;
    public ACTION_TYPE type;

    public ActionPoint( Aff aff, float trimmable, ACTION_TYPE type)
    {
        this.aff = aff;
        this.trimmable = trimmable;
        this.type = type;
    }
}
