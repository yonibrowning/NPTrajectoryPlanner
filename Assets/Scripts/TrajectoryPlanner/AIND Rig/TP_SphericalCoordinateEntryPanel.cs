using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TrajectoryPlanner;

public class TP_SphericalCoordinateEntryPanel : MonoBehaviour
{

    [SerializeField] private TMP_InputField apArcPosField;
    [SerializeField] private TMP_InputField mlArcPosField;
    [SerializeField] private TMP_InputField manipulatorXField;
    [SerializeField] private TMP_InputField manipulatorYField;
    [SerializeField] private TMP_InputField manipulatorZField;
    [SerializeField] private TMP_InputField spinField;

    [SerializeField] private TrajectoryPlannerManager tpmanager;

    public bool keyboardMovement = true;


    public float apArcAngle;
    public float mlArcAngle;
    public float manipulatorX;
    public float manipulatorY;
    public float manipulatorZ;
    public float spin;

    // Start is called before the first frame update
    void Start()
    {
        // Pull the tpmanager object
        GameObject main = GameObject.Find("main");
        tpmanager = main.GetComponent<TrajectoryPlannerManager>();
        // Set what to do when the field is clicked on.
        apArcPosField.onSelect.AddListener(delegate { OnThisSelect(); });
        mlArcPosField.onSelect.AddListener(delegate { OnThisSelect(); });
        manipulatorXField.onSelect.AddListener(delegate { OnThisSelect(); });
        manipulatorYField.onSelect.AddListener(delegate { OnThisSelect(); });
        manipulatorZField.onSelect.AddListener(delegate { OnThisSelect(); });
        spinField.onSelect.AddListener(delegate { OnThisSelect(); });
       
        // Set what to do when the field is deselected
        apArcPosField.onDeselect.AddListener(delegate { OnDeselect(); });
        mlArcPosField.onDeselect.AddListener(delegate { OnDeselect(); });
        manipulatorXField.onDeselect.AddListener(delegate { OnDeselect(); });
        manipulatorYField.onDeselect.AddListener(delegate { OnDeselect(); });
        manipulatorZField.onDeselect.AddListener(delegate { OnDeselect(); });
        spinField.onDeselect.AddListener(delegate { OnDeselect(); });

        apArcPosField.onEndEdit.AddListener(delegate { OnDeselect(); });
        mlArcPosField.onEndEdit.AddListener(delegate { OnDeselect(); });
        manipulatorXField.onEndEdit.AddListener(delegate { OnDeselect(); });
        manipulatorYField.onEndEdit.AddListener(delegate { OnDeselect(); });
        manipulatorZField.onEndEdit.AddListener(delegate { OnDeselect(); });
        spinField.onEndEdit.AddListener(delegate { OnDeselect(); });


        apArcPosField.onEndEdit.AddListener(delegate { Apply(); });
        mlArcPosField.onEndEdit.AddListener(delegate { Apply(); });
        manipulatorXField.onEndEdit.AddListener(delegate { Apply(); });
        manipulatorYField.onEndEdit.AddListener(delegate { Apply(); });
        manipulatorZField.onEndEdit.AddListener(delegate { Apply(); });
        spinField.onEndEdit.AddListener(delegate { Apply(); });

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
        if (keyboardMovement&(tpmanager.GetActiveProbeController()!=null))
        {
            SetTextValues(tpmanager.GetActiveProbeController());
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


    public void SetProbePosition()
    {
        tpmanager.GetActiveProbeController().MoveToSphericalRigCoordinates(new SphericalRigCoordinates(apArcAngle, mlArcAngle,spin, manipulatorX, manipulatorY, manipulatorZ));
    }

    public void SetTextValues(ProbeManager activeProbe)
    {
        StartCoroutine(SetTextValuesCoroutine(activeProbe));
    }

    public void SetTextValues()
    {
        SetTextValues(tpmanager.GetActiveProbeController());
    }

    IEnumerator SetTextValuesCoroutine(ProbeManager activeProbe)
    {

        apArcPosField.text = activeProbe.rigCoordinates.apArcAngle.ToString();
        mlArcPosField.text = activeProbe.rigCoordinates.mlArcAngle.ToString();

        spinField.text = activeProbe.rigCoordinates.spin.ToString();

        manipulatorXField.text = activeProbe.rigCoordinates.manipulatorX.ToString();
        manipulatorYField.text = activeProbe.rigCoordinates.manipulatorY.ToString();
        manipulatorZField.text = activeProbe.rigCoordinates.manipulatorZ.ToString();

        yield return null;


    }
}
