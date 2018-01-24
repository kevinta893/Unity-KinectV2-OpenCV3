# Unity+KinectV2+OpenCV3

A package that includes both Microsoft Kinect V2 and OpenCV 3 (using OpenCVSharp). Requires Unity 2017 or higher.
 Thus requires the use of a *mcs.rsp* file to allow the use of ```extern alias```. 

## Installation
Simply drag the .unitypackage into your project and you're mostly ready to go. You will need to configure your build settings.

You will need to setup the following for Unity in the player settings:

Player settings:
* *Resolution and Presentation > Run In Background* should be set to **true**
* *File > Build Settings... > Player Settings... > Other Settings > Configuration> API Compatibility Level* should be set to **.NET 2.0**

## How to Use
See the Assets/Scene/Kinect+OpenCV+Demo scene on how to use. The KinectManager gameobject manages information being passed by the Windows DLLs and Unity. KinectView contains gameobjects that translate information by the KinectManagers into graphical elements.

### Body Tracking
Body tracking information can be fetched from the **BodyView** script which is usually attached to the KinectManager. Register your game object for events concerning the detection of a  Body detected by Kinect. The script emits events for when a new body is detected or destroyed. Since it uses Unity's SendMessage() functionality, implement the *both* of the following signatures in your script:

Bodies are only instantiated when they are detected. Listen to the following events when a body is found/lost:
```C#

void Kinect_BodyFound(object args)
{
	BodyGameObject bodyFound = (BodyGameObject) args;
}

void Kinect_BodyLost(object args)
{
	ulong bodyDeletedId = (ulong) args;
}
```

Joints on the body only have their positions updated in real time. To get positions of the body you will need to use the transform.localPosition properties of each joint. You can get the positions of the bodies as follows:

```C#
BodyGameObject bodyFound = (BodyGameObject) args;
GameObject thumbRight = bodyFound.GetJoint(Windows.Kinect.JointType.ThumbRight);

Vector3 thumbPosition = thumbRight.transform.localPosition;
```

### OpenCV 3
Using OpenCVSharp, it is possible to work with the Kinect's camera streams using OpenCV 3. Note that you will want to use **LateUpdate()** function instead of Update() to allow the Kinect to fully collect and create the textures before debugging. 

See also the **KinectCVUtilities** script for a bunch of useful functions for working with OpenCV and Kinect in Unity. Examples of how to use these functions are in the examples.

Included OpenCV examples are:
* Blob tracking in Infrared stream, use a retroreflective material (e.g. mocap) to have that be tracked by the Kinect
* Face Detection in Color stream, slower than what the Kinect provides, but may be of use

Enable the game objects for these demos as necessary or remove them entirely to use the scenes as a template.


### Troubleshooting

Some potential errors:
* OpenCVSharp tends to use some system libraries that other libraries tend to use commonly. Thus function names will clash. To resolve this, see [extern alias](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/extern-alias) keyword. In Unity, you will also need to make a file in the root directory called [mcs.rsp](https://docs.unity3d.com/Manual/PlatformDependentCompilation.html).





## Libraries Used
* Microsoft's KinectForWindows_UnityPro_2.0.1410
* [shimat/opencvsharp](https://github.com/shimat/opencvsharp)
* Demo code derived from: [VahidN/OpenCVSharp-Samples](https://github.com/VahidN/OpenCVSharp-Samples)

### License
An alternate license has been used to comply with OpenCVSharp's license to distribute the required DLLs. See License.txt in this folder. This license is specifically only for this project .