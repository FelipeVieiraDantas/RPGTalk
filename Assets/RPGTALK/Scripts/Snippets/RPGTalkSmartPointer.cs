using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTALK.Snippets
{
    [AddComponentMenu("Seize Studios/RPGTalk/Snippets/Smart Pointer")]
    [ExecuteInEditMode]
    [RequireComponent(typeof(LineRenderer))]
    public class RPGTalkSmartPointer : MonoBehaviour
    {

        public Vector3 endPoint;
        public bool ignoreZFromEndPoint;
        LineRenderer line;
        public Vector3 endPointOffset;
        public float maximumStretch = 2;
        public Vector3 addOffsetAfterMaximumStretch;
        public float maximumStretchWithOffset = 10;

        // Use this for initialization
        void Start()
        {
            line = GetComponent<LineRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            if (line == null)
            {
                return;
            }

            line.SetPosition(0, transform.position);

            Vector3 newpos = endPoint;
            if (ignoreZFromEndPoint)
            {
                newpos.z = transform.position.z;
            }
            newpos += endPointOffset;
            //Vector3.distance is heavy. Changing it to sqrMagnitude
            if ((transform.position - newpos).sqrMagnitude > maximumStretch * maximumStretch)
            {
                float passedStretch = (maximumStretch * maximumStretch) - (transform.position - newpos).sqrMagnitude;
                Vector3 appliedOffset = addOffsetAfterMaximumStretch * passedStretch;
                newpos = transform.position + ((newpos - transform.position).normalized * maximumStretch);
                newpos = newpos + appliedOffset;

                if ((transform.position - newpos).sqrMagnitude > maximumStretchWithOffset * maximumStretchWithOffset)
                {
                    newpos = transform.position + ((newpos - transform.position).normalized * maximumStretchWithOffset);
                }
            }


            line.SetPosition(line.positionCount - 1, newpos);

        }

    }
}