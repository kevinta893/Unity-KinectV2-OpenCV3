using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using Windows.Kinect;
using System;
using System.Runtime.InteropServices;
using System.IO;

/// <summary>
/// Use this script for programming advanced sensor based computer visioning.
/// </summary>
public class KinectManager : MonoBehaviour {




    //Data
    public int ColorWidth { get; private set; }
    public int ColorHeight { get; private set; }

    public int DepthWidth { get; private set; }
    public int DepthHeight { get; private set; }

    public int IRWidth { get; private set; }
    public int IRHeight { get; private set; }

    //Kinect sensor
    private KinectSensor _Sensor;
    private MultiSourceFrameReader _Reader;

    //color data
    private Texture2D _ColorTexture;
    private byte[] _ColorRawData;

    //depth data
    private ushort[] _DepthData;

    //ir Data
    private ushort[] _IRData;
    private byte[] _IRRawData;
    private Texture2D _IRTexture;

    //body data
    private Body[] _BodyData = null;

    // Use this for initialization
    void Start() {

        _Sensor = KinectSensor.GetDefault();
        if (_Sensor != null)
        {
            _Reader = _Sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);

            //color
            FrameDescription colorFrameDesc = _Sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            ColorWidth = colorFrameDesc.Width;
            ColorHeight = colorFrameDesc.Height;
            _ColorTexture = new Texture2D(colorFrameDesc.Width, colorFrameDesc.Height, TextureFormat.RGBA32, false);
            _ColorRawData = new byte[colorFrameDesc.BytesPerPixel * colorFrameDesc.LengthInPixels];


            //depth
            FrameDescription depthFrameDesc = _Sensor.DepthFrameSource.FrameDescription;
            DepthWidth = depthFrameDesc.Width;
            DepthHeight = depthFrameDesc.Height;

            _DepthData = new ushort[depthFrameDesc.LengthInPixels];


            //ir
            FrameDescription irFrameDesc = _Sensor.InfraredFrameSource.FrameDescription;
            IRWidth = irFrameDesc.Width;
            IRHeight = irFrameDesc.Height;

            _IRData = new ushort[irFrameDesc.LengthInPixels];
            _IRRawData = new byte[irFrameDesc.LengthInPixels * 4];
            _IRTexture = new Texture2D(irFrameDesc.Width, irFrameDesc.Height, TextureFormat.RGBA32, false);
            Debug.Log(IRWidth + "  " + IRHeight);


            //body
            BodyFrameReader bodyDesc = _Sensor.BodyFrameSource.OpenReader();

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }

        }
        else
        {
            Debug.LogError("Error! Kinect Sensor could not be started.");
            OnApplicationQuit();        //clear and close the kinect stream if possible.
        }
    }


    void Update() {
        UpdateKinect();                         //leave here, updates kinect sensor data for Unity

    }


    void UpdateKinect()
    {
        if (_Reader != null)
        {
            MultiSourceFrame frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {

                //color processing with depth
                ColorFrame colorFrame = frame.ColorFrameReference.AcquireFrame();
                if (colorFrame != null)
                {
                    DepthFrame depthFrame = frame.DepthFrameReference.AcquireFrame();
                    if (depthFrame != null)
                    {
                        colorFrame.CopyConvertedFrameDataToArray(_ColorRawData, ColorImageFormat.Rgba);
                        _ColorTexture.LoadRawTextureData(_ColorRawData);
                        _ColorTexture.Apply();

                        depthFrame.CopyFrameDataToArray(_DepthData);

                        depthFrame.Dispose();
                        depthFrame = null;
                    }

                    colorFrame.Dispose();
                    colorFrame = null;

                }

                //ir processing
                InfraredFrame irFrame = frame.InfraredFrameReference.AcquireFrame();
                if (irFrame != null)
                {
                    irFrame.CopyFrameDataToArray(_IRData);

                    int index = 0;
                    foreach (ushort ir in _IRData)
                    {
                        byte intensity = (byte)(ir >> 8);
                        _IRRawData[index++] = intensity;
                        _IRRawData[index++] = intensity;
                        _IRRawData[index++] = intensity;
                        _IRRawData[index++] = 255; // Alpha
                    }

                    //load raw data
                    _IRTexture.LoadRawTextureData(_IRRawData);
                    _IRTexture.Apply();

                    irFrame.Dispose();
                    irFrame = null;
                }


                //body processing
                BodyFrame bodyFrame = frame.BodyFrameReference.AcquireFrame();
                if (bodyFrame != null)
                {
                    if (_BodyData == null)
                    {
                        _BodyData = new Body[_Sensor.BodyFrameSource.BodyCount];
                    }
                    bodyFrame.GetAndRefreshBodyData(_BodyData);

                    bodyFrame.Dispose();
                    bodyFrame = null;

                }
                frame = null;
            }
        }
    }






    #region Public accessor functions

    public KinectSensor Sensor
    {
        get
        {
            return this._Sensor;
        }
    }


    public Texture2D ColorTexture
    {
        get
        {
            return this._ColorTexture;
        }
    }


    //TextureFormat.RGBA32, 8-bit 4 channel
    public byte[] ColorRawData
    {
        get
        {
            return this._ColorRawData;
        }
    }

    
    public ushort[] DepthData
    {
        get
        {
            return this._DepthData;
        }
    }

    //Expects a point in the depth/ir space
    public ushort GetDepth(Vector2 pt)
    {
        int index = (int) ((pt.y * DepthWidth) + pt.x);
        return _DepthData[index];
    }

    //expects a point in the depth/irspace
    public ushort GetDepth(int x, int y)
    {
        return _DepthData[(y * DepthWidth) + x];
    }

    public Texture2D IRTexture
    {
        get
        {
            return this._IRTexture;
        }
    }

    //TextureFormat.RGBA32, 8-bit 4 channel
    public byte[] IRRawData
    {
        get
        {
            return this._IRRawData;
        }
    }

    public Body[] BodyData
    {
        get
        {
            return this._BodyData;
        }
    }

    #endregion

    //==========================
    //Turns off Kinect sensor when application closed
    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }

            _Sensor = null;
        }
    }
}
