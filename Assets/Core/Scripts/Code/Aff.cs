using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TreeSharpPlus;


public enum SYNC_FLAG
{
    START = 0,
    END = 1,
	MIDDLE = 2
}

public class Aff
{
    public readonly string name = "";
    private object node = null;
    public readonly object gesture = null;
    private int layer = -1;
    private int index = -1;
    private float timeOffset = 0;

    public Aff(params object[] tokens)
    {
        foreach (object obj in tokens)
        {
            if (obj.GetType() == typeof(string))
            {
                name = (string)obj;
            }
            else if (obj is Node)
            {
                node = obj;
            }
        }
    }

    public static List<Aff> Flatten(params object[] affs)
    {
        List<Aff> results = new List<Aff>();

        foreach (object obj in affs)
        {
            if (obj.GetType() == typeof(Aff))
            {
                results.Add((Aff)obj);
            }
            else if (obj.GetType() == typeof(List<Aff>))
            {
                results.AddRange((List<Aff>)obj);
            }
        }

        return results;
    }

    public Node GetNode()
    {
        return (Node)node;
    }

    public int GetLayer()
    {
        return layer;
    }

    public void SetLayer(int layer)
    {
        this.layer = layer;
    }

    public void SetIndex(int index)
    {
        this.index = index;
    }

    public int GetIndex()
    {
        return index;
    }

    public float GetTimeOffset()
    {
        return timeOffset;
    }

    public void SetTimeOffset(float timeOffset)
    {
        this.timeOffset = timeOffset;
    }

    public long GetDuration()
    {
        return GetDurationHelper(node);
    }
	/*public void Trim(long time){	
		
		if (node.GetType () == typeof(LeafWait)) {
			LeafWait lw = ((LeafWait)node);
			lw.TrimWait (time);
		}
	}*/
    private long GetDurationHelper(object node)
    {
        long wait = 20;// (long)Mathf.Round(1000 * Time.fixedDeltaTime);

        if (node.GetType().IsSubclassOf(typeof(NodeGroup)))
        {
            NodeGroup nodeGroup = (NodeGroup)node;

            //if (node.GetType() == typeof(Sequence) || node.GetType() == typeof(SequenceGesture)) {
            //wait = (long)Mathf.Round(1000 * Time.fixedDeltaTime) + WAIT_OFFSET;
            //}

            if (node.GetType().IsSubclassOf(typeof(Parallel)))
            {
                wait += GetDurationHelper(nodeGroup.Children[0]);
            }
            else
            {
                foreach (Node child in nodeGroup.Children)
                {
                    if (child != null)
                    {
                        wait += GetDurationHelper(child);
                    }
                }
            }
        }
        else
        {
            if (node.GetType() == typeof(LeafWait))
            {
                wait += ((LeafWait)node).GetWait();
            }
        }

        return wait;
    }
}
