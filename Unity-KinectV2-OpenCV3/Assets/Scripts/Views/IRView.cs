using UnityEngine;
using System.Collections;

public class IRView : MonoBehaviour
{
    public KinectManager infraredSource;

    void Start()
    {
        gameObject.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
    }

    void Update()
    {
        if (infraredSource == null)
        {
            return;
        }

        gameObject.GetComponent<Renderer>().material.mainTexture = infraredSource.IRTexture;
    }
}
