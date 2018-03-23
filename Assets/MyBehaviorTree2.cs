using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TreeSharpPlus;
using UnityEngine;

public class MyBehaviorTree2 : MonoBehaviour
{
	public Transform wander1;
	public Transform wander2;
	public Transform wander3;
	public GameObject participant, p2;
	public GameObject police;
	private BehaviorAgent behaviorAgent;
	// Use this for initialization
	void Start ()
	{
		behaviorAgent = new BehaviorAgent (this.BuildTreeRoot ());
		BehaviorManager.Instance.Register (behaviorAgent);
		behaviorAgent.StartBehavior ();

	}

	// Update is called once per frame
	void Update ()
	{

	}

	protected Node ST_ApproachAndWait(Transform target)
	{
		Val<Vector3> position = Val.V (() => target.position);
		return new Sequence( participant.GetComponent<BehaviorMecanim>().Node_GoTo(position), new LeafWait(1000));
	}

	protected Node BuildTreeRoot()
	{
		//Val<float> pp = Val.V (() => police.transform.position.z);
		//Func<bool> act = () => (police.transform.position.z > 10);
		//Node af1 = new Node(this.ST_ApproachAndWait (wander1));
		/*List<Aff>[] affs = new List<Aff>[2];
		affs[0] = new List<Aff>(){new Aff (new SequenceGesture(new LeafWait(1500)), "walk1"),
			new Aff (new SequenceGesture(new LeafWait(5000)), "dance"),
			new Aff(new SequenceGesture(new LeafWait(4000)), "walk2")};
		//affs [0].Add (new Aff (new LeafWait(1500), "walk1"));
		//affs [0].Add (new Aff (new LeafWait(5000), "dance"));
		//affs [0].Add (new Aff(new LeafWait(4000), "walk2"));
		affs [1] = new List<Aff> (){new Aff (new SequenceGesture(new LeafWait(1000)),"Cheer"),
			new Aff (new SequenceGesture(new LeafWait(1000)),"Pump"),
			new Aff (new SequenceGesture(new LeafWait(3000)),"woodcut")};
		Sync[] syncs = new Sync[2];
		SyncPoint[] a = new SyncPoint[3];
		a[0] = new SyncPoint ("walk1");
		a [1] = new SyncPoint ("dance",SYNC_FLAG.END);
		a [2] = new SyncPoint ("walk2");
		syncs [0] = new Sync(a);
		SyncPoint[] b = new SyncPoint[3];
		b[0] = new SyncPoint ("Pump",SYNC_FLAG.START);
		b [1] = new SyncPoint ("Pump",SYNC_FLAG.START);
		b [2] = new SyncPoint ("woodcut");
		syncs [1] = new Sync (b);
	*/
		//this.ST_ApproachAndWait(wander1),
		//participant.GetComponent<BehaviorMecanim> ().Play_AnimationTimeFrame ("Talking On Phone", 0,2000,8000),
		//	participant.GetComponent<BehaviorMecanim> ().Play_Animation ("Talking On Phone", 0),
		//this.ST_ApproachAndWait(wander3)
			
		//Node trigger = new DecoratorLoop (new LeafAssert (act));
		Node root = BehaviorScheduler2.Synchronize (
			new List<Aff>[]{
				new List<Aff>(){
					new Aff(
						"cheer",new LeafWait(3000)
					),
					new Aff(
						"talk",new LeafWait(4000)
					),
					new Aff(
						"walk",new LeafWait(5000)
					)
				},
				new List<Aff>(){
					new Aff(
						"talk2",new LeafWait(8000)
					),
					new Aff(
						"start2",new LeafWait(1000)
					),
					new Aff(
						"walker", new LeafWait(8000)
					)
				}
			},
			new Sync[]{
				new Sync(
					new SyncPoint("cheer",SYNC_FLAG.START),
					new SyncPoint("talk2",SYNC_FLAG.END)
				),
				new Sync(
					new SyncPoint("talk"),
					new SyncPoint("start2")
				),
				new Sync(
					new SyncPoint("walk"),
					new SyncPoint("walker")
				)
			}
		);
		return root;
	}
}
