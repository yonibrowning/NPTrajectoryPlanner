using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphericalRigCoordinates
{
    public float apArcAngle;
    public float mlArcAngle;
    public float spin;
    public float manipulatorX;
    public float manipulatorY;
    public float manipulatorZ;

    public SphericalRigCoordinates(float apArcAngle, float mlArcAngle, float spin, float manipulatorX, float manipulatorY, float manipulatorZ)
    {
        this.apArcAngle = apArcAngle;
        this.mlArcAngle = mlArcAngle;
        this.spin = spin;
        this.manipulatorX = manipulatorX;
        this.manipulatorY = manipulatorY;
        this.manipulatorZ = manipulatorZ;
    }

    public SphericalRigCoordinates CopyRigCoordinates(SphericalRigCoordinates old)
    {
        return new SphericalRigCoordinates(old.apArcAngle, old.mlArcAngle, old.spin, old.manipulatorX, old.manipulatorY, old.manipulatorZ);
    }

    public List<string> ToList()
    {
        List<string> stringList = new List<string>();

        stringList.Add(apArcAngle.ToString());
        stringList.Add(mlArcAngle.ToString());
        stringList.Add(spin.ToString());
        stringList.Add(manipulatorX.ToString());
        stringList.Add(manipulatorY.ToString());
        stringList.Add(manipulatorZ.ToString());
        return stringList;
    }

    public override string ToString()
    {
        List<string> stringList = ToList();
        string returnme = "apAngle, " + stringList[0] + ", mlAngle, " + stringList[1] + ", spin, " + stringList[2] + ", X, " + stringList[3] + ", Y, " + stringList[4] + ", Z, " + stringList[5];
        return returnme;
    }
}
