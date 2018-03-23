using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Sync {
    public SyncPoint[] syncPoints;

	public Sync(params SyncPoint[] syncPoints)
    {
        this.syncPoints = syncPoints;
    }
}
