using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using Windows.Kinect;

// This demonstrates the OpenCV version of the face tracking Algorithm known as the Haar cascade.
// Not to be confused with Kinect's built in Face tracking API
public class FaceTrackerOpenCVDemo : MonoBehaviour {

    public KinectManager kinectManager;

    // Use this for initialization
    void Start () {
		
	}
	
	// Must be called after KinectManager's update() function
	void LateUpdate () {
        //demo code, comment out or remove as necessary
        //Demo code and many more samples for OpenCVSharp can be found at: https://github.com/VahidN/OpenCVSharp-Samples
        DemoFaceTrack();
    }

    private CascadeClassifier cascade = new CascadeClassifier(@"Assets\OpenCVSharp-v3.2\Data\haarcascade_frontalface_alt.xml");
    private CascadeClassifier nestedCascade = new CascadeClassifier(@"Assets\OpenCVSharp-v3.2\Data\haarcascade_eye_tree_eyeglasses.xml");

    private void DemoFaceTrack()
    {
        int ColorWidth = kinectManager.ColorWidth;
        int ColorHeight = kinectManager.ColorHeight;

        Mat colorImage = new Mat(kinectManager.ColorHeight, ColorWidth, MatType.CV_8UC4, kinectManager.ColorRawData);              //rows=height, cols=width
        Mat grayImage = new Mat();
        Cv2.CvtColor(colorImage, grayImage, ColorConversionCodes.RGBA2GRAY);
        Cv2.EqualizeHist(grayImage, grayImage);

        OpenCvSharp.Rect[] faces = cascade.DetectMultiScale(
               image: grayImage,
               scaleFactor: 1.1,
               minNeighbors: 2,
               flags: HaarDetectionType.DoRoughSearch | HaarDetectionType.ScaleImage,
               minSize: new Size(30, 30)
               );


        for (int i = 0; i < faces.Length; i++)
        {
            OpenCvSharp.Rect faceRect = faces[i];

            //outline overall face in image
            var rndColor = Scalar.FromRgb(
                UnityEngine.Random.Range(0, 255),
                UnityEngine.Random.Range(0, 255),
                UnityEngine.Random.Range(0, 255)
                );
            Cv2.Rectangle(colorImage, faceRect, rndColor, 3);

            //now do nested features like the eyes
            Mat subFaceImage = new Mat(colorImage, faceRect);
            Mat detectedFaceGrayImage = new Mat();
            Cv2.CvtColor(subFaceImage, detectedFaceGrayImage, ColorConversionCodes.RGBA2GRAY);
            var nestedObjects = nestedCascade.DetectMultiScale(
                image: detectedFaceGrayImage,
                scaleFactor: 1.1,
                minNeighbors: 2,
                flags: HaarDetectionType.DoRoughSearch | HaarDetectionType.ScaleImage,
                minSize: new Size(30, 30)
                );

            //display each nested face feature
            foreach (var nestedObject in nestedObjects)
            {
                var center = new Point
                {
                    X = (int)(Math.Round(nestedObject.X + nestedObject.Width * 0.5, MidpointRounding.ToEven) + faceRect.Left),
                    Y = (int)(Math.Round(nestedObject.Y + nestedObject.Height * 0.5, MidpointRounding.ToEven) + faceRect.Top)
                };
                var radius = Math.Round((nestedObject.Width + nestedObject.Height) * 0.25, MidpointRounding.ToEven);
                Cv2.Circle(colorImage, center, (int)radius, rndColor, thickness: 3);
            }

        }


        //Convert back to RGBA32
        //Mat faceImageOut = new Mat(IRWidth, IRHeight, MatType.CV_8UC4);
        //Cv2.CvtColor(colorImage, faceImageOut, ColorConversionCodes.RGBA2RGBA);

        //load onto texture
        byte[] rawTextureBytes = KinectCVUtilities.ConvertMatToBytes(colorImage);
        kinectManager.ColorTexture.LoadRawTextureData(rawTextureBytes);
        kinectManager.ColorTexture.Apply();
    }
}
