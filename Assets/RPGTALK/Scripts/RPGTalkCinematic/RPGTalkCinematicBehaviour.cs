using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using RPGTALK.Timeline;

[Serializable]
public class RPGTalkCinematicBehaviour : PlayableBehaviour
{

	//The variables of the RPGTalk that will be changed
	public TextAsset txtToParse;
	public string lineToStart = "1";
	public string lineToBreak = "-1";
	public float textSpeed = 50;
	[Tooltip("Should the timeline be paused until the player finish the talk?")]
	public bool pauseUntilTalkEnd;

	RPGTalk m_TrackBinding;
	bool m_FirstFrameHappened;
	bool reachedFinal;

	//Saving the defaults
	TextAsset m_txtToParse;
	string m_lineToStart = "1";
	string m_lineToBreak = "-1";
	float m_textSpeed = 50;
	bool m_enableQuickSkip = true;
	bool m_enablePass = true;
	RPGTalkTimeline rpgTime;

    //AutoPass
    public bool autoPass;
    public float secondsAutoPass;
    double[] startTime;

    Playable playableObj;


	//Each frame of the behaviour
	public override void ProcessFrame (Playable playable, FrameData info, object playerData)
	{
		m_TrackBinding = playerData as RPGTalk;
		if (!m_FirstFrameHappened) {
			OnBehaviourPlay (playable, info);
			return;
		}

		

		//for now, we wont support spped changes into timeline
		if(m_TrackBinding.actualTextSpeed != textSpeed){
			m_TrackBinding.actualTextSpeed = textSpeed;
		}


        if(m_TrackBinding.cutscenePosition > m_TrackBinding.rpgtalkElements.Count)
        {
            m_TrackBinding.cutscenePosition = m_TrackBinding.rpgtalkElements.Count;
        }


        //The current character will be calculated based on the textspeed and the time of the playable
        float currentChar = m_TrackBinding.actualTextSpeed * (float)(playable.GetTime() - startTime[m_TrackBinding.cutscenePosition - 1] );







        if (currentChar >= 0)
        {
            //only change it if there is something new to change and we are not paused
            if (Mathf.Min(currentChar, m_TrackBinding.rpgtalkElements[m_TrackBinding.cutscenePosition - 1].dialogText.Length)
            != m_TrackBinding.currentChar && (!rpgTime || !rpgTime.isPaused))
            {
                m_TrackBinding.currentChar = currentChar;
                m_TrackBinding.PutRightTextToShow();
            }
        }

        //if we are autopassing it
        if (autoPass)
        {
            //If we still have more in that talk...
            if (m_TrackBinding.cutscenePosition < m_TrackBinding.rpgtalkElements.Count)
            {
                //If we passed the time that the last character on that cutscene should have been seen
                if (playable.GetTime() >= startTime[m_TrackBinding.cutscenePosition])
                {
                    m_TrackBinding.cutscenePosition++;
                }
            }
            else
            {
                //If, for some reason, we are playing the timeline backwards...
                if (m_TrackBinding.cutscenePosition > 1 && playable.GetTime() < startTime[m_TrackBinding.cutscenePosition - 1])
                {
                    m_TrackBinding.cutscenePosition--;
                }
            }
        }



        //If we reached the final, check if we should pause the timeline until the player finish the talk
        if (playable.GetTime () >= playable.GetDuration () - (double)0.1) {
			if (!reachedFinal) {
				reachedFinal = true;
				if(pauseUntilTalkEnd){
					if (!rpgTime) {
						Debug.LogError ("To use the option 'Pause Until Talk End' the RpgTalk must contain a RPGTalkTimeline Component");
					} else {
						rpgTime.Pause ();
					}
				}
			}
		} else {
			reachedFinal = false;
		}

	}

	public override void OnBehaviourPlay (Playable playable, FrameData info)
	{
		if (!m_TrackBinding)
			return;

		if (!m_FirstFrameHappened)
		{
			//on the first frame, set the defaults to recover after
			m_txtToParse = m_TrackBinding.txtToParse;
			m_lineToBreak = m_TrackBinding.lineToBreak;
			m_lineToStart = m_TrackBinding.lineToStart;
			m_textSpeed = m_TrackBinding.textSpeed;
			m_enableQuickSkip = m_TrackBinding.enableQuickSkip;
			m_enablePass = m_TrackBinding.enablePass;

			//change the rpgTalk parameters
			if (txtToParse != null) {
				m_TrackBinding.txtToParse = txtToParse;
			}

			//If we won't wait for player action to finish, only one line (line to start) will be allowed.
			if (!pauseUntilTalkEnd) {
                //If we didn't set neither pauseUntilTalkEnd or autoPass, then we can't have lineToBreak.
                if (!autoPass) {
                    m_TrackBinding.lineToBreak = lineToStart;
                }else{
                    m_TrackBinding.lineToBreak = lineToBreak;
                }
                    m_TrackBinding.enablePass = false;
			} else {
				m_TrackBinding.enablePass = true;
				m_TrackBinding.lineToBreak = lineToBreak;
			}
			m_TrackBinding.lineToStart = lineToStart;
			m_TrackBinding.textSpeed = textSpeed;
			m_TrackBinding.enableQuickSkip = false;

			m_FirstFrameHappened = true;
			m_TrackBinding.NewTalk ();

            startTime = new double[m_TrackBinding.rpgtalkElements.Count];
            for(int i = 0; i < m_TrackBinding.rpgtalkElements.Count; i++)
            {
                if(i == 0)
                {
                    startTime[i] = 0;
                }
                else
                {
                    startTime[i] = (m_TrackBinding.rpgtalkElements[i-1].dialogText.Length
                / m_TrackBinding.actualTextSpeed) + secondsAutoPass;
                }

            }


            //Put an event for the end of the talk
            m_TrackBinding.OnEndTalk += OnEndTalk;

			if (m_TrackBinding.GetComponent<RPGTalkTimeline> ()) {
				rpgTime = m_TrackBinding.GetComponent<RPGTalkTimeline> ();
			}

            playableObj = playable;

		}

	}

	public override void OnBehaviourPause (Playable playable, FrameData info)
	{
		if (!m_TrackBinding)
			return;

		if (m_FirstFrameHappened) {
			//Let's make everything go back to the defaults
			ReturnDefaults ();
		}
	}

	public override void OnGraphStop (Playable playable)
	{
		if (!m_TrackBinding)
			return;

		if (m_FirstFrameHappened) {
			//Let's make everything go back to the defaults
			ReturnDefaults ();
		}
	}

	void ReturnDefaults(){
		m_TrackBinding.txtToParse = m_txtToParse;
		m_TrackBinding.lineToBreak = m_lineToBreak;
		m_TrackBinding.lineToStart = m_lineToStart;
		m_TrackBinding.textSpeed = m_textSpeed;
		m_TrackBinding.enableQuickSkip = m_enableQuickSkip;
		m_TrackBinding.enablePass = m_enablePass;

		m_FirstFrameHappened = false;
		m_TrackBinding.EndTalk();
		m_TrackBinding.OnEndTalk -= OnEndTalk;

		rpgTime = null;
	}

	void OnEndTalk(){
		//When a talk has finished, should we resume the timeline?
		if(pauseUntilTalkEnd){
			if (rpgTime) {
				rpgTime.Resume ();
			}
		}
	}

}
