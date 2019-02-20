using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class RPGTalkCinematicClip : PlayableAsset, ITimelineClipAsset
{
    public RPGTalkCinematicBehaviour template = new RPGTalkCinematicBehaviour ();

    public ClipCaps clipCaps
    {
		get { return ClipCaps.None; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<RPGTalkCinematicBehaviour>.Create (graph, template);
        return playable;
    }
		
}
