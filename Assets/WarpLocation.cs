using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpLocation : MonoBehaviour
{

    public Vector3 CCF_coordinate;
    public float scale = .25f;
    private BrainWarper brainWarper;
    [SerializeField] GameObject sphereChild;

    Quaternion startRotation;

    // Start is called before the first frame update
    void Start()
    {
        brainWarper = GameObject.Find("BrainWarper").GetComponent<BrainWarper>();
        Vector3 tmp = new Vector3(CCF_coordinate.x, CCF_coordinate.y, CCF_coordinate.z);
        this.transform.localPosition = SetToTransformPoint(tmp);
        startRotation = this.transform.rotation;


    }

    // Update is called once per frame
    void Update()
    {
        Vector3 tmp = new Vector3( CCF_coordinate.x , CCF_coordinate.y, CCF_coordinate.z);
        this.transform.position = SetToTransformPoint(tmp);
        this.transform.rotation = startRotation;
        this.transform.Translate(5.7f, 4f, -6.6f);
        this.transform.Rotate(0f, -90f, -180f);
        sphereChild.transform.localScale = Vector3.one * scale;

    }

    private Vector3 SetToTransformPoint(Vector3 coordinate)
    {
        if (brainWarper)
        {
            coordinate.x = +11400f - coordinate.x;
            coordinate.y = -coordinate.y;
            coordinate.z = 13200f - coordinate.z;
            return brainWarper.TransformPointInverse(coordinate) / 1000;
        }
        else
        {
            coordinate.x = -11400+coordinate.x;
            coordinate.y = -coordinate.y;
            return coordinate/1000;
        }
    }



}
