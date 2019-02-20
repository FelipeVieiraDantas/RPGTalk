using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
