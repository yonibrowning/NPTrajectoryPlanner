using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrajectoryPlanner;


public class BrainWarper : MonoBehaviour
{
        
    private TrajectoryPlannerManager tpmanager;
    //public string filename;
    private CoordinateTransform coordinateTransform;    

    // Start is called before the first frame update
    void Start()
    {
        GameObject main = GameObject.Find("main");
        tpmanager = main.GetComponent<TrajectoryPlannerManager>();
        //coordinateTransform = tpmanager.GetCoordinateTransform();//new MRIThinPlateTransform();//
    }

    // Update is called once per frame
    public Mesh WarpMesh(Mesh warp_me)
    {
        CoordinateTransform coordinateTransform = tpmanager.GetActiveCoordinateTransform();
        Vector3[] vertices = warp_me.vertices;
        Debug.Log(vertices[1]);
        for (int ii=0;ii<vertices.Length;ii++){
            vertices[ii] = coordinateTransform.FromCCF(vertices[ii]);
        }
        Debug.Log(vertices[1]);

        warp_me.vertices = vertices;
        warp_me.RecalculateNormals();
        warp_me.RecalculateTangents();
        return warp_me;
    }

    public void WrapBrain(){
        Debug.Log("Trying some brain warping");
        GameObject[] allBrainBits = GameObject.FindGameObjectsWithTag("BrainRegion");
        Debug.Log(allBrainBits.Length);
        foreach (GameObject brainBit in allBrainBits){

            if (brainBit.GetComponent<MeshFilter>()!=null){ 
                Mesh brainBitMesh = brainBit.GetComponent<MeshFilter>().mesh;
                brainBitMesh = WarpMesh(brainBitMesh);
                Debug.Log(brainBit.name);
            }
        }
        Debug.Log("Warping Complete");
    }

}
