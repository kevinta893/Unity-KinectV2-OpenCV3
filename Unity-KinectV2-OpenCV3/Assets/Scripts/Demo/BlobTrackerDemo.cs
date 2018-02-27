using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using Windows.Kinect;



public class BlobTrackerDemo : MonoBehaviour {

    public KinectManager kinectManager;

    [Tooltip("Assumes Unity's 3D plane object")]
    public Transform irPlane;
    [Tooltip("Assumes Unity's 3D plane object")]
    public Transform colorPlane;

    public GameObject markerPrefab;

    public bool overrideIRTexture = true;

    [Header("Tracking Calibration")]
    [Tooltip("Use this to give an offset for the tracking when the tracking is slightly off due to position")]
    public Vector3 irOffset;
    [Tooltip("Use this to give an offset for the tracking when the tracking is slightly off due to position")]
    public Vector3 colorOffset = new Vector3(3.2f, 0,0);


    private GameObject irTrack;
    private GameObject colorTrack;

    // Use this for initialization
    void Start () {
        irTrack = Instantiate(markerPrefab);
        colorTrack = Instantiate(markerPrefab);
	}

    // Must be called after KinectManager's update() function
    void LateUpdate()
    {
        //demo code, comment out or remove as necessary
        //Demo code and many more samples for OpenCVSharp can be found at: https://github.com/VahidN/OpenCVSharp-Samples
        DemoIRBlobTrack();
    }



    private void DemoIRBlobTrack()
    {
        int IRWidth = kinectManager.IRWidth;
        int IRHeight = kinectManager.IRHeight;

        //get image and convert to threshold image
        Mat irImage = new Mat(IRHeight, IRWidth, MatType.CV_8UC4, kinectManager.IRRawData);              //rows=height, cols=width
        Mat ir8Bit = new Mat();
        Cv2.CvtColor(irImage, ir8Bit, ColorConversionCodes.RGBA2GRAY);
        Cv2.Threshold(ir8Bit, ir8Bit, thresh: 200, maxval: 255, type: ThresholdTypes.Binary);

        //Find blobs
        SimpleBlobDetector.Params detectorParams = new SimpleBlobDetector.Params
        {
            //MinDistBetweenBlobs = 10, // 10 pixels between blobs
            //MinRepeatability = 1,

            //MinThreshold = 100,
            //MaxThreshold = 255,
            //ThresholdStep = 5,

            FilterByArea = false,
            //FilterByArea = true,
            //MinArea = 0.001f, // 10 pixels squared
            //MaxArea = 500,

            FilterByCircularity = false,
            //FilterByCircularity = true,
            //MinCircularity = 0.001f,

            FilterByConvexity = false,
            //FilterByConvexity = true,
            //MinConvexity = 0.001f,
            //MaxConvexity = 10,

            FilterByInertia = false,
            //FilterByInertia = true,
            //MinInertiaRatio = 0.001f,

            FilterByColor = false
            //FilterByColor = true,
            //BlobColor = 255 // to extract light blobs
        };

        SimpleBlobDetector simpleBlobDetector = SimpleBlobDetector.Create(detectorParams);
        KeyPoint[] blobs = simpleBlobDetector.Detect(ir8Bit);


        foreach (KeyPoint kp in blobs)
        {
            
            Vector2 blobPt = new Vector2(kp.Pt.X, kp.Pt.Y);

            //transform ir point to unity world space
            Vector2 irDimensions = new Vector2(kinectManager.IRWidth, kinectManager.IRHeight);
            irTrack.transform.localPosition = KinectCVUtilities.TransformTextureToUnity(irPlane, irDimensions, blobPt) + irOffset;


            //transform ir point to color space, then world space
            DepthSpacePoint depthPt = new DepthSpacePoint();
            depthPt.X = blobPt.x;
            depthPt.Y = blobPt.y;
            double depth = GetAvg(kinectManager.DepthData, (int) depthPt.X, (int) depthPt.Y, kinectManager.DepthWidth, kinectManager.DepthHeight);
            ColorSpacePoint colorMappedPt = kinectManager.Sensor.CoordinateMapper.MapDepthPointToColorSpace(depthPt, (ushort) depth);

            Vector2 colorDimensions = new Vector2(kinectManager.ColorWidth, kinectManager.ColorHeight);
            Vector2 colorPt = new Vector2(colorMappedPt.X, colorMappedPt.Y);    
            colorTrack.transform.localPosition = KinectCVUtilities.TransformTextureToUnity(colorPlane, colorDimensions, colorPt) + colorOffset;
        }


        //convert back to unity texture, add nice debug drawings
        Mat irImageKeyPoints = new Mat();
        Cv2.DrawKeypoints(ir8Bit, blobs, irImageKeyPoints, color: Scalar.FromRgb(255, 0, 0),
                    flags: DrawMatchesFlags.DrawRichKeypoints);

        //Convert back to RGBA32
        Mat irImageOut = new Mat(IRWidth, IRHeight, MatType.CV_8UC4);
        Cv2.CvtColor(irImageKeyPoints, irImageOut, ColorConversionCodes.BGR2RGBA);      //OpenCV is weird and has it in BGR format

        //load onto texture
        byte[] rawTextureData = KinectCVUtilities.ConvertMatToBytes(irImageOut);

        if (overrideIRTexture)
        {
            kinectManager.IRTexture.LoadRawTextureData(rawTextureData);
            kinectManager.IRTexture.Apply();
        }

    }

    private double GetAvg(ushort[] depthData, int x, int y, int width, int height)
    {

        double sum = 0.0;

        for (int y1 = y; y1 < y + 4; y1++)
        {
            for (int x1 = x; x1 < x + 4; x1++)
            {
                int fullIndex = (y1 * width) + x1;
                fullIndex = Mathf.Clamp(fullIndex, 0, depthData.Length - 1);
                if (depthData[fullIndex] == 0)
                    sum += 4500;
                else
                    sum += depthData[fullIndex];

            }
        }

        return sum / 16;
    }

}
