using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using System.IO;

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

        /// <summary>
        /// Finish the cutscene. But if it finds a question in it, may jump only to that point
        /// </summary>
        /// <param name="jumpQuestions">If set to <c>true</c> jump questions.</param>
        public void Skip(bool jumpQuestions = false)
        {
            if (jumpQuestions)
            {
                timelineDirector.time = timelineDirector.duration;
                return;
            }

            //If we don't want to jump questions... there is the tricky part... First, let's get our RPGTalk Cinematic Track
            RPGTalkCinematicTrack track = timelineDirector.playableGraph.GetOutput(0).GetReferenceObject() as RPGTalkCinematicTrack;

            //Foreach clip on that track, we will want to read the lineToStart and lineToBreak, pretty much like we do on RPGTalk
            foreach (var clip in track.GetClips())
            {

                RPGTalkCinematicBehaviour behaviour = (clip.asset as RPGTalkCinematicClip).template;
                //If we didn't waited for player interaction, that this isn't the clip we are looking for
                if (!behaviour.pauseUntilTalkEnd)
                {
                    continue;
                }

                //reduce one for the line to Start and break, if they were ints
                //return the default lines to -2 if they were not ints
                int actualLineToStart,actualLineToBreak;
                if (int.TryParse(behaviour.lineToStart, out actualLineToStart))
                {
                    actualLineToStart -= 1;
                }
                else
                {
                    actualLineToStart = -2;
                }
                if (int.TryParse(behaviour.lineToBreak, out actualLineToBreak))
                {
                    if (behaviour.lineToBreak != "-1")
                    {
                        actualLineToBreak -= 1;
                    }
                }
                else
                {
                    actualLineToBreak = -2;
                }


                StringReader reader = new StringReader(behaviour.txtToParse.text);

                string line = reader.ReadLine();
                int currentLine = 0;

                while (line != null)
                {
                    //if the lineToStart or lineToBreak were strings, find out what line they actually were
                    if (actualLineToStart == -2)
                    {
                        if (line.IndexOf("[title=" + behaviour.lineToStart + "]") != -1)
                        {
                            actualLineToStart = currentLine + 1;
                        }
                        else
                        {
                            line = reader.ReadLine();
                            currentLine++;
                            continue;
                        }
                    }
                    if (actualLineToBreak == -2)
                    {
                        if (line.IndexOf("[title=" + behaviour.lineToBreak + "]") != -1)
                        {
                            actualLineToBreak = currentLine - 1;
                        }
                    }

                    if (currentLine >= actualLineToStart)
                    {
                        if (actualLineToBreak < 0 || currentLine <= actualLineToBreak)
                        {
                            //If this line was a choice, we want to skip to it
                            if (line.IndexOf("[choice]") != -1)
                            {
                                timelineDirector.time = clip.start;
                                return;
                            }

                        }
                        else
                        {
                            break;
                        }
                    }

                    line = reader.ReadLine();
                    currentLine++;
                }


            }

            //No questions were found... So let's just go to the end
            timelineDirector.time = timelineDirector.duration;


        }
    }

}