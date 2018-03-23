
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TreeSharpPlus;
using UnityEngine;

public class BehaviorScheduler {


    public static Node Synchronize(List<Aff>[] affs, Sync[] syncs)
    {
        Dictionary<string, Aff> affordances = new Dictionary<string, Aff>();
        float[] layerTimeOffsets = new float[affs.Length];
        HashSet<int> completedLayers = new HashSet<int>();
        int[] layerPriorities = new int[] { 2, 1, 0 };
        SequenceParallel behavior = new SequenceParallel();

        int layerCount = 0;
        foreach (List<Aff> layer in affs)
        {
            int indexCount = 0;
            foreach (Aff aff in layer)
            {
                aff.SetLayer(layerCount);
                aff.SetIndex(indexCount);

                affordances.Add(aff.name, aff);

                indexCount++;
            }
            layerCount++;
        }
        foreach (Sync sync in syncs)
        {
            sync.syncPoints = sync.syncPoints.OrderByDescending(a => layerPriorities[affordances[a.name].GetLayer()]).ToArray();

            foreach (SyncPoint syncPoint in sync.syncPoints)
            {
                switch (syncPoint.flag)
                {
                    case SYNC_FLAG.START:
                        //syncPoint.time = 0;
                        break;
                    case SYNC_FLAG.END:
                        syncPoint.time = affordances[syncPoint.name].GetDuration();
                        break;

                }
            }
        }

        for (int layerPair = 0; layerPair + 1 < affs.Length; layerPair++)
        {
            int[] layers = new int[] { layerPair, layerPair + 1 };  //Layers that will be synchronized
            layers = layers.OrderByDescending(l => layerPriorities[l]).ToArray();
            foreach (int layer in layers)
            {
                float timeOffset = 0;
                foreach (Aff aff in affs[layer])
                {
                    aff.SetTimeOffset(timeOffset);

                    timeOffset += aff.GetDuration();
                }   //Set cumulative time offsets for each Aff
            }

            Sync[] relevantSyncs = syncs.Where(s => s.syncPoints.Select(a => affordances[a.name].GetLayer()).Intersect(layers).Count() == 2).ToArray();
            for (int syncIndex = 0; syncIndex < relevantSyncs.Length; syncIndex++)
            {
                float[] layerSyncDiffs = new float[layers.Length];

                if (syncIndex == 0)
                {
                    for (int layerIndex = 0; layerIndex < layers.Length; layerIndex++)
                    {
                        SyncPoint layerSyncPoint = relevantSyncs[syncIndex].syncPoints.First(a => affordances[a.name].GetLayer() == layers[layerIndex]);    //SyncPoint for layerIndex in relevantSyncs[0]
                        layerSyncDiffs[layerIndex] = affordances[layerSyncPoint.name].GetTimeOffset() + layerSyncPoint.time;
                         
                        //print("sync " + syncIndex + " - " + (syncIndex + 1) + "; layer " + layerIndex + " - " + layerSyncDiffs[layerIndex]);
                    }

                    if (layerSyncDiffs[0] > layerSyncDiffs[1])
                    {
                        //print("ZERO sync " + syncIndex + " - " + (syncIndex + 1) + "; pad");

                        layerTimeOffsets[layers[1]] += layerSyncDiffs[0] - layerSyncDiffs[1];
                    }
                    else if (layerSyncDiffs[0] < layerSyncDiffs[1])
                    {
                        //print("ZERO sync " + syncIndex + " - " + (syncIndex + 1) + "; trim " + (layerSyncDiffs[1] - layerSyncDiffs[0]));

                        if (completedLayers.Count == 0)
                        {
                            layerTimeOffsets[layers[0]] += layerSyncDiffs[1] - layerSyncDiffs[0];
                        }
                        else
                        {
                            foreach (int layer in completedLayers)
                            {
                                layerTimeOffsets[layer] += layerSyncDiffs[1] - layerSyncDiffs[0];
                            }   //If a completed layer needs padding, all completed layers do, because they are synchronized at the start
                        }
                    }
                }

                if (syncIndex + 1 < relevantSyncs.Length)
                {
                    for (int layerIndex = 0; layerIndex < layers.Length; layerIndex++)
                    {
                        SyncPoint layerSyncPoint = relevantSyncs[syncIndex].syncPoints.First(a => affordances[a.name].GetLayer() == layers[layerIndex]);    //SyncPoint for layerIndex in relevantSyncs[syncIndex]
                        SyncPoint layerNextSyncPoint = relevantSyncs[syncIndex + 1].syncPoints.First(a => affordances[a.name].GetLayer() == layers[layerIndex]);    //SyncPoint for layerIndex in relevantSyncs[syncIndex + 1]
                        layerSyncDiffs[layerIndex] = (affordances[layerNextSyncPoint.name].GetTimeOffset() + layerNextSyncPoint.time) - (affordances[layerSyncPoint.name].GetTimeOffset() + layerSyncPoint.time);

                        //print("sync " + syncIndex + " - " + (syncIndex + 1) + "; layer " + layerIndex + " - " + layerSyncDiffs[layerIndex]);
                    }

                    if (layerSyncDiffs[0] > layerSyncDiffs[1])
                    {
                        //print("sync " + syncIndex + " - " + (syncIndex + 1) + "; pad");

                        int paddingStart = affordances[relevantSyncs[syncIndex].syncPoints.First(b => affordances[b.name].GetLayer() == layers[1]).name].GetIndex();
                        int paddingEnd = affordances[relevantSyncs[syncIndex + 1].syncPoints.First(b => affordances[b.name].GetLayer() == layers[1]).name].GetIndex();
                        if ((affs[layers[1]][paddingStart].GetDuration() - relevantSyncs[syncIndex].syncPoints.First(b => affordances[b.name].GetLayer() == layers[1]).time) == 0)//if flag is end
                        {
                            paddingStart++;
                        }
                        if (relevantSyncs[syncIndex + 1].syncPoints.First(b => affordances[b.name].GetLayer() == layers[1]).time == 0)//if flag is start
                        {
                            paddingEnd--;
                        }

                        if (paddingEnd < paddingStart)
                        {
                            affs[layers[1]].Insert(paddingStart, new Aff("lay" + layers[1] + "_pad" + paddingStart, new LeafWait(Val.V(() => (long)Mathf.Round((layerSyncDiffs[0] - layerSyncDiffs[1]) - (1000 * Time.fixedDeltaTime))))));
                        }
                        else if (paddingEnd == paddingStart)
                        {
                            if (affordances[relevantSyncs[syncIndex].syncPoints.First(b => affordances[b.name].GetLayer() == layers[1]).name].GetIndex() == affordances[relevantSyncs[syncIndex + 1].syncPoints.First(b => affordances[b.name].GetLayer() == layers[1]).name].GetIndex())
                            {
                                affs[layers[1]].Insert(paddingStart + 1, new Aff("lay" + layers[1] + "_pad" + (paddingStart + 1), new LeafWait(Val.V(() => (long)Mathf.Round((layerSyncDiffs[0] - layerSyncDiffs[1]) / 2 - (1000 * Time.fixedDeltaTime))))));
                                affs[layers[1]].Insert(paddingStart, new Aff("lay" + layers[1] + "_pad" + paddingStart, new LeafWait(Val.V(() => (long)Mathf.Round((layerSyncDiffs[0] - layerSyncDiffs[1]) / 2 - (1000 * Time.fixedDeltaTime))))));
                            }
                            else if (paddingStart == affordances[relevantSyncs[syncIndex].syncPoints.First(b => affordances[b.name].GetLayer() == layers[1]).name].GetIndex())
                            {
                                affs[layers[1]].Insert(paddingStart + 1, new Aff("lay" + layers[1] + "_pad" + (paddingStart + 1), new LeafWait(Val.V(() => (long)Mathf.Round((layerSyncDiffs[0] - layerSyncDiffs[1]) - (1000 * Time.fixedDeltaTime))))));
                            }
                            else if (paddingStart == affordances[relevantSyncs[syncIndex + 1].syncPoints.First(b => affordances[b.name].GetLayer() == layers[1]).name].GetIndex())
                            {
                                affs[layers[1]].Insert(paddingStart, new Aff("lay" + layers[1] + "_pad" + paddingStart, new LeafWait(Val.V(() => (long)Mathf.Round((layerSyncDiffs[0] - layerSyncDiffs[1]) - (1000 * Time.fixedDeltaTime))))));
                            }
                        }
                        else
                        {
                            for (int paddingIndex = paddingEnd; paddingIndex > paddingStart; paddingIndex--)
                            {
                                affs[layers[1]].Insert(paddingIndex, new Aff("lay" + layers[1] + "_pad" + paddingIndex, new LeafWait(Val.V(() => (long)Mathf.Round((layerSyncDiffs[0] - layerSyncDiffs[1]) / (paddingEnd - paddingStart) - (1000 * Time.fixedDeltaTime))))));
                            }
                        }
                    }
                    else if (layerSyncDiffs[0] < layerSyncDiffs[1])
                    {
                        List<ActionPoint> trimPoints = new List<ActionPoint>();
                        float trimmableSum = 0;

                        for (int paddingIndex = affordances[relevantSyncs[syncIndex].syncPoints.First(b => affordances[b.name].GetLayer() == layers[1]).name].GetIndex(); paddingIndex <= affordances[relevantSyncs[syncIndex + 1].syncPoints.First(b => affordances[b.name].GetLayer() == layers[1]).name].GetIndex(); paddingIndex++)
                        {
                            if (paddingIndex == affordances[relevantSyncs[syncIndex].syncPoints.First(b => affordances[b.name].GetLayer() == layers[1]).name].GetIndex())
                            {
                                float trimmable = affs[layers[1]][paddingIndex].GetDuration() - relevantSyncs[syncIndex].syncPoints.First(b => affordances[b.name].GetLayer() == layers[1]).time;
                                if (trimmable > 0 && trimmable < affs[layers[1]][paddingIndex].GetDuration())
                                {
                                    trimPoints.Add(new ActionPoint(affs[layers[1]][paddingIndex], trimmable, ACTION_TYPE.END));
                                }
                                else if (trimmable == affs[layers[1]][paddingIndex].GetDuration())
                                {
                                    trimPoints.Add(new ActionPoint(affs[layers[1]][paddingIndex], trimmable, ACTION_TYPE.BOTH));
                                }
                                trimmableSum += trimmable;
                            }
                            else if (paddingIndex == affordances[relevantSyncs[syncIndex + 1].syncPoints.First(b => affordances[b.name].GetLayer() == layers[1]).name].GetIndex())
                            {
                                float trimmable = relevantSyncs[syncIndex + 1].syncPoints.First(b => affordances[b.name].GetLayer() == layers[1]).time;
                                if (trimmable > 0 && trimmable < affs[layers[1]][paddingIndex].GetDuration())
                                {
                                    trimPoints.Add(new ActionPoint(affs[layers[1]][paddingIndex], trimmable, ACTION_TYPE.FRONT));
                                }
                                else if (trimmable == affs[layers[1]][paddingIndex].GetDuration())
                                {
                                    trimPoints.Add(new ActionPoint(affs[layers[1]][paddingIndex], trimmable, ACTION_TYPE.BOTH));
                                }
                                trimmableSum += trimmable;
                            }
                            else
                            {
                                float trimmable = affs[layers[1]][paddingIndex].GetDuration();
                                if (trimmable > 0)
                                {
                                    trimPoints.Add(new ActionPoint(affs[layers[1]][paddingIndex], trimmable, ACTION_TYPE.BOTH));
                                }
                                trimmableSum += trimmable;
                            }
                        }

                        float diff = layerSyncDiffs[1] - layerSyncDiffs[0];
                        foreach (ActionPoint trimPoint in trimPoints)
                        {
                            //print(trimPoint.aff.name + " - " + (int)(trimPoint.trimmable / trimmableSum * diff) + " - " + trimPoint.type);

                            if (trimPoint.aff.GetNode().GetType() == typeof(SequenceGesture))
                            {
                                if (trimPoint.type == ACTION_TYPE.FRONT)
                                {
                                    SequenceGesture temp = (SequenceGesture)trimPoint.aff.GetNode();
                                    ((LeafDoGesture)temp.Children[0]).SetParam(trimPoint.trimmable / trimmableSum * diff);
                                    ((LeafWait)temp.Children[1]).TrimWait((long)Mathf.Round(trimPoint.trimmable / trimmableSum * diff));
                                }
                                else if (trimPoint.type == ACTION_TYPE.END)
                                {
                                    SequenceGesture temp = (SequenceGesture)trimPoint.aff.GetNode();
                                    ((LeafWait)temp.Children[1]).TrimWait((long)Mathf.Round(trimPoint.trimmable / trimmableSum * diff));
                                }
                                else
                                {
                                    SequenceGesture temp = (SequenceGesture)trimPoint.aff.GetNode();
                                    ((LeafDoGesture)temp.Children[0]).SetParam(trimPoint.trimmable / trimmableSum * diff / 2);
                                    ((LeafWait)temp.Children[1]).TrimWait((long)Mathf.Round(trimPoint.trimmable / trimmableSum * diff));
                                }
                            }
                            else if (trimPoint.aff.GetNode().GetType() == typeof(LeafWait))
                            {
                                ((LeafWait)trimPoint.aff.GetNode()).TrimWait((long)Mathf.Round(trimPoint.trimmable / trimmableSum * diff));
                            }
                            else
                            {

                            }
                        }

                        //print("CONFLICT AT SYNC POINT " + syncIndex);
                        //print(syncIndex + " - trim - " + str);
                    }
                }

                int indexCount = 0;
                foreach (Aff a in affs[layers[1]])
                {
                    a.SetIndex(indexCount);

                    indexCount++;
                }
            }

            foreach (int layer in layers)
            {
                if (!completedLayers.Contains(layer))
                {
                    completedLayers.Add(layer);
                }
            }
        }

        string str = "";
        for (int layerIndex = 0; layerIndex < affs.Length; layerIndex++)
        {
            affs[layerIndex].Insert(0, new Aff("lay" + layerIndex + "_pad0", new LeafWait(Val.V(() => (long)Mathf.Round(layerTimeOffsets[layerIndex])))));

            Sequence sequence = new Sequence();
            behavior.Children.Add(sequence);
            foreach (Aff aff in affs[layerIndex])
            {
                str += aff.name + "-" + aff.GetDuration() + "\t";
                sequence.Children.Add(aff.GetNode());
            }
            str += "\n";
        }
       Debug.Log(str);

        return behavior;
    }

}
