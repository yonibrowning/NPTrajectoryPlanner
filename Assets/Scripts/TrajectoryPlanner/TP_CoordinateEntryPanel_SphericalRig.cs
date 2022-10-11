using TMPro;
using UnityEngine;
using TrajectoryPlanner;

public class TP_CoordinateEntryPanel_SphericalRig: MonoBehaviour
{
    [SerializeField] private TMP_Text _xText;
    [SerializeField] private TMP_Text _yText;
    [SerializeField] private TMP_Text _zText;

    [SerializeField] private TMP_InputField _xField;
    [SerializeField] private TMP_InputField _yField;
    [SerializeField] private TMP_InputField _zField;
    [SerializeField] private TMP_InputField _mlField;
    [SerializeField] private TMP_InputField _apField;
    [SerializeField] private TMP_InputField _spinField;

    [SerializeField] private TrajectoryPlannerManager _tpmanager;
    
    [SerializeField] private TP_ProbeQuickSettings _probeQuickSettings;

    private ProbeManager _linkedProbe;

    private void Start()
    {
        _xField.onEndEdit.AddListener(delegate { ApplyPosition(); });
        _yField.onEndEdit.AddListener(delegate { ApplyPosition(); });
        _zField.onEndEdit.AddListener(delegate { ApplyPosition(); });

        _mlField.onEndEdit.AddListener(delegate { ApplyAngles(); });
        _apField.onEndEdit.AddListener(delegate { ApplyAngles(); });
        _spinField.onEndEdit.AddListener(delegate { ApplyAngles(); });
    }

    public void LinkProbe(ProbeManager probeManager)
    {
        _linkedProbe = probeManager;
        // change the apmldv/depth text fields to match the prefix on this probe's insertion
        //string prefix = _linkedProbe.GetProbeController().Insertion.CoordinateTransform.Prefix;
        string prefix = "Man ";
        _xText.text = prefix + "X";
        _yText.text = prefix + "Y";
        _zText.text = prefix + "Z";
    }

    public void UnlinkProbe()
    {
        _linkedProbe = null;
    }

    public void UpdateText()
    {
        if (_linkedProbe == null)
        {
            _xField.text = "";
            _yField.text = "";
            _zField.text = "";
            _mlField.text = "";
            _apField.text = "";
            _spinField.text = "";
            return;
        }

        SphericalRigCoordinates rigCoordinates = ((SphericalRigProbeController)_linkedProbe.GetProbeController()).GetSphericalRigCoordiantes();

        float depth = float.NaN;
        float mult = _tpmanager.GetSetting_DisplayUM() ? 1000f : 1f;

        _xField.text = Round2Str(rigCoordinates.manipulatorX * mult);
        _yField.text = Round2Str(rigCoordinates.manipulatorY  * mult);
        _zField.text = Round2Str(rigCoordinates.manipulatorZ  * mult);
        _mlField.text = Round2Str(rigCoordinates.mlArcAngle);
        _apField.text = Round2Str(rigCoordinates.apArcAngle);
        _spinField.text = Round2Str(rigCoordinates.spin);
        
    }

    private string Round2Str(float value)
    {
        if (float.IsNaN(value))
            return "nan";

        return _tpmanager.GetSetting_DisplayUM() ? ((int)value).ToString() : value.ToString("F3");
    }

    private void ApplyPosition()
    {
        //try
        //{
        //    float ap = (apField.text.Length > 0) ? float.Parse(apField.text) : 0;
        //    float ml = (mlField.text.Length > 0) ? float.Parse(mlField.text) : 0;
        //    float dv = (dvField.text.Length > 0) ? float.Parse(dvField.text) : 0;
        //    float depth = (depthField.text.Length > 0 && depthField.text != "nan") ? 
        //        float.Parse(depthField.text) + 200f : 
        //        0;

        //    Debug.LogError("TODO implement");
        //    //linkedProbe.GetProbeController().SetProbePositionTransformed(ap, ml, dv, depth/1000f);
        //}
        //catch
        //{
        //    Debug.Log("Bad formatting?");
        //}
    }

    private void ApplyAngles()
    {
        /*try
        {
            Vector3 angles = new Vector3((_mp.text.Length > 0) ? float.Parse(_phiField.text) : 0,
                (_thetaField.text.Length > 0) ? float.Parse(_thetaField.text) : 0,
                (_spinField.text.Length > 0) ? float.Parse(_spinField.text) : 0);

            if (_tpmanager.GetSetting_UseIBLAngles())
                angles = Utils.IBL2World(angles);

            _linkedProbe.GetProbeController().SetProbeAngles(angles);
        }
        catch
        {
            Debug.Log("Bad formatting?");
        }*/
    }
}
