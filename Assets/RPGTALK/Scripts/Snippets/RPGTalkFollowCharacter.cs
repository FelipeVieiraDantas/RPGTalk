using System.Collections;
using UnityEngine;

namespace RPGTALK.Snippets
{
    [AddComponentMenu("Seize Studios/RPGTalk/Snippets/Follow Character")]
    public class RPGTalkFollowCharacter : MonoBehaviour
    {
        [Header("Set ups")]
        public Canvas canvas;
        public RectTransform dialogWindow;

        [Header("Camera Specs")]
        public bool rotateToTarget;
        public bool cameraBillboard;
        public Camera basedOnWhatCamera;

        [Header("Limits")]
        public bool containInsideScreen;
        public float unitsToMoveWhenOutside = 1;
        public bool maximumIsInitialPoint = true;
        public bool mantainZ = true;

        [Header("Pointer")]
        public RectTransform pointer;
        public Vector3 pointerInitialOffset;
        public bool mantainXDistanceFromClosestCorner;
        int closestCorner;

        RPGTalk rpgTalk;

        Vector3 initialPoint;
        bool doneFirst;

        RPGTalkSmartPointer smartPointer;
        Vector3 pointerPos;


        bool following;

        // Start is called before the first frame update
        void Awake()
        {
            rpgTalk = GetComponent<RPGTalk>();
            rpgTalk.OnNewTalk += BeginMove;
            rpgTalk.OnEndTalk += EndMove;
            if (pointer != null)
            {
                smartPointer = pointer.GetComponent<RPGTalkSmartPointer>();
            }
        }

        void BeginMove()
        {
            following = true;
        }

        void EndMove()
        {
            following = false;
            doneFirst = false;
        }

        private void Update()
        {
            if (!following || rpgTalk.following == null)
            {
                return;
            }

            //Make sure that anything that should follow is following and is billboarding
            Vector3 newPos = rpgTalk.following.position + rpgTalk.followingOffset;
            //If we mantain the z, we will keep the previous Z of the dialogWindow. Perfect for 2D projects.
            if (mantainZ)
            {
                newPos.z = dialogWindow.transform.position.z;
            }

            //If our canvas was overlay, we want the pos to be screen based
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                newPos = basedOnWhatCamera.WorldToScreenPoint(newPos);
            }


            Quaternion newRotation = Quaternion.identity;
            if (rotateToTarget)
            {
                newRotation = rpgTalk.following.rotation;
            }
            if (cameraBillboard)
            {
                newRotation = basedOnWhatCamera.transform.rotation;
            }

            dialogWindow.transform.position = newPos;
            dialogWindow.transform.rotation = newRotation;

            //If the pointer was a smart pointer, we want to set the end of it to our following obj.
            if (smartPointer != null)
            {
                //If our canvas was overlay, we want the pos to be screen based
                if (canvas == null || canvas.renderMode == RenderMode.WorldSpace)
                {
                    smartPointer.endPoint = rpgTalk.following.position;
                }
            }



            //Get the corners of the recttransform
            Vector3[] corners = new Vector3[4];
            dialogWindow.GetWorldCorners(corners);

