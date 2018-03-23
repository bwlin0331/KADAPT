
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TreeSharpPlus;
using UnityEngine;

public class BehaviorScheduler2 {


	public static Node Synchronize(List<Aff>[] affs, Sync[] syncs)
	{
		Dictionary<string, Aff> affordances = new Dictionary<string, Aff>();
		//float[] layerTimeOffsets = new float[affs.Length];
		HashSet<int> completedLayers = new HashSet<int>();
		//int[] layerPriorities = new int[] { 2, 1, 0 };
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
			sync.syncPoints = sync.syncPoints.OrderBy(a => affordances[a.name].GetLayer()).ToArray();

			foreach (SyncPoint syncPoint in sync.syncPoints)
			{
				switch (syncPoint.flag)
				{
				case SYNC_FLAG.START:
					syncPoint.time = 0;
					break;
				case SYNC_FLAG.END:
					syncPoint.time = affordances[syncPoint.name].GetDuration();
					break;
				case SYNC_FLAG.MIDDLE:
					syncPoint.time = affordances [syncPoint.name].GetDuration () / 2;
					affordances [syncPoint.name].Trim ((long)Mathf.Round(syncPoint.time));
					break;
				}
			}
		}
			
		for (int layerpair = 0; (layerpair + 1) < affs.Length; layerpair++) { //loop for each layer pair
			int[] layers = { layerpair, layerpair + 1 };
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
			for (int syncIndex = 0; syncIndex < relevantSyncs.Length; syncIndex++) {//loop through our syncs pertaining to current layerpair
				//get syncpoint of first layer
				SyncPoint primarySynch = relevantSyncs[syncIndex].syncPoints.First(a => affordances[a.name].GetLayer() == layers[0]);
				float primarySynchTime = affordances [primarySynch.name].GetTimeOffset() + primarySynch.time;
				//get syncpoint of second layer
				SyncPoint secondarySynch = relevantSyncs[syncIndex].syncPoints.First(a => affordances[a.name].GetLayer() == layers[1]);
				float secondarySynchTime = affordances [secondarySynch.name].GetTimeOffset() + secondarySynch.time;

				//need code for beginning index case here

				if (primarySynchTime > secondarySynchTime) {//first case where we need to pad
					float synchdif = primarySynchTime - secondarySynchTime; //value we need to pad by

					//gets the index of the second layer's affordance
					int padindex = affordances[relevantSyncs[syncIndex].syncPoints.First(b => affordances[b.name].GetLayer() == layers[1]).name].GetIndex();
					//if we're synching the end then we need to insert the padding after the current affordance
					if (secondarySynch.flag == SYNC_FLAG.END) {
						padindex++;
					} else if (secondarySynch.flag == SYNC_FLAG.MIDDLE) {
						// special case
					}

					//insert padding into the correct location
					affs[layers[1]].Insert(padindex, new Aff("lay" + layers[1] + "_pad" + padindex, new LeafWait(Val.V(() => (long)Mathf.Round((synchdif) - (1000 * Time.fixedDeltaTime))))));
				}
				if (primarySynchTime < secondarySynchTime) {//second case where we need to trim
					float synchdif = secondarySynchTime - primarySynchTime; //value we need to trim by
					int trimindex = affordances[relevantSyncs[syncIndex].syncPoints.First(b => affordances[b.name].GetLayer() == layers[1]).name].GetIndex();

					if (secondarySynch.flag == SYNC_FLAG.START) {
						trimindex--;
					} else if (secondarySynch.flag == SYNC_FLAG.MIDDLE) {
						//special case
					}
					//trim out difference in time
					affs [layers [1]] [trimindex].Trim ((long)Mathf.Round(synchdif));
				}

				//third trivial case where nothing happens because timing is equivalent
				//implement: update the offset time of every affordance in a layer after trim, pad, can be made more efficient
				foreach (int layer in layers)
				{
					float timeOffset = 0;
					foreach (Aff aff in affs[layer])
					{
						aff.SetTimeOffset(timeOffset);

						timeOffset += aff.GetDuration();
					}   //Set cumulative time offsets for each Aff
				}
			}
		}

		//Builds out the behavior with the affs dictionary
		for (int layerIndex = 0; layerIndex < affs.Length; layerIndex++)
		{
			//affs[layerIndex].Insert(0, new Aff("lay" + layerIndex + "_pad0", new LeafWait(Val.V(() => (long)Mathf.Round(layerTimeOffsets[layerIndex])))));

			Sequence sequence = new Sequence();
			behavior.Children.Add(sequence);
			foreach (Aff aff in affs[layerIndex])
			{
			//	str += aff.name + "-" + aff.GetDuration() + "\t";
				sequence.Children.Add(aff.GetNode());
			}
			//str += "\n";
		}
		return behavior;
	}

}
