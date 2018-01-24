using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;


public class BodyGameObject
{

    private ulong id;
    private GameObject parent;
    private Dictionary<Kinect.JointType, GameObject> joints;

    public ulong ID
    {
        get { return this.id; }
        private set { this.id = value; }
    }


    public BodyGameObject(GameObject parent, ulong id)
    {
        this.id = id;
        this.parent = parent;
        this.joints = new Dictionary<Kinect.JointType, GameObject>();

        //get all joints as game objects, add to dictionary
        joints.Add(Kinect.JointType.AnkleLeft, GetJointFromGameObject(Kinect.JointType.AnkleLeft));
        joints.Add(Kinect.JointType.AnkleRight, GetJointFromGameObject(Kinect.JointType.AnkleRight));

        joints.Add(Kinect.JointType.ElbowLeft, GetJointFromGameObject(Kinect.JointType.ElbowLeft));
        joints.Add(Kinect.JointType.ElbowRight, GetJointFromGameObject(Kinect.JointType.ElbowRight));

        joints.Add(Kinect.JointType.FootLeft, GetJointFromGameObject(Kinect.JointType.FootLeft));
        joints.Add(Kinect.JointType.FootRight, GetJointFromGameObject(Kinect.JointType.FootRight));

        joints.Add(Kinect.JointType.HandLeft, GetJointFromGameObject(Kinect.JointType.HandLeft));
        joints.Add(Kinect.JointType.HandRight, GetJointFromGameObject(Kinect.JointType.HandRight));
        joints.Add(Kinect.JointType.HandTipLeft, GetJointFromGameObject(Kinect.JointType.HandTipLeft));
        joints.Add(Kinect.JointType.HandTipRight, GetJointFromGameObject(Kinect.JointType.HandTipRight));

        joints.Add(Kinect.JointType.Head, GetJointFromGameObject(Kinect.JointType.Head));

        joints.Add(Kinect.JointType.HipLeft, GetJointFromGameObject(Kinect.JointType.HipLeft));
        joints.Add(Kinect.JointType.HipRight, GetJointFromGameObject(Kinect.JointType.HipRight));

        joints.Add(Kinect.JointType.KneeLeft, GetJointFromGameObject(Kinect.JointType.KneeLeft));
        joints.Add(Kinect.JointType.KneeRight, GetJointFromGameObject(Kinect.JointType.KneeRight));

        joints.Add(Kinect.JointType.Neck, GetJointFromGameObject(Kinect.JointType.Neck));

        joints.Add(Kinect.JointType.ShoulderLeft, GetJointFromGameObject(Kinect.JointType.ShoulderLeft));
        joints.Add(Kinect.JointType.ShoulderRight, GetJointFromGameObject(Kinect.JointType.ShoulderRight));

        joints.Add(Kinect.JointType.SpineBase, GetJointFromGameObject(Kinect.JointType.SpineBase));
        joints.Add(Kinect.JointType.SpineMid, GetJointFromGameObject(Kinect.JointType.SpineMid));
        joints.Add(Kinect.JointType.SpineShoulder, GetJointFromGameObject(Kinect.JointType.SpineShoulder));

        joints.Add(Kinect.JointType.ThumbLeft, GetJointFromGameObject(Kinect.JointType.ThumbLeft));
        joints.Add(Kinect.JointType.ThumbRight, GetJointFromGameObject(Kinect.JointType.ThumbRight));

        joints.Add(Kinect.JointType.WristLeft, GetJointFromGameObject(Kinect.JointType.WristLeft));
        joints.Add(Kinect.JointType.WristRight, GetJointFromGameObject(Kinect.JointType.WristRight));
    }

    private GameObject GetJointFromGameObject(Kinect.JointType jType)
    {
        return parent.transform.Find(jType.ToString()).gameObject;
    }

    public GameObject GetJoint(Kinect.JointType jType)
    {
        return joints[jType];
    }

    public GameObject GetBodyParent()
    {
        return parent;
    }
}

