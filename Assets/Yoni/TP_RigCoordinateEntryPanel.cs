using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class TP_RigCoordinateEntryPanel : MonoBehaviour
{

    [SerializeField] private TMP_InputField apArcPosField;
    [SerializeField] private TMP_InputField mlArcPosField;
    [SerializeField] private TMP_InputField manipulatorXField;
    [SerializeField] private TMP_InputField manipulatorYField;
    [SerializeField] private TMP_InputField manipulatorZField;
    [SerializeField] private TMP_InputField spinField;

    [SerializeField] private TMP_InputField rigCenterXField;
    [SerializeField] private TMP_InputField rigCenterYField;
    [SerializeField] private TMP_InputField rigCenterZField;

    [SerializeField] private RigController rigController;
    public bool keyboardMovement = true;


    public float apArcAngle;
    public float mlArcAngle;
    public float manipulatorX;
    public float manipulatorY;
    public float manipulatorZ;
    public float spin;


    [SerializeField] private TrajectoryPlannerManager tpmanager;

    // Start is called before the first frame update
    void Start()
    {
        /*// Set what to do when value changes
        apArcPosField.onValueChanged.AddListener(delegate { Apply(); });
        mlArcPosField.onValueChanged.AddListener(delegate { Apply(); });
        manipulatorXField.onValueChanged.AddListener(delegate { Apply(); });
        manipulatorYField.onValueChanged.AddListener(delegate { Apply(); });
        manipulatorZField.onValueChanged.AddListener(delegate { Apply(); });
        spinField.onValueChanged.AddListener(delegate { Apply(); });

        rigCenterXField.onValueChanged.AddListener(delegate { SetRigCenter(); });
        rigCenterYField.onValueChanged.AddListener(delegate { SetRigCenter(); });
        rigCenterZField.onValueChanged.AddListener(delegate { SetRigCenter(); });
        */
        
        // Set what to do when the field is clicked on.
        apArcPosField.onSelect.AddListener(delegate { OnThisSelect(); });
        mlArcPosField.onSelect.AddListener(delegate { OnThisSelect(); });
        manipulatorXField.onSelect.AddListener(delegate { OnThisSelect(); });
        manipulatorYField.onSelect.AddListener(delegate { OnThisSelect(); });
        manipulatorZField.onSelect.AddListener(delegate { OnThisSelect(); });
        spinField.onSelect.AddListener(delegate { OnThisSelect(); });
        rigCenterXField.onSelect.AddListener(delegate { OnThisSelect(); });
        rigCenterYField.onSelect.AddListener(delegate { OnThisSelect(); });
        rigCenterZField.onSelect.AddListener(delegate { OnThisSelect(); });

        // Set what to do when the field is deselected
        apArcPosField.onDeselect.AddListener(delegate { OnDeselect(); });
        mlArcPosField.onDeselect.AddListener(delegate { OnDeselect(); });
        manipulatorXField.onDeselect.AddListener(delegate { OnDeselect(); });
        manipulatorYField.onDeselect.AddListener(delegate { OnDeselect(); });
        manipulatorZField.onDeselect.AddListener(delegate { OnDeselect(); });
        spinField.onDeselect.AddListener(delegate { OnDeselect(); });
        rigCenterXField.onDeselect.AddListener(delegate { OnDeselect(); });
        rigCenterYField.onDeselect.AddListener(delegate { OnDeselect(); });
        rigCenterZField.onDeselect.AddListener(delegate { OnDeselect(); });

        apArcPosField.onEndEdit.AddListener(delegate { OnDeselect(); });
        mlArcPosField.onEndEdit.AddListener(delegate { OnDeselect(); });
        manipulatorXField.onEndEdit.AddListener(delegate { OnDeselect(); });
        manipulatorYField.onEndEdit.AddListener(delegate { OnDeselect(); });
        manipulatorZField.onEndEdit.AddListener(delegate { OnDeselect(); });
        spinField.onEndEdit.AddListener(delegate { OnDeselect(); });
        rigCenterXField.onEndEdit.AddListener(delegate { OnDeselect(); });
        rigCenterYField.onEndEdit.AddListener(delegate { OnDeselect(); });
        rigCenterZField.onEndEdit.AddListener(delegate { OnDeselect(); });

        apArcPosField.onEndEdit.AddListener(delegate { Apply(); });
        mlArcPosField.onEndEdit.AddListener(delegate { Apply(); });
        manipulatorXField.onEndEdit.AddListener(delegate { Apply(); });
        manipulatorYField.onEndEdit.AddListener(delegate { Apply(); });
        manipulatorZField.onEndEdit.AddListener(delegate { Apply(); });
        spinField.onEndEdit.AddListener(delegate { Apply(); });
        rigCenterXField.onEndEdit.AddListener(delegate { Apply(); });
        rigCenterYField.onEndEdit.AddListener(delegate { Apply(); });
        rigCenterZField.onEndEdit.AddListener(delegate { Apply(); });

    }

    public void OnThisSelect()
    {
        keyboardMovement = false;
    }

    public void OnDeselect()
    {
        //Apply();
        keyboardMovement = true;
    }

    // Update is called once per frame
    void Update()
    {

        if (keyboardMovement&(tpmanager.activeProbeController!=null))
        {
            SetTextValues(tpmanager.activeProbeController);
        }
    }
    

    public void Apply()
    {
        //Debug.Log("Apply Called!!!");
        try{
            apArcAngle = float.Parse(apArcPosField.text);
            mlArcAngle = float.Parse(mlArcPosField.text);
            spin = float.Parse(spinField.text);

            manipulatorX = float.Parse(manipulatorXField.text);
            manipulatorY = float.Parse(manipulatorYField.text);
            manipulatorZ = float.Parse(manipulatorZField.text);
        }
        catch
        {
            //Debug.Log("Bad formatting?");
        }

        SetProbePosition();

    }

    public void SetRigCenter()
    {
        float rigCenterX = 0f;
        float rigCenterY = 0f;
        float rigCenterZ = 0f;

        try
        {
            rigCenterX = float.Parse(rigCenterXField.text);
        }catch{}

        try
        {
            rigCenterZ = float.Parse(rigCenterZField.text);
        }
        catch { }

        try
        {
            rigCenterY = float.Parse(rigCenterYField.text);
        }
        catch { }

        rigController.MoveRigCenter(new Vector3(rigCenterX, rigCenterY, rigCenterZ)) ;

        foreach (ProbeController thisProbe in tpmanager.allProbes)
        {
            thisProbe.SetProbePositionAINDRig();
        }


    }

    public void SetRigCenterText()
    {
        rigCenterXField.text = rigController.rigCenter.position.x.ToString();
        rigCenterYField.text = rigController.rigCenter.position.y.ToString();
        rigCenterZField.text = rigController.rigCenter.position.z.ToString();
    }


    public void SetProbePosition()
    {
        SetRigCenter();
        tpmanager.activeProbeController.MoveToAINDRigCoordinates(new RigCoordinates(apArcAngle, mlArcAngle,spin, manipulatorX, manipulatorY, manipulatorZ));
    }

    public void SetTextValues(ProbeController activeProbe)
    {
        StartCoroutine(SetTextValuesCoroutine(activeProbe));
    }

    public void SetTextValues()
    {
        SetTextValues(tpmanager.activeProbeController);
    }

    IEnumerator SetTextValuesCoroutine(ProbeController activeProbe)
    {

        apArcPosField.text = activeProbe.rigCoordinates.apArcAngle.ToString();
        mlArcPosField.text = activeProbe.rigCoordinates.mlArcAngle.ToString();

        spinField.text = activeProbe.rigCoordinates.spin.ToString();

        manipulatorXField.text = activeProbe.rigCoordinates.manipulatorX.ToString();
        manipulatorYField.text = activeProbe.rigCoordinates.manipulatorY.ToString();
        manipulatorZField.text = activeProbe.rigCoordinates.manipulatorZ.ToString();

        SetRigCenterText();

        //rigCenterXField.text = rigController.rigCenter.transform.position.x.ToString();
        //rigCenterYField.text = rigController.rigCenter.transform.position.y.ToString();
        //rigCenterZField.text = rigController.rigCenter.transform.position.z.ToString();

        yield return null;


    }




}
