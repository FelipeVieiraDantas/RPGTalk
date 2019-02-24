using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using RPGTALK.Timeline;

namespace RPGTALK.Snippets
{
    [AddComponentMenu("Seize Studios/RPGTalk/Snippets/Skip Cutscene")]
    public class RPGTalkSkipCutscene : MonoBehaviour
    {
        public KeyCode keyToSkip = KeyCode.None;
        public string buttonToSkip = "";
        public bool skipWithMouse;
        public bool needToSkipTwice;
        public float timeBetweenSkips;
        public UnityEvent OnFirstTwiceSkip;
        public UnityEvent OnCancelTwiceSkip;
        public UnityEvent OnSkip;
        public bool canSkip = true;
        public bool jumpQuestions;
        public float delaySkip;

        RPGTalk rpgtalk;
        bool isTalking;
        float timingSkip;
        bool delaying;
        RPGTalkTimeline timeline;

        private void Start()
        {
            rpgtalk = GetComponent<RPGTalk>();
            timeline = GetComponent<RPGTalkTimeline>();
            rpgtalk.OnNewTalk += OnTalkStart;
            rpgtalk.OnEndTalk += OnTalkFinish;
        }

        private void Update()
        {
            if (isTalking && canSkip && !delaying)
            {
                if ((keyToSkip != KeyCode.None && Input.GetKeyDown(keyToSkip)) ||
                (buttonToSkip != "" && Input.GetButtonDown(buttonToSkip)) ||
                    (skipWithMouse && Input.GetMouseButtonDown(0)))
                {
                    Skip();
                }

                timingSkip -= Time.deltaTime;
                if (timingSkip <= 0)
                {
                    OnCancelTwiceSkip.Invoke();
                }
            }
        }

        public void Skip()
        {
            if (needToSkipTwice && timingSkip <= 0)
            {
                timingSkip = timeBetweenSkips;
                OnFirstTwiceSkip.Invoke();
                return;
            }

            OnSkip.Invoke();
            delaying = true;
            Invoke("ActuallySkip", delaySkip);
        }

        void ActuallySkip()
        {
            delaying = false;

            if (timeline != null)
            {
                timeline.Skip(jumpQuestions);
            }
            else
            {
                rpgtalk.EndTalk(jumpQuestions);
            }

        }

        void OnTalkStart()
        {

            isTalking = true;
        }

        void OnTalkFinish()
        {
            isTalking = false;
            if (timingSkip > 0)
            {
                timingSkip = 0;
                OnCancelTwiceSkip.Invoke();
            }
        }

    }
}