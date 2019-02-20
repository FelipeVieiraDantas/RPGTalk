using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace RPGTALK.Timeline
{

	//The Timeline function into the new Unity is awesome. Really cool.
	//But sadly, it still a little raw, there are still room for improvement
	//The way the "Pause" works on the timeline can create unexpected behaviour in many scripts
	//Like RPGTalk, or even Cinemachine. So the class RPGTalkTimeline makes a new way of Pausing
	//The timeline, forcing it back to one position in time.
	//That solution is provisory and should be changed when the Timeline component improves.
	[AddComponentMenu("Seize Studios/RPGTalk/RPGTalk Timeline")]
	public class RPGTalkTimeline : MonoBehaviour {

		public PlayableDirector timelineDirector;
		//[HideInInspector]
		public bool isPaused;
		double pausedTime;

		void Update(){
			if (isPaused) {
				timelineDirector.time = pausedTime;
			}
		}

		/// <summary>
		/// Pauses the timeline
		/// </summary>
		public void Pause(){
			if (!timelineDirector) {
				Debug.LogError ("A director should be setted into the RPGTalkTimeline Component");
				return;
			}
			#if UNITY_EDITOR
			if(Application.isPlaying){
			#endif
			pausedTime = timelineDirector.time;
			isPaused = true;
				#if UNITY_EDITOR
			}
				#endif
		}

		/// <summary>
		/// Resumes the timeline
		/// </summary>
		public void Resume(){
			isPaused = false;
		}
	}

}