using UnityEngine;
using System;
using System.Collections;
using TreeSharpPlus;

public class MyBehaviorTree : MonoBehaviour
{
	public Transform wander1;
	public Transform wander2;
	public Transform wander3;
	public GameObject daniel,dave,richard;
	private BehaviorMecanim dan,dav,ric;
	private GameObject d1, d2;
	private BehaviorAgent behaviorAgent;
	// Use this for initialization
	void Start ()
	{
		dan = daniel.GetComponent<BehaviorMecanim> ();
		dav = dave.GetComponent<BehaviorMecanim> ();
		ric = richard.GetComponent<BehaviorMecanim> ();
		behaviorAgent = new BehaviorAgent (this.BuildTreeRoot ());
		BehaviorManager.Instance.Register (behaviorAgent);
		behaviorAgent.StartBehavior ();

	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	protected Node ST_ApproachAndWait(GameObject p, Transform target)
	{
		Val<Vector3> position = Val.V (() => target.position);
		Val<float> dist = new Val<float> (1.8f);
		return new Sequence( p.GetComponent<BehaviorMecanim>().Node_GoToUpToRadius(position,dist));
	}

	

	protected Node IntroWalk(){
		return new SequenceParallel (
			this.ST_ApproachAndWait (daniel, this.wander1),
			this.ST_ApproachAndWait (dave, this.wander1),
			this.ST_ApproachAndWait(richard,this.wander2));
	}
	protected Node faceEachOther(GameObject p1, GameObject p2){
		Val<Vector3> pos1 = new Val<Vector3> (p1.transform.position);
		Val<Vector3> pos2 = new Val<Vector3> (p2.transform.position);
		return
			new SequenceParallel (
				p1.GetComponent<BehaviorMecanim> ().Node_GoToUpToRadius(pos2,1.5f),
				p2.GetComponent<BehaviorMecanim> ().Node_GoToUpToRadius(pos1,1.5f));


	}
	protected Node ranHeads(Val<string> gest){
		return new Sequence(new SelectorShuffle (dan.ST_PlayFaceGesture (gest, 2000),
			dav.ST_PlayFaceGesture (gest, 2000),
			ric.ST_PlayFaceGesture (gest, 2000)));
	}
	protected Node ranHands(Val<string> gest){
		return new Sequence(new SelectorShuffle (dan.ST_PlayHandGesture (gest, 2000),
			dav.ST_PlayHandGesture (gest, 2000),
			ric.ST_PlayHandGesture (gest, 2000)));
	}
	protected Node Conversation(){
	//	int t = 0;
		//Func<bool> act = () => (t < 10);
		return// new DecoratorLoop (
		//new LeafAssert ((t) => t < 10),
		//new LeafInvoke (t += 1),
			new SequenceParallel (
			new SequenceShuffle (
				ranHeads ("HEADSHAKE"),
				ranHeads ("HEADNOD"),
				ranHeads ("DRINK"),
				ranHeads ("EAT"),
				ranHeads ("SAD")
			),
			new SequenceShuffle (
				ranHands ("THINK"),
				ranHands ("CLAP"),
				ranHands ("YAWN"),
				ranHands ("WRITING"),
				ranHands ("CHEER")
			));
		//);
	}
	protected Node Introduction(){
		return new Sequence(new SequenceParallel (
			this.ST_ApproachAndWait (daniel, this.wander3),
			this.ST_ApproachAndWait (dave, this.wander3),
			this.ST_ApproachAndWait (richard, this.wander3)),
			Conversation()
			);
	}
	protected Node BuildTreeRoot()
	{
		Val<string> gest = new Val<string> ("WAVE");
		Val<long> dur = new Val<long> (2000);
		Node roaming = new DecoratorLoop (
					new Sequence(
					IntroWalk(),
				dan.Node_GoToUpToRadius(dave.transform.position,1.0f),
				dav.Node_GoToUpToRadius(daniel.transform.position,1.0f),
					new LeafWait(1000),
					dan.Node_HeadLookStop(),
					dav.Node_HeadLookStop(),
				new SequenceParallel(dan.ST_PlayHandGesture("WAVE",2500),
					dav.ST_PlayHandGesture("WAVE",2500)),
					new LeafWait(1000),
				Introduction()
				));
		return roaming;
	}
}
