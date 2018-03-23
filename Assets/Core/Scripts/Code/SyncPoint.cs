using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncPoint
{
    public string name;
    public SYNC_FLAG flag;
    public float time = 0;

    public SyncPoint(string name) : this(name, SYNC_FLAG.START)
    {

    }

    public SyncPoint(string name, SYNC_FLAG flag)
    {
        this.name = name;
        this.flag = flag;
    }
}