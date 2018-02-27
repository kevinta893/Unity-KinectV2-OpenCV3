using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyTemplate : MonoBehaviour {

    private List<BodyGameObject> bodies = new List<BodyGameObject>();
    private MeshRenderer mesh;

	void Start () {
        mesh = GetComponent<MeshRenderer>();
        mesh.material.color = new Color(1.0f, 0.0f, 1.0f);

    }


    //wait for KinectManager to completely update first
    void LateUpdate () {
        //TODO Your code here
        if (bodies.Count > 0)
        {
            //some bodies, send orientation update
            GameObject thumbRight = bodies[0].GetJoint(Windows.Kinect.JointType.ThumbRight);
            GameObject handRight = bodies[0].GetJoint(Windows.Kinect.JointType.HandRight);
            GameObject handTipRight = bodies[0].GetJoint(Windows.Kinect.JointType.HandTipRight);

            float wristRotation = KinectCVUtilities.VerticalWristRotation(
                handTipRight.transform.localPosition,
                handRight.transform.localPosition,
                thumbRight.transform.localPosition
                );

            //update the rotation
            this.transform.rotation = Quaternion.Euler(0, wristRotation, 0);

        }
    }




    void Kinect_BodyFound(object args)
    {
        BodyGameObject bodyFound = (BodyGameObject) args;
        bodies.Add(bodyFound);
    }

    void Kinect_BodyLost(object args)
    {
        ulong bodyDeletedId = (ulong) args;

        lock (bodies){
            foreach (BodyGameObject bg in bodies)
            {
                if (bg.ID == bodyDeletedId)
                {
                    bodies.Remove(bg);
                    return;
                }
            }
        }
    }
}
