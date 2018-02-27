using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class ColorView : MonoBehaviour
{
    public KinectManager colorSource;

    void Start()
    {
        gameObject.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
    }

    void Update()
    {
        if (colorSource == null)
        {
            return;
        }

        gameObject.GetComponent<Renderer>().material.mainTexture = colorSource.ColorTexture;
    }
}
