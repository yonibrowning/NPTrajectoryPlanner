using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using TrajectoryPlanner;

public class SphericalRigController : MonoBehaviour
{

    [SerializeField] public Transform rigCenter;
    public float APOffsetAngle;
    private TrajectoryPlannerManager tpmanager;
    private Utils util;

    void Awake()
    {
        GameObject main = GameObject.Find("main");
        tpmanager = main.GetComponent<TrajectoryPlannerManager>();
        util = main.GetComponent<Utils>();
    }

    public void MoveRigCenter(Vector3 newPosition)
    {
        rigCenter.position = newPosition;
        SetProbePosition();
    }

    public void ParseRigMLFromString(string X){
        float x = float.Parse(X);
        SetRigML(x);
    }

    public void ParseRigAPFromString(string Z){
        float z = float.Parse(Z);
        SetRigAP(z);
    }

    public void ParseRigDVFromString(string Y){
        float y = float.Parse(Y);
        SetRigDV(y);
    }

    public void ParseRigTiltFromString(string tilt){
        float aptilt = (float)double.Parse(tilt);
        SetRigAPTilt(aptilt);
    }

    public void SetRigML(float X){
        Vector3 tmp_rigCenter = rigCenter.position;
        tmp_rigCenter.x = X;
        rigCenter.position = tmp_rigCenter;
        SetProbePosition();
    }

    public void SetRigAP(float Z){
        Vector3 tmp_rigCenter = rigCenter.position;
        tmp_rigCenter.z = Z;
        rigCenter.position = tmp_rigCenter;
        SetProbePosition();
    }
    
    public void SetRigDV(float Y){
        Vector3 tmp_rigCenter = rigCenter.position;
        tmp_rigCenter.y = Y;
        rigCenter.position = tmp_rigCenter;
        SetProbePosition();
    }

    public void SetRigAPTilt(float angle){
        APOffsetAngle = angle;
        SetProbePosition();
    }



    public void SetProbePosition()
    {
        foreach (ProbeManager probe in tpmanager.GetAllProbes()){
            probe.GetProbeController().SetProbePosition();
        }
    }


}


