using UnityEngine;
using System;
using System.Collections;
using TreeSharpPlus;
using RootMotion.FinalIK;
public class MyBehaviorTree : MonoBehaviour
{
	public Transform[] wanders;
	public Transform wander1;
	public Transform wander2;
	public Transform wander3;
	public GameObject daniel,dave,richard,victim;
	public InteractionObject prop;
	private BehaviorMecanim dan,dav,ric;
	private Vector3 dn,dv;
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
		return p.GetComponent<BehaviorMecanim>().Node_GoTo(position);
	}

	protected Node ST_ApproachArea(GameObject p, Transform target)
	{
		Val<Vector3> position = Val.V (() => target.position);
		return p.GetComponent<BehaviorMecanim> ().Node_GoToUpToRadius (position, 2.0f);

	}

	protected Node IntroWalk(){
		return new SequenceParallel (
			this.ST_ApproachAndWait (daniel, wanders[0]),
			this.ST_ApproachAndWait (dave, wanders[1]),
			this.ST_ApproachAndWait(richard,this.wander2));
	}
	protected Node faceEachOther(GameObject p1, GameObject p2){
		Val<Vector3> pos1 = Val.V(() => p1.transform.position);
		Val<Vector3> pos2 = Val.V (() => p2.transform.position);
		return
			new SequenceParallel (
				p1.GetComponent<BehaviorMecanim>().ST_TurnToFace(pos2),
				p2.GetComponent<BehaviorMecanim>().ST_TurnToFace(pos1));


	}
	protected Node shakeHands(GameObject p1, GameObject p2){
		InteractionObject p1hand = p1.transform.Find ("Interactions").transform.Find("INTER_Take").GetComponent<InteractionObject>();
		InteractionObject p2hand = p2.transform.Find ("Interactions").transform.Find("INTER_Take").GetComponent<InteractionObject>();
		return new Sequence (
			new SequenceParallel(p1.GetComponent<BehaviorMecanim>().Node_StartInteraction(FullBodyBipedEffector.RightHand,p2hand),
				p2.GetComponent<BehaviorMecanim>().Node_StartInteraction(FullBodyBipedEffector.RightHand,p1hand)),
				new LeafWait(2000),
				p1.GetComponent<BehaviorMecanim>().Node_StopInteraction(FullBodyBipedEffector.RightHand),
				p2.GetComponent<BehaviorMecanim>().Node_StopInteraction(FullBodyBipedEffector.RightHand)
		);
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
					ranHeads ("SAD"),
					ranHeads("ROAR"),
					ranHeads("ACKNOWLEDGE")
				),
				new SequenceShuffle (
					ranHands ("THINK"),
					ranHands ("CLAP"),
					ranHands ("YAWN"),
					ranHands ("WRITING"),
					ranHands ("CHEER"),
					ranHands("CHESTPUMPSALUTE"),
					ranHands("TEXTING")
				));
		//);
	}
	protected Node Introduction(){
		return new Sequence(new SequenceParallel (
			this.ST_ApproachArea (daniel, this.wander3),
			this.ST_ApproachArea (dave, this.wander3),
			this.ST_ApproachArea (richard, this.wander3)),
			Conversation()
		);
	}
	//protected Node Killer(){

	//}
	protected Node PickUp(GameObject p, InteractionObject o){
		return p.GetComponent<BehaviorMecanim> ().Node_StartInteraction (FullBodyBipedEffector.RightHand, o);
	}

	protected Node Story1(){
		Val<Vector3> rp = Val.V (() => richard.transform.position);
		Val<Vector3> dp = Val.V (() => daniel.transform.position);
		Func<bool> caught = () => ((rp.Value - dp.Value).magnitude < 1.2f);
		Debug.Log((rp.Value - dp.Value).magnitude.ToString());
		return new Sequence (
			ric.Node_GoToUpToRadius(victim.transform.position, 3.0f),
			ric.ST_PlayBodyGesture("DUCK", 3000),
			new SequenceParallel(
				ric.Node_RunTo(Val.V(()=>wanders[2].position)),
				new Sequence(new LeafWait(5000),dan.Node_RunToUpToRadius(Val.V(()=>richard.transform.position),1.5f)),
				new Sequence(new LeafWait(5000),dav.Node_RunToUpToRadius(Val.V(()=>richard.transform.position),1.5f))),
			dan.ST_PlayHandGesture("hitstealth",1000),
			ric.ST_PlayBodyGesture("DYING",250)
		);
	}
	protected Node BuildTreeRoot()
	{

		Val<string> gest = new Val<string> ("WAVE");
		Val<long> dur = new Val<long> (2000);
		//Val<FullBodyBipedEffector> eff;
		//Val<InteractionObject> obj = new Val<InteractionObject> (ls.GetComponent<InteractionObject>());
		Node roaming = new Sequence(
					new LeafWait(500),
				new SequenceParallel(victim.GetComponent<BehaviorMecanim>().Node_OrientTowards(daniel.transform.position),
					dan.Node_OrientTowards(victim.transform.position)),
				dan.Node_HeadLook(victim.transform.position),
				PickUp(daniel,prop),
				new LeafWait(2000),
				dan.ST_PlayHandGesture("pistolaim",5000),
					victim.GetComponent<BehaviorMecanim>().ST_PlayBodyGesture("DYING", 250),
				new SequenceParallel(dan.Node_SitandStand(2000),dan.ST_PlayHandGesture("DRINK",1000)),
				dan.Node_HeadLookStop(),
				dan.Node_StopInteraction(FullBodyBipedEffector.RightHand),
					IntroWalk(),
				new LeafWait(200),
					faceEachOther(daniel, dave),
				shakeHands(daniel,dave),
					new LeafWait(1000),
					dan.Node_HeadLookStop(),
					dav.Node_HeadLookStop(),
				new SequenceParallel(dan.ST_PlayHandGesture("WAVE",2500),
					dav.ST_PlayHandGesture("WAVE",2500)),
					new LeafWait(1000),
				Introduction(),
				Story1()
			);
		return roaming;
	}
}
