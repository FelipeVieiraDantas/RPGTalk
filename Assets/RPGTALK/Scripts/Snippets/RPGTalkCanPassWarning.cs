using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RPGTALK.Snippets
{
    [AddComponentMenu("Seize Studios/RPGTalk/Snippets/Can Pass Warning")]
    public class RPGTalkCanPassWarning : MonoBehaviour
    {

        public UnityEvent OnCanPass, OnPassed;

        RPGTalk rpgtalk;

        // Start is called before the first frame update
        void Start()
        {
            rpgtalk = GetComponent<RPGTalk>();
            rpgtalk.OnEndAnimating += CanPass;
            rpgtalk.OnPlayNext += Passed;
            rpgtalk.OnEndTalk += Passed;
        }

        void CanPass()
        {
            if (rpgtalk.enablePass)
            {
                OnCanPass.Invoke();
            }
        }

        void Passed()
        {
            OnPassed.Invoke();
        }
    }
}