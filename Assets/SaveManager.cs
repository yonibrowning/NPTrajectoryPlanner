using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Threading;

public class SaveManager : MonoBehaviour
{
    public GameObject savePanel;
    public InputField saveLocField;
    public InputField loadLocField;

    private TrajectoryPlannerManager tpmanager;
    private RigController rigController;
    public TP_RigCoordinateEntryPanel rigCoordinatePanel;


    // Start is called before the first frame update
    void Start()
    {
        tpmanager = GameObject.Find("main").GetComponent<TrajectoryPlannerManager>();
        rigController = GameObject.Find("Rig").GetComponent<RigController>();
        rigCoordinatePanel = GameObject.Find("RigCoordinateEntryPanel").GetComponent<TP_RigCoordinateEntryPanel>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N)&&(Input.GetKey(KeyCode.RightControl)||Input.GetKey(KeyCode.LeftControl)))
        {
            savePanel.SetActive(!savePanel.activeSelf);
            
            rigCoordinatePanel.gameObject.SetActive(!rigCoordinatePanel.isActiveAndEnabled);

        }
    }

    public void SaveFile()
    {
        string path = saveLocField.text;
        List<string> lines = new List<string>();

        StreamWriter writer = new StreamWriter(path, false);
        // Write the Rig Center Position
        writer.WriteLine(rigController.rigCenter.position.ToString());

        List<ProbeController> probeList = tpmanager.allProbes;


        foreach (ProbeController probe in probeList)
        {
            string tmp = probe.GetID()+", Type,"+ probe.GetProbeType() +","+probe.rigCoordinates.ToString();
            writer.WriteLine(tmp);
            Debug.Log(tmp);
        }

        writer.Close();
    }


    public void LoadFile()
    {
        StreamReader reader = new StreamReader(loadLocField.text);

        string this_line = reader.ReadLine();
        Vector3 rigCenter = StringToVector3(this_line);
        //rigController.rigCenter.position = rigCenter;

        while (reader.Peek() >= 0)
        {
            this_line = reader.ReadLine();
            string[] splt = this_line.Split(',');

            int probe_code = int.Parse(splt[2]);
            float apAngle = float.Parse(splt[4]);
            float mlAngle = float.Parse(splt[6]);
            float spin = float.Parse(splt[8]);
            float manX = float.Parse(splt[10]);
            float manY = float.Parse(splt[12]);
            float manZ = float.Parse(splt[14]);

            ProbeController newProbe = tpmanager.AddNewProbe(probe_code);
            newProbe.rigController = rigController;
            newProbe.SetProbePositionToAINDRigCenter(rigController.rigCenter);
            newProbe.MoveToAINDRigCoordinates(new RigCoordinates(apAngle, mlAngle, spin, manX, manY, manZ));
            Debug.Log(newProbe.rigCoordinates.ToString());

        }

        //rigController.rigCenter.position = rigCenter;
        //if (rigCoordinatePanel.isActiveAndEnabled)
        //{
        //    rigCoordinatePanel.SetTextValues();
        //}


    }

    private Vector3 StringToVector3(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }
}
