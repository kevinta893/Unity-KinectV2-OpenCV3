using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using Windows.Kinect;
using System;
using System.Runtime.InteropServices;
using System.IO;


/// <summary>
/// Utilities for bridging Kinect, OpenCV and Unity together
/// </summary>
public static class KinectCVUtilities
{

    /// <summary>
    /// Gets an angle between 3 points that form a connection:  p1---p2---p3
    /// Such that the vector v = p3-p2 defines the angle around p1
    /// </summary>
    /// <param name="p1">A position in space</param>
    /// <param name="p2">A position in space</param>
    /// <param name="p3">A position in space</param>
    /// <returns></returns>
    public static float VerticalWristRotation(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 a = p2 - p1;
        Vector3 b = p2 - p3;
        a.Normalize();
        b.Normalize();
        Vector3 thetaVector = Vector3.ProjectOnPlane(b, a);
        thetaVector.Normalize();

        Vector3 rightOnPlane = Vector3.ProjectOnPlane(Vector3.right, a);
        return Vector3.SignedAngle(rightOnPlane, thetaVector, Vector3.up);
    }

    /// <summary>
    /// The centroid computed as according to: 
    /// https://en.wikipedia.org/wiki/Centroid#Centroid_of_a_polygon
    /// 
    /// If vertices.count == 1, returns that point
    /// If vertices.count == 2, returns the midpoint of the line drawn between the two points
    /// </summary>
    /// <param name="vertices"></param>
    /// <returns></returns>
    public static Point Centroid2D(Point[] vertices)
    {
        if (vertices.Length == 1)
        {
            return vertices[0];
        }
        else if (vertices.Length == 2)
        {
            Point p1 = vertices[0];
            Point p2 = vertices[1];

            return new Point((p1.X + p2.X) / 2.0, (p1.X + p2.X) / 2.0);
        }

        //compute the signed area, note that if the points are "numbered" in clockwise order, the area will be negative, but the centroid coordinates will still be correct.
        double signedArea = 0;
        for (int i = 0; i < vertices.Length; i++)
        {
            Point v_i = vertices[i % vertices.Length];
            Point v_i_1 = vertices[(i + 1) % vertices.Length];          //element i+1

            signedArea += (v_i.X * v_i_1.Y) - (v_i_1.X * v_i.Y);
        }
        //compute last case and outer product
        signedArea *= (1.0 / 2.0);




        //now compute Cx
        double cx = 0;
        for (int i = 0; i < vertices.Length; i++)
        {
            Point v_i = vertices[i % vertices.Length];
            Point v_i_1 = vertices[(i + 1) % vertices.Length];          //element i+1

            cx += (v_i.X + v_i_1.X) * ((v_i.X * v_i_1.Y) - (v_i_1.X * v_i.Y));
        }
        //compute last case and outer product
        cx *= (1.0 / (6.0 * signedArea));

        //now compute Cy
        double cy = 0;
        for (int i = 0; i < vertices.Length ; i++)
        {
            Point v_i = vertices[i % vertices.Length];
            Point v_i_1 = vertices[(i + 1) % vertices.Length];          //element i+1

            cy += (v_i.Y + v_i_1.Y) * ((v_i.X * v_i_1.Y) - (v_i_1.X * v_i.Y));
        }
        //compute last case and outer product
        cy *= (1.0 / (6.0 * signedArea));



        return new Point(cx, cy);
    }

    public static Point Centroid2D(List<Point> vertices)
    {
        Point[] arr = vertices.ToArray();
        return Centroid2D(arr);
    }

    /// <summary>
    /// Calculates the Perimeter given the set of ordered points, does not assume list is a convex hull
    /// If vertices.count == 1, perimeter is zero
    /// If vertices.count == 2, it is the length of the line drawn between the two points.
    /// </summary>
    /// <param name="vertices"></param>
    /// <returns></returns>
    public static double Perimeter(Point[] vertices)
    {
        if (vertices.Length == 2)
        {
            return Point.Distance(vertices[0], vertices[1]);
        }
        else if (vertices.Length < 2)
        {
            //NaN for vertices less than 2
            return Double.NaN;
        }

        double perimeter = 0;
        for (int i = 0; i < vertices.Length; i++)
        {
            perimeter += Point.Distance(vertices[i % vertices.Length], vertices[(i + 1) % vertices.Length]);            //ensure wrapback from last to first
        }

        return perimeter;
    }

    public static double Perimeter(List<Point> vertices)
    {
        Point[] arr = vertices.ToArray();
        return Perimeter(arr);
    }


    /// <summary>
    /// Gets the perimeter of the convex hull given by the list of points 
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    public static double PerimeterHull(Point[] points)
    {
        return Perimeter(Cv2.ConvexHull(points));
    }

    public static double PerimeterHull(List<Point> points)
    {
        return Perimeter(Cv2.ConvexHull(points));
    }




    #region Conversion Functions

    /// <summary>
    /// Useful for converting CV Mats to raw image bytes
    /// </summary>
    /// <param name="img"></param>
    /// <returns></returns>
    public static byte[] ConvertMatToBytes(OpenCvSharp.Mat img)
    {
        int renderSize = img.Width * img.Height * img.Channels();
        byte[] ret = new byte[renderSize];
        Marshal.Copy(img.Data, ret, 0, renderSize);
        return ret;
    }


    /// <summary>
    /// Transforms a pixel point from a 2D cartesian grid to a point defined by a 3D plane
    /// </summary>
    /// <param name="plane">The 3D plane to transform the point to (assumes center of model is origin)</param>
    /// <param name="texSize">The size of the texture for point that it is referring to</param>
    /// <param name="pt">The 2D pixel point to transform</param>
    /// <returns></returns>
    public static Vector3 TransformTextureToUnity(Transform plane, Vector2 texSize, Vector2 pt)
    {
        Vector2 worldCoord = new Vector2();
        worldCoord.x = pt.x;
        worldCoord.y = pt.y;


        Vector2 planePos = new Vector2(plane.position.x, plane.position.y);

        float irWidth = texSize.x;
        float irHeight = texSize.y;

        float planeWidth = plane.localScale.x;
        float planeHeight = plane.localScale.y;



        //scale the local pixel system to the unity world system.
        Vector2 scaleTransform = new Vector2(planeWidth / irWidth, planeHeight / irHeight);
        worldCoord = Vector2.Scale(worldCoord, scaleTransform);

        //invert the y since y0 starts from bottom up
        worldCoord.y = planeHeight - worldCoord.y;

        //transform to real world, the pixel point is in the unity world coord system
        worldCoord += planePos;

        //convert to plane's coordinate system, plane's have their origins (world position) start at the center of the object
        worldCoord.x -= planeWidth / 2;
        worldCoord.y -= planeHeight / 2;

        //now apply the rotation of the plane
        Vector3 worldPt = new Vector3(worldCoord.x, worldCoord.y, plane.position.z);
        worldPt -= plane.position;
        worldPt = plane.rotation * worldPt;
        worldPt += plane.position;

        return worldPt;
    }

    public static Vector2 TransformUnityToTexture(Transform plane, Vector2 texSize, Vector3 pt)
    {
        //detransform the coordinates using the plane
        Vector3 detransform = pt;
        //detransform
        detransform -= plane.position;
        detransform = Quaternion.Inverse(plane.rotation) * detransform;
        detransform = Vector3.Scale(new Vector3(1.0f / plane.localScale.x, 1.0f / plane.localScale.y, 1.0f / plane.localScale.z), detransform);      //remove plane scale


        detransform.x = detransform.x * texSize.x;
        detransform.y = detransform.y * texSize.y;


        //negate up and down
        detransform.y = texSize.y - detransform.y;

        //offset the coordinates since they refer an origin starting at the middle of the plane
        Vector2 planeOffset = new Vector2(texSize.x / 2.0f, texSize.y / 2.0f);
        planeOffset.x *= 1;
        planeOffset.y *= -1;
        detransform.x = detransform.x + planeOffset.x;
        detransform.y = detransform.y + planeOffset.y;

        return new Vector2(detransform.x, detransform.y);

    }

    /// <summary>
    /// Converts a list to points
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static List<Vector2> PointsToVector2(List<Point> list)
    {
        List<Vector2> ret = new List<Vector2>();
        foreach (Point p in list)
        {
            ret.Add(new Vector2(p.X, p.Y));
        }
        return ret;
    }

    /// <summary>
    /// Converts a list to points
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static Vector2[] PointsToVector2(Point[] list)
    {
        Vector2[] ret = new Vector2[list.Length];
        for(int i = 0; i < list.Length; i++)
        {
            ret[i] = new Vector2(list[i].X, list[i].Y);
        }
        return ret;
    }

    public static Vector2 PointToVector2(Point p)
    {
        return new Vector2(p.X, p.Y);
    }

    #endregion

    #region Debug Drawing functions
    //=================================================
    //Utility draw functions, dont forget to call Apply() on the texture when done.


    /// <summary>
    /// Draws a circle from center and radius. Expects points in the texture's coordinate system
    /// </summary>
    public static void DrawCircle(Texture2D tex, Vector2 pt, Color color, int radius)
    {
        int diameter = radius * 2;

        Vector2 center = new Vector2(diameter / 2, diameter / 2);
        int ptX = (int)pt.x;
        int ptY = (int)pt.y;


        for (int i = 0; i < diameter; i++)
        {
            for (int j = 0; j < diameter; j++)
            {
                Vector2 drawPt = new Vector2(i, j);

                if ((drawPt - center).sqrMagnitude <= (radius))
                {

                    tex.SetPixel(ptX + (i - radius), ptY + (j - radius), color);
                }
            }

        }


        tex.Apply();
    }

    /// <summary>
    /// Draws a line from start to end points. Expects points in the texture's coordinate system
    /// Source; http://wiki.unity3d.com/index.php?title=TextureDrawLine
    /// </summary>
    public static void DrawLine(Texture2D tex, Vector2 start, Vector2 end, Color color)
    {
        int x0 = (int)start.x;
        int y0 = (int)start.y;
        int x1 = (int)end.x;
        int y1 = (int)end.y;

        int dy = (int)(y1 - y0);
        int dx = (int)(x1 - x0);
        int stepx, stepy;

        if (dy < 0) { dy = -dy; stepy = -1; }
        else { stepy = 1; }
        if (dx < 0) { dx = -dx; stepx = -1; }
        else { stepx = 1; }
        dy <<= 1;
        dx <<= 1;

        float fraction = 0;

        tex.SetPixel(x0, y0, color);
        if (dx > dy)
        {
            fraction = dy - (dx >> 1);
            while (Mathf.Abs(x0 - x1) > 1)
            {
                if (fraction >= 0)
                {
                    y0 += stepy;
                    fraction -= dx;
                }
                x0 += stepx;
                fraction += dy;
                tex.SetPixel(x0, y0, color);
            }
        }
        else
        {
            fraction = dx - (dy >> 1);
            while (Mathf.Abs(y0 - y1) > 1)
            {
                if (fraction >= 0)
                {
                    x0 += stepx;
                    fraction -= dy;
                }
                y0 += stepy;
                fraction += dx;
                tex.SetPixel(x0, y0, color);
            }
        }

    }


    /// <summary>
    /// Draws a rectangle. An option to have it filled.
    /// </summary>
    /// <param name="tex"></param>
    /// <param name="topLeft"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="color"></param>
    /// <param name="filled"></param>
    public static void DrawRectangle(Texture2D tex, Vector2 topLeft, int width, int height, Color color, bool filled)
    {
        //Easy, draw the 4 sides
        DrawLine(tex, topLeft, new Vector2(topLeft.x + width, topLeft.y), color);
        DrawLine(tex, topLeft, new Vector2(topLeft.x, topLeft.y + height), color);

        Vector2 bottomRight = new Vector2(topLeft.x + width, topLeft.y + height);
        DrawLine(tex, bottomRight, new Vector2(bottomRight.x - width, bottomRight.y), color);
        DrawLine(tex, bottomRight, new Vector2(bottomRight.x, bottomRight.y - height), color);


        //fill the rest of the rectangle
        if (filled)
        {
            int topLeftX = (int)topLeft.x;
            int topLeftY = (int)topLeft.y;

            //fill
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    tex.SetPixel(topLeftX + i, topLeftY + j, color);
                }
            }
        }
    }


    /// <summary>
    /// Draws a cross (+ symbol) at the specified point. Size determines the length of the arms,
    /// Expects size ("diameter") to be odd integer, otherwise bumps up to odd
    /// 
    /// </summary>
    /// <param name="tex"></param>
    /// <param name="center"></param>
    /// <param name="color"></param>
    /// <param name="size"></param>
    public static void DrawCross(Texture2D tex, Vector2 center, Color color, int size)
    {
        //if even, turn to odd to make pretty cross
        if (size % 2 == 0)
        {
            size = size + 1;
        }

        float radius = (size - 1) / 2.0f;

        Vector2 vTop = new Vector2(center.x, center.y - radius);
        Vector2 vBottom = new Vector2(center.x, center.y + radius + 1);
        Vector2 hLeft = new Vector2(center.x - radius, center.y);
        Vector2 hRight = new Vector2(center.x + radius + 1, center.y);

        DrawLine(tex, vTop, vBottom, color);
        DrawLine(tex, hLeft, hRight, color);
    }

    /// <summary>
    /// Draws a vector starting at point 'pt' to the length of 'raduis' at angle 'degrees'
    /// </summary>
    /// <param name="tex"></param>
    /// <param name="pt"></param>
    /// <param name="degrees"></param>
    /// <param name="color"></param>
    /// <param name="radius"></param>
    public static void DrawVector(Texture2D tex, Vector2 pt, float degrees, Color color, int radius)
    {
        float radians = degrees * Mathf.Deg2Rad;
        Vector2 delta;
        delta.x = Mathf.Cos(radians) * radius;
        delta.y = Mathf.Sin(radians) * radius;

        DrawLine(tex, pt, pt + delta, color);
        DrawCircle(tex, pt + delta, color, 4);

    }

    /// <summary>
    /// Draws a vector starting at point 'pt' and the indicated direction vector. Can make the scale of the
    /// direction vector longer
    /// </summary>
    /// <param name="tex"></param>
    /// <param name="pt"></param>
    /// <param name="directionVector"></param>
    /// <param name="color"></param>
    /// <param name="scale"></param>
    public static void DrawVector(Texture2D tex, Vector2 pt, Vector2 directionVector, Color color, float scale)
    {
        Vector2 delta = directionVector * scale;

        DrawLine(tex, pt, pt + delta, color);
        DrawCircle(tex, pt + delta, color, 4);

    }

    #endregion

}
