using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RigCenterMenu : MonoBehaviour
{
    [SerializeField] SphericalRigController rigController;

    [SerializeField] private TMP_InputField _apField;
    [SerializeField] private TMP_InputField _mlField;
    [SerializeField] private TMP_InputField _dvField;

    // Start is called before the first frame update
    void Start()
    {
        GrabRigPosition();
    }
    private void GrabRigPosition(){
        Vector3 pos = rigController.rigCenter.transform.position;
        _mlField.text = pos.x.ToString();
        _apField.text = pos.y.ToString();
        _dvField.text = pos.z.ToString();
    }

    public void ApplyPosition(){
        rigController.ParseRigMLFromString(_mlField.text);
        rigController.ParseRigAPFromString(_apField.text);
        rigController.ParseRigDVFromString(_dvField.text);
        rigController.SetProbePosition();
    }
}