            //Settings for the first time we run
            if (!doneFirst)
            {
                //doneFirst = true;
                initialPoint = dialogWindow.transform.position;
                //If we got a pointer, let's teleport it to the initial position
                if (pointer != null)
                {
                    pointerPos = initialPoint;
                    pointerPos.y = corners[0].y;
                    //If we want to mantain X distance, let's find out what is the closest corner
                    if (mantainXDistanceFromClosestCorner)
                    {
                        closestCorner = -1;
                        for (int i = 0; i < corners.Length; i++)
                        {

                            //Vector3.distance is heavy. Changing it to sqrMagnitude
                            if (closestCorner == -1 || (pointer.position - corners[i]).sqrMagnitude < (pointer.position - corners[closestCorner]).sqrMagnitude)
                            {
                                closestCorner = i;
                            }

                        }
                        pointerPos.x = corners[closestCorner].x;
                    }
                    //If our canvas was overlay, we want the pos to be screen based
                    if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        pointerPos = basedOnWhatCamera.ScreenToWorldPoint(pointerPos);
                    }
                    pointer.position = pointerPos + pointerInitialOffset;



                }
                //If our canvas was overlay, we want the pos to be screen based
                if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    pointerPos = basedOnWhatCamera.ScreenToWorldPoint(pointerPos);
                    initialPoint = basedOnWhatCamera.ScreenToWorldPoint(initialPoint);
                }
            }


            //If our canvas was overlay, we want the pos to be screen based
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {

                if (smartPointer != null && pointer.parent == dialogWindow.transform)
                {

                    pointerPos = dialogWindow.transform.position;
                    pointerPos.y = corners[0].y;

                    if (mantainXDistanceFromClosestCorner)
                    {
                        pointerPos.x = corners[closestCorner].x;
                    }


                    Debug.Log(pointerPos);
                    Vector3 followingPoint = rpgTalk.following.position + rpgTalk.followingOffset;
                    Vector3 followingPointScreen = basedOnWhatCamera.WorldToScreenPoint(followingPoint);
                    Debug.Log(followingPointScreen);
                    Vector3 directionBetweenPoints = (pointerPos - dialogWindow.position).normalized;

                    Vector3 appliedOffset = pointerInitialOffset;
                    appliedOffset.x *= directionBetweenPoints.x;
                    appliedOffset.y *= directionBetweenPoints.y;
                    appliedOffset.z *= directionBetweenPoints.z;


                    Vector3 cornerOnWorld = basedOnWhatCamera.ScreenToWorldPoint(pointerPos + appliedOffset);
                    Vector3 followOnWorld = basedOnWhatCamera.ScreenToWorldPoint(followingPointScreen);

                    Debug.Log(cornerOnWorld);

                    pointer.position = cornerOnWorld;
                    smartPointer.endPoint = followOnWorld;


                }
            }


            if (containInsideScreen)
            {


                //If our canvas was overlay, we want the pos to be screen based
                if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {


                    if (basedOnWhatCamera.ScreenToViewportPoint(corners[2]).x > 1 && basedOnWhatCamera.ScreenToViewportPoint(corners[0]).x < 0)
                    {
                        Debug.LogError("Your Canvas was out left and right at the same time! I don't know what to do!");
                        return;
                    }
                    if (basedOnWhatCamera.ScreenToViewportPoint(corners[1]).y > 1 && basedOnWhatCamera.ScreenToViewportPoint(corners[0]).y < 0)
                    {
                        Debug.LogError("Your Canvas was out top and bottom at the same time! I don't know what to do!");
                        return;
                    }


                    //if the left corners passed the view
                    while (basedOnWhatCamera.ScreenToViewportPoint(corners[0]).x < 0 &&
                    (!maximumIsInitialPoint || basedOnWhatCamera.ScreenToViewportPoint(corners[0]).x < basedOnWhatCamera.WorldToViewportPoint(initialPoint).x))
                    {
                        Vector3 newpos = dialogWindow.localPosition;
                        newpos.x += unitsToMoveWhenOutside;
                        dialogWindow.localPosition = newpos;
                        dialogWindow.GetWorldCorners(corners);
                    }
                    //if the right corners passed the view
                    while (basedOnWhatCamera.ScreenToViewportPoint(corners[2]).x > 1 &&
                    (!maximumIsInitialPoint || basedOnWhatCamera.ScreenToViewportPoint(corners[2]).x > basedOnWhatCamera.WorldToViewportPoint(initialPoint).x))
                    {
                        Vector3 newpos = dialogWindow.localPosition;
                        newpos.x -= unitsToMoveWhenOutside;
                        dialogWindow.localPosition = newpos;
                        dialogWindow.GetWorldCorners(corners);
                    }

                    //if the bottom corners passed the view
                    while (basedOnWhatCamera.ScreenToViewportPoint(corners[0]).y < 0 &&
                    (!maximumIsInitialPoint || basedOnWhatCamera.ScreenToViewportPoint(corners[0]).y < basedOnWhatCamera.WorldToViewportPoint(initialPoint).y))
                    {
                        Vector3 newpos = dialogWindow.localPosition;
                        newpos.y += unitsToMoveWhenOutside;
                        dialogWindow.localPosition = newpos;
                        dialogWindow.GetWorldCorners(corners);
                    }
                    //if the top corners passed the view
                    while (basedOnWhatCamera.ScreenToViewportPoint(corners[1]).y > 1 &&
                    (!maximumIsInitialPoint || basedOnWhatCamera.ScreenToViewportPoint(corners[1]).y > basedOnWhatCamera.WorldToViewportPoint(initialPoint).y))
                    {
                        Vector3 newpos = dialogWindow.localPosition;
                        newpos.y -= unitsToMoveWhenOutside;
                        dialogWindow.localPosition = newpos;
                        dialogWindow.GetWorldCorners(corners);
                    }


                }
                else
                {


                    if (basedOnWhatCamera.WorldToViewportPoint(corners[2]).x > 1 && basedOnWhatCamera.WorldToViewportPoint(corners[0]).x < 0)
                    {
                        Debug.LogError("Your Canvas was out left and right at the same time! I don't know what to do!");
                        return;
                    }
                    if (basedOnWhatCamera.WorldToViewportPoint(corners[1]).y > 1 && basedOnWhatCamera.WorldToViewportPoint(corners[0]).y < 0)
                    {
                        Debug.LogError("Your Canvas was out top and bottom at the same time! I don't know what to do!");
                        return;
                    }
                    //if the left corners passed the view
                    while (basedOnWhatCamera.WorldToViewportPoint(corners[0]).x < 0 &&
                    (!maximumIsInitialPoint || basedOnWhatCamera.WorldToViewportPoint(corners[0]).x < basedOnWhatCamera.WorldToViewportPoint(initialPoint).x))
                    {
                        Vector3 newpos = dialogWindow.localPosition;
                        newpos.x += unitsToMoveWhenOutside;
                        dialogWindow.localPosition = newpos;
                        dialogWindow.GetWorldCorners(corners);
                    }
                    //if the right corners passed the view
                    while (basedOnWhatCamera.WorldToViewportPoint(corners[2]).x > 1 &&
                    (!maximumIsInitialPoint || basedOnWhatCamera.WorldToViewportPoint(corners[2]).x > basedOnWhatCamera.WorldToViewportPoint(initialPoint).x))
                    {
                        Vector3 newpos = dialogWindow.localPosition;
                        newpos.x -= unitsToMoveWhenOutside;
                        dialogWindow.localPosition = newpos;
                        dialogWindow.GetWorldCorners(corners);
                    }

                    //if the bottom corners passed the view
                    while (basedOnWhatCamera.WorldToViewportPoint(corners[0]).y < 0 &&
                    (!maximumIsInitialPoint || basedOnWhatCamera.WorldToViewportPoint(corners[0]).y < basedOnWhatCamera.WorldToViewportPoint(initialPoint).y))
                    {
                        Vector3 newpos = dialogWindow.localPosition;
                        newpos.y += unitsToMoveWhenOutside;
                        dialogWindow.localPosition = newpos;
                        dialogWindow.GetWorldCorners(corners);
                    }
                    //if the top corners passed the view
                    while (basedOnWhatCamera.WorldToViewportPoint(corners[1]).y > 1 &&
                    (!maximumIsInitialPoint || basedOnWhatCamera.WorldToViewportPoint(corners[1]).y > basedOnWhatCamera.WorldToViewportPoint(initialPoint).y))
                    {
                        Vector3 newpos = dialogWindow.localPosition;
                        newpos.y -= unitsToMoveWhenOutside;
                        dialogWindow.localPosition = newpos;
                        dialogWindow.GetWorldCorners(corners);
                    }

                }
            }

        }
    }
}