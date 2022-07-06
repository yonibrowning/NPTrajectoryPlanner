using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using Kitware.VTK;

public class BrainWarper : MonoBehaviour
{

    vtkThinPlateSplineTransform forward_transform;
    vtkThinPlateSplineTransform inverse_transform;
    vtkPoints source_points;
    vtkPoints target_points;
    private string source_file = "C:\\Users\\yoni.browning\\OneDrive - Allen Institute\\Documents\\MRI\\MarScans\\Visualization\\source_landmarks.csv";
    private string target_file = "C:\\Users\\yoni.browning\\OneDrive - Allen Institute\\Documents\\MRI\\MarScans\\Visualization\\target_landmarks.csv";

    [SerializeField] Vector3 headframeOrigin;
    [SerializeField] float rx;
    [SerializeField] float ry;
    [SerializeField] float rz;


    // Start is called before the first frame update
    public void Awake()
    {
        Debug.Log("Awake!");

        source_points = ReadFile(source_file);
        target_points = ReadFile(target_file);
        Debug.Log("Points Loaded!");

        
        forward_transform = new vtkThinPlateSplineTransform();
        forward_transform.SetSourceLandmarks(source_points);
        forward_transform.SetTargetLandmarks(target_points);
        forward_transform.SetBasisToR();
        forward_transform.Update();
        Debug.Log("BuiltTransform");


        inverse_transform = new vtkThinPlateSplineTransform();
        inverse_transform.SetSourceLandmarks(target_points);
        inverse_transform.SetTargetLandmarks(source_points);
        inverse_transform.SetBasisToR();
        inverse_transform.Update();
        
        Debug.Log("We have a transform!");

        //Debug.Log(source_points.GetNumberOfPoints());
        //Debug.Log(target_points.GetNumberOfPoints());
        //Debug.Log(TransformPointForward(Vector3.one));
        // Debug.Log(TransformPointInverse(Vector3.one));


    }

    private vtkPoints ReadFile(string filename)
    {
        vtkPoints newPoints = new vtkPoints();

        StreamReader reader = new StreamReader(filename);

        string this_line = reader.ReadLine();

        while (reader.Peek() >= 0)
        {
            this_line = reader.ReadLine();
            string[] splt = this_line.Split(',');

            float x = float.Parse(splt[0]);
            float y = float.Parse(splt[1]);
            float z = float.Parse(splt[2]);
            newPoints.InsertNextPoint(x, y, z);
        }

        return newPoints;
    }

    private Mesh CopyAndTransformMesh(Mesh initialMesh, vtkThinPlateSplineTransform transform)
    {
        Mesh copyMesh = new Mesh();

        copyMesh.vertices = initialMesh.vertices;
        copyMesh.colors = initialMesh.colors;
        copyMesh.triangles = initialMesh.triangles;

        for (int ii = 0; ii < copyMesh.vertices.Length; ii++)
        {
            float[] inPoint = new float[3];

            Vector3 point = copyMesh.vertices[ii];

            double[] outPoint = transform.TransformPoint((double)point.x, (double)point.y, (double)point.z);
            copyMesh.vertices[ii] = new Vector3((float)outPoint[0], (float)outPoint[1], (float)outPoint[2]);
        }
        copyMesh.RecalculateBounds();
        copyMesh.RecalculateNormals();
        copyMesh.RecalculateTangents();
        return copyMesh;
    }

    public Mesh CopyAndTransformMeshForward(Mesh initialMesh)
    {
        return CopyAndTransformMesh(initialMesh, forward_transform);
    }

    public Mesh CopyAndTransformMeshInverse(Mesh initialMesh)
    {
        return CopyAndTransformMesh(initialMesh, inverse_transform);
    }

    public Vector3 TransformPointForward(Vector3 point)
    {
        point += headframeOrigin;
        double[] outPoint = forward_transform.TransformPoint((double)point.x, (double)point.y, (double)point.z);
        return new Vector3((float)outPoint[0], (float)outPoint[1], (float)outPoint[2]);

    }



    public Vector3 TransformPointInverse(Vector3 point)
    {
        /*
        double[] outPoint = inverse_transform.TransformPoint((double)point.x, (double)point.y, (double)point.z);
        return new Vector3((float)outPoint[0], (float)outPoint[1], (float)outPoint[2]);
        */
        double[] outPoint = inverse_transform.TransformPoint((double)point.x, (double)point.y, (double)point.z);
        Vector3 transVector = new Vector3((float)outPoint[0], (float)outPoint[1], (float)outPoint[2]);
        return transVector-headframeOrigin;
       // return Rotate(transVector - headframeOrigin, rx, ry, rz);

    }


    public List<Vector3> TransformPointsInverse(List<Vector3> points)
    {
        List<Vector3> returnMe = new List<Vector3>();
        for (int i = 0; i < points.Count; i++)
        {
            returnMe.Add(TransformPointInverse(points[i]));
        }
        return returnMe;
    }

    public Vector3[] TransformPointsInverse(Vector3[] points)
    {
        List<Vector3> returnMe = new List<Vector3>();
        for (int i = 0; i < points.Length; i++)
        {
            returnMe.Add(TransformPointInverse(points[i]));
        }
        return returnMe.ToArray();
    }

    private Vector3 RotateX(Vector3 rotateMe, float degree)
    {
        float rad = degree * (Mathf.PI / 180f);
        Vector3 row1 = new Vector3(1.0f, 0.0f, 0.0f);
        Vector3 row2 = new Vector3(0.0f, Mathf.Cos(rad), -Mathf.Sin(rad));
        Vector3 row3 = new Vector3(0.0f, Mathf.Sin(rad), Mathf.Cos(rad));

        return new Vector3(Vector3.Dot(rotateMe, row1), Vector3.Dot(rotateMe, row2), Vector3.Dot(rotateMe, row3));
    }

    private Vector3 RotateY(Vector3 rotateMe, float degree)
    {
        float rad = degree * (Mathf.PI / 180.0f);
        Vector3 row1 = new Vector3(Mathf.Cos(rad), 0.0f, Mathf.Sin(rad));
        Vector3 row2 = new Vector3(0.0f, 1.0f, 0.0f);
        Vector3 row3 = new Vector3(-Mathf.Sin(rad), 0.0f, Mathf.Cos(rad));

        return new Vector3(Vector3.Dot(rotateMe, row1), Vector3.Dot(rotateMe, row2), Vector3.Dot(rotateMe, row3));

    }

    private Vector3 RotateZ(Vector3 rotateMe, float degree)
    {
        float rad = degree * (Mathf.PI / 180f);
        Vector3 row1 = new Vector3(Mathf.Cos(rad), -Mathf.Sin(rad),0.0f);
        Vector3 row2 = new Vector3(Mathf.Sin(rad), Mathf.Cos(rad), 0.0f);
        Vector3 row3 = new Vector3(0.0f, 0f, 1.0f);

        return new Vector3(Vector3.Dot(rotateMe, row1), Vector3.Dot(rotateMe, row2), Vector3.Dot(rotateMe, row3));
    }

    private Vector3 Rotate(Vector3 rotateMe, float rx, float ry, float rz)
    {
        Vector3 returnMe = RotateZ(RotateY(RotateX(rotateMe, rx), ry), rz);
        return returnMe;
    }


    private List<Vector3> Rotate(List<Vector3> rotateList,float rx, float ry, float rz)
    {
        for (int ii = 0;ii< rotateList.Count; ii++)
        {
            rotateList[ii] = Rotate(rotateList[ii], rx, ry, rz);
        }
        return rotateList;
    }

    private Vector3[] Rotate(Vector3[] rotateList, float rx, float ry, float rz)
    {
        for (int ii = 0; ii < rotateList.Length; ii++)
        {
            rotateList[ii] = Rotate(rotateList[ii], rx, ry, rz);
        }
         return rotateList;
    }



}
