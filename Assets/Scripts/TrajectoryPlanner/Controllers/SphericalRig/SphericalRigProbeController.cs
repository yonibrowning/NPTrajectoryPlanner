using System;
using System.Collections;
using System.Collections.Generic;
using TrajectoryPlanner;
using UnityEngine;
using UnityEngine.EventSystems;
using CoordinateSpaces;

public class SphericalRigProbeController : ProbeController
{
    #region Movement Constants
    private const float REC_HEIGHT_SPEED = 0.1f;
    private const float MOVE_INCREMENT_TAP = 0.010f; // move 1 um per tap
    private const float MOVE_INCREMENT_TAP_FAST = 0.100f;
    private const float MOVE_INCREMENT_TAP_SLOW = 0.001f;
    private const float MOVE_INCREMENT_HOLD = 0.100f; // move 50 um per second when holding
    private const float MOVE_INCREMENT_HOLD_FAST = 1.000f;
    private const float MOVE_INCREMENT_HOLD_SLOW = 0.010f;
    private const float ROT_INCREMENT_TAP = 1f;
    private const float ROT_INCREMENT_TAP_FAST = 10f;
    private const float ROT_INCREMENT_TAP_SLOW = 0.1f;
    private const float ROT_INCREMENT_HOLD = 5f;
    private const float ROT_INCREMENT_HOLD_FAST = 25;
    private const float ROT_INCREMENT_HOLD_SLOW = 2.5f;
    #endregion

    #region Key hold flags
    private bool keyFast = false;
    private bool keySlow = false;
    private bool keyHeld = false; // If a key is held, we will skip re-checking the key hold delay for any other keys that are added
    private float keyPressTime = 0f;
    private const float keyHoldDelay = 0.300f;
    #endregion

    #region Angle limits
    private const float maxAP = 75f;
    private const float maxML = 40f;

    private const float minTheta = 0;
    private const float maxTheta = 90;
    #endregion

    #region Recording region
    private float minRecordHeight;
    private float maxRecordHeight; // get this by measuring the height of the recording rectangle and subtracting from 10
    private float recordingRegionSizeY;
    #endregion

    #region Defaults
    // in x,y,z
    private Vector3 defaultStart = Vector3.zero; // new Vector3(5.4f, 5.7f, 0.332f);
    private float defaultDepth = 0f;
    private Vector2 defaultAngles = new Vector2(-90f, 0f); // 0 phi is forward, default theta is 90 degrees down from horizontal, but internally this is a value of 0f
    #endregion

    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private float depth;

    // Offset vectors
    private GameObject probeTipOffset;
    private GameObject probeTipTop;

    // References
    [SerializeField] private Transform _probeTipT;
    [SerializeField] private List<GameObject> recordingRegionGOs;
    [SerializeField] private Transform rotateAround;

    public override Transform ProbeTipT { get { return _probeTipT; } }

    private SphericalRigController rigController;
    private SphericalRigCoordinates rigCoordinates;
    


    private void Awake()
    {
        // Create two points offset from the tip that we'll use to interpolate where we are on the probe
        probeTipOffset = new GameObject(name + "TipOffset");
        probeTipOffset.transform.parent = _probeTipT;
        probeTipOffset.transform.position = _probeTipT.position + _probeTipT.up * 0.2f;

        probeTipTop = new GameObject(name + "TipTop");
        probeTipTop.transform.parent = _probeTipT;
        probeTipTop.transform.position = _probeTipT.position + _probeTipT.up * 10.2f;

        depth = defaultDepth;

        _initialPosition = transform.position;
        _initialRotation = transform.rotation;

        UpdateRecordingRegionVars();

        Insertion = new ProbeInsertion(defaultStart, defaultAngles, TPManager.GetCoordinateSpace(), TPManager.GetActiveCoordinateTransform());

        //Attach to AIND Spherical Rig
        rigController = GameObject.Find("Rig Controller").GetComponent<SphericalRigController>();
        CreateSphericalRigCoordinates();
    }

    private void Start()
    {
        SetProbePosition();
    }

    private void CreateSphericalRigCoordinates()
    {
        SetProbePositionToRigCenter();
        rigCoordinates = new SphericalRigCoordinates(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f);
    }
    

    private void SetProbePositionToRigCenter()
    {
        //Set Rotation to zero!
        transform.rotation =  Quaternion.identity;//;initialRotation; 
        //Debug.Log(initialRotation);
        transform.Rotate(new Vector3(0f, 180f, 0f)); //Our zero is different from Unity's
        //Find difference between probe tip and rig center
        Vector3 diff = _probeTipT.position - rigController.rigCenter.position;
        //Move to rig center
        transform.position -= diff;
    }

    public override void SetProbePosition()
    {
        //Move probe tip position to be at the center of the the rig.
        SetProbePositionToRigCenter();

        // Set XYZ
        rigCoordinates.manipulatorX = Mathf.Clamp(rigCoordinates.manipulatorX, -7.5f, 7.5f);
        rigCoordinates.manipulatorY = Mathf.Clamp(rigCoordinates.manipulatorY, -7.5f, 7.5f);
        rigCoordinates.manipulatorZ = Mathf.Clamp(rigCoordinates.manipulatorZ, -7.5f, 7.5f);
        rigCoordinates.mlArcAngle = Mathf.Clamp(rigCoordinates.mlArcAngle, -40f, 40f);
        rigCoordinates.apArcAngle = Mathf.Clamp(rigCoordinates.apArcAngle, -75f, 75f);

        transform.Translate(rigCoordinates.manipulatorX, -rigCoordinates.manipulatorZ, rigCoordinates.manipulatorY);

        //Set Roll
        transform.RotateAround(rigController.rigCenter.position, new Vector3(0f, 0f, 1f), rigCoordinates.mlArcAngle);

        //Set Pitch
        transform.RotateAround(rigController.rigCenter.position, new Vector3(-1f, 0f, 0f), rigCoordinates.apArcAngle-rigController.APOffsetAngle);
        
        //Spin
        transform.RotateAround(rotateAround.position, rotateAround.up, rigCoordinates.spin);
    }



    public override void ResetPosition(){
        rigCoordinates.manipulatorX = 0;
        rigCoordinates.manipulatorY = 0;
        rigCoordinates.manipulatorZ = 0;
    }

    public override void ResetAngles(){
        rigCoordinates.mlArcAngle = 0;
        rigCoordinates.apArcAngle = 0;
        rigCoordinates.spin = 0;
    }

    public override void SetProbeAngles(Vector3 angles)
    {
        rigCoordinates.mlArcAngle = angles.x;
        rigCoordinates.apArcAngle = angles.y;
        rigCoordinates.spin = angles.z;
        SetProbePosition();
    }

    public override void SetProbePosition(Vector3 position){
        rigCoordinates.manipulatorX = 0;
        rigCoordinates.manipulatorY = 0;
        rigCoordinates.manipulatorZ = 0;
        SetProbePosition();
    }

    public void SetProbePositon(SphericalRigCoordinates newCooridnate){
        rigCoordinates = newCooridnate;
        SetProbePosition();
    }

    public override void SetProbePosition(Vector4 positionDepth){
        throw new NotImplementedException();

    }

    public override void SetProbePosition(ProbeInsertion localInsertion)
    {
        throw new NotImplementedException();
    }

    public override void ResetInsertion()
    {
        ResetPosition();
        ResetAngles();
        SetProbePosition();
    }

    public SphericalRigCoordinates GetSphericalRigCoordiantes(){
        return rigCoordinates;
    }

    public void MoveProbeSphericalArcAngles(float ap, float ml, bool pressed)
    {
        float speed = pressed ? 
            keyFast ? MOVE_INCREMENT_TAP_FAST : keySlow ? MOVE_INCREMENT_TAP_SLOW : MOVE_INCREMENT_TAP : 
            keyFast ? MOVE_INCREMENT_HOLD_FAST * Time.deltaTime : keySlow ? MOVE_INCREMENT_HOLD_SLOW * Time.deltaTime : MOVE_INCREMENT_HOLD * Time.deltaTime;

        rigCoordinates.apArcAngle += ap * speed;
        rigCoordinates.mlArcAngle += ml * speed;
    }

    public void MoveProbeSphericalDV(float dv, bool pressed)
    {
        float speed = pressed ? 
            keyFast ? MOVE_INCREMENT_TAP_FAST : keySlow ? MOVE_INCREMENT_TAP_SLOW : MOVE_INCREMENT_TAP : 
            keyFast ? MOVE_INCREMENT_HOLD_FAST * Time.deltaTime : keySlow ? MOVE_INCREMENT_HOLD_SLOW * Time.deltaTime : MOVE_INCREMENT_HOLD * Time.deltaTime;

        rigCoordinates.manipulatorZ += dv * speed;
    }

    public void MoveProbeSphericalXY(float x,float y, bool pressed)
    {
        float speed = pressed ? 
            keyFast ? MOVE_INCREMENT_TAP_FAST : keySlow ? MOVE_INCREMENT_TAP_SLOW : MOVE_INCREMENT_TAP : 
            keyFast ? MOVE_INCREMENT_HOLD_FAST * Time.deltaTime : keySlow ? MOVE_INCREMENT_HOLD_SLOW * Time.deltaTime : MOVE_INCREMENT_HOLD * Time.deltaTime;

        rigCoordinates.manipulatorX += x * speed;
        rigCoordinates.manipulatorY += y * speed;
    }

    public void MoveProbeSphericalSpin(float s,bool pressed){
        float speed = pressed ? 
            keyFast ? MOVE_INCREMENT_TAP_FAST : keySlow ? MOVE_INCREMENT_TAP_SLOW : MOVE_INCREMENT_TAP : 
            keyFast ? MOVE_INCREMENT_HOLD_FAST * Time.deltaTime : keySlow ? MOVE_INCREMENT_HOLD_SLOW * Time.deltaTime : MOVE_INCREMENT_HOLD * Time.deltaTime;

        rigCoordinates.spin += s * speed;
    }


    #region Keyboard movement

    private void CheckForSpeedKeys()
    {
        keyFast = Input.GetKey(KeyCode.LeftShift);
        keySlow = Input.GetKey(KeyCode.LeftControl);
    }

    public override bool MoveProbe_Keyboard(bool checkForCollisions)
    {
        SphericalRigCoordinates initialCoordinates = rigCoordinates.CopyRigCoordinates(rigCoordinates);

        bool moved = false;
        bool keyHoldDelayPassed = (Time.realtimeSinceStartup - keyPressTime) > keyHoldDelay;

        CheckForSpeedKeys();

        //AP Arc Angle Move Anterior
        if (Input.GetKeyDown(KeyCode.W))
        {
            moved = true;
            keyPressTime = Time.realtimeSinceStartup;
            MoveProbeSphericalArcAngles(1f, 0f, true);
        }
        else if (Input.GetKey(KeyCode.W) && (keyHeld || keyHoldDelayPassed))
        {
            keyHeld = true;
            moved = true;
            MoveProbeSphericalArcAngles(1f, 0f, true);
        }

        //AP Arc Angle Move Posterior
        if (Input.GetKeyDown(KeyCode.S))
        {
            moved = true;
            keyPressTime = Time.realtimeSinceStartup;
            MoveProbeSphericalArcAngles(-1f, 0f, true);
        }
        else if (Input.GetKey(KeyCode.S) && (keyHeld || keyHoldDelayPassed))
        {
            keyHeld = true;
            moved = true;
            MoveProbeSphericalArcAngles(-1f, 0f, true);
        }

        //ML Arc Angle Move Left
        if (Input.GetKeyDown(KeyCode.A))
        {
            moved = true;
            keyPressTime = Time.realtimeSinceStartup;
            MoveProbeSphericalArcAngles(0, -1f, true);
        }
        else if (Input.GetKey(KeyCode.A) && (keyHeld || keyHoldDelayPassed))
        {
            keyHeld = true;
            moved = true;
            MoveProbeSphericalArcAngles(0, -1f, true);
        }

        //ML Arc Angle Move Right
        if (Input.GetKeyDown(KeyCode.D))
        {
            moved = true;
            keyPressTime = Time.realtimeSinceStartup;
            MoveProbeSphericalArcAngles(0, 1f, true);
        }
        else if (Input.GetKey(KeyCode.D) && (keyHeld || keyHoldDelayPassed))
        {
            keyHeld = true;
            moved = true;
            MoveProbeSphericalArcAngles(0, 1f, true);
        }

        //Spin Clockwise
        if (Input.GetKeyDown(KeyCode.Q))
        {
            moved = true;
            keyPressTime = Time.realtimeSinceStartup;
            MoveProbeSphericalSpin(-1f, true);
        }
        else if (Input.GetKey(KeyCode.Q) && (keyHeld || keyHoldDelayPassed))
        {
            keyHeld = true;
            moved = true;
            MoveProbeSphericalSpin(-1f, true);
        }

        //Spin CounterClockwise
        if (Input.GetKeyDown(KeyCode.E))
        {
            moved = true;
            keyPressTime = Time.realtimeSinceStartup;
            MoveProbeSphericalSpin(1f, true);
        }
        else if (Input.GetKey(KeyCode.E) && (keyHeld || keyHoldDelayPassed))
        {
            keyHeld = true;
            moved = true;
            MoveProbeSphericalSpin(1f, true);
        }

        //Lower Probe
        if (Input.GetKeyDown(KeyCode.Z))
        {
            moved = true;
            keyPressTime = Time.realtimeSinceStartup;
            MoveProbeSphericalDV(1f, true);
        }
        else if (Input.GetKey(KeyCode.Z) && (keyHeld || keyHoldDelayPassed))
        {
            keyHeld = true;
            moved = true;
            MoveProbeSphericalDV(1f, true);
        }

        //Raise Probe
        if (Input.GetKeyDown(KeyCode.X))
        {
            moved = true;
            keyPressTime = Time.realtimeSinceStartup;
            MoveProbeSphericalDV(-1f, true);
        }
        else if (Input.GetKey(KeyCode.X) && (keyHeld || keyHoldDelayPassed))
        {
            keyHeld = true;
            moved = true;
            MoveProbeSphericalDV(-1f, true);
        }

        //Move Manipulator Right
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            moved = true;
            keyPressTime = Time.realtimeSinceStartup;
            MoveProbeSphericalXY(1,0, true);
        }
        else if (Input.GetKey(KeyCode.RightArrow) && (keyHeld || keyHoldDelayPassed))
        {
            keyHeld = true;
            moved = true;
            MoveProbeSphericalXY(1,0, true);
        }

        //Move Manipulator Left
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            moved = true;
            keyPressTime = Time.realtimeSinceStartup;
            MoveProbeSphericalXY(-1,0, true);
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && (keyHeld || keyHoldDelayPassed))
        {
            keyHeld = true;
            moved = true;
            MoveProbeSphericalXY(-1,0, true);
        }

        //Move Manipulator Forward
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            moved = true;
            keyPressTime = Time.realtimeSinceStartup;
            MoveProbeSphericalXY(0,1, true);
        }
        else if (Input.GetKey(KeyCode.UpArrow) && (keyHeld || keyHoldDelayPassed))
        {
            keyHeld = true;
            moved = true;
            MoveProbeSphericalXY(0,1, true);
        }

        //Move Manipulator Backward
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            moved = true;
            keyPressTime = Time.realtimeSinceStartup;
            MoveProbeSphericalXY(0,-1, true);
        }
        else if (Input.GetKey(KeyCode.DownArrow) && (keyHeld || keyHoldDelayPassed))
        {
            keyHeld = true;
            moved = true;
            MoveProbeSphericalXY(0,-1, true);
        }

        if (moved)// & tpmanager.thisCoordinatePanel.GetComponent<TP_SphericalCoordinateEntryPanel>().keyboardMovement)
        {
            // If the probe was moved, set the new position
            SetProbePosition();

            // Check collisions if we need to
            if (checkForCollisions)
                ProbeManager.CheckCollisions(TPManager.GetAllNonActiveColliders());

            // Update all the UI panels
            ProbeManager.UpdateUI();

            return true;

        }
        else
        {
            return false;
        }
    }
    #endregion
   
    #region Recording region UI
    public override void ChangeRecordingRegionSize(float newSize)
    {
        recordingRegionSizeY = newSize;

        foreach (GameObject go in recordingRegionGOs)
        {
            // This is a little complicated if we want to do it right (since you can accidentally scale the recording region off the probe.
            // For now, we will just reset the y position to be back at the bottom of the probe.
            Vector3 scale = go.transform.localScale;
            scale.y = newSize;
            go.transform.localScale = scale;
            Vector3 pos = go.transform.localPosition;
            pos.y = newSize / 2f + 0.2f;
            go.transform.localPosition = pos;
        }

        UpdateRecordingRegionVars();
    }

    /// <summary>
    /// Move the recording region up or down
    /// </summary>
    /// <param name="dir">-1 or 1 to indicate direction</param>
    private void ShiftRecordingRegion(float dir)
    {
        // Loop over recording regions to handle 4-shank (and 8-shank) probes
        foreach (GameObject recordingRegion in recordingRegionGOs)
        {
            Vector3 localPosition = recordingRegion.transform.localPosition;
            float localRecordHeightSpeed = Input.GetKey(KeyCode.LeftShift) ? REC_HEIGHT_SPEED * 2 : REC_HEIGHT_SPEED;
            localPosition.y = Mathf.Clamp(localPosition.y + dir * localRecordHeightSpeed, minRecordHeight, maxRecordHeight);
            recordingRegion.transform.localPosition = localPosition;
        }
    }

    private void UpdateRecordingRegionVars()
    {
        minRecordHeight = recordingRegionGOs[0].transform.localPosition.y;
        maxRecordHeight = minRecordHeight + (10 - recordingRegionGOs[0].transform.localScale.y);
    }

    #endregion



    #region Getters

    /// <summary>
    /// Return the tip coordinates in **un-transformed** world coordinates
    /// </summary>
    /// <returns></returns>
    public override (Vector3 tipCoordWorld, Vector3 tipUpWorld) GetTipWorld()
    {
        Vector3 tipCoordWorld = WorldT2WorldU(_probeTipT.position);
        Vector3 tipUpWorld = (WorldT2WorldU(probeTipTop.transform.position) - tipCoordWorld).normalized;
        return (tipCoordWorld, tipUpWorld);
    }

    /// <summary>
    /// Convert a transformed world coordinate into an un-transformed coordinate
    /// </summary>
    /// <param name="coordWorldT"></param>
    /// <returns></returns>
    private Vector3 WorldT2WorldU(Vector3 coordWorldT)
    {
        return Insertion.CoordinateSpace.Space2World(Insertion.CoordinateTransform.Transform2Space(Insertion.CoordinateTransform.Space2TransformRot(Insertion.CoordinateSpace.World2Space(coordWorldT))));
    }

    public override (Vector3 startCoordWorld, Vector3 endCoordWorld) GetRecordingRegionWorld()
    {
        return GetRecordingRegionWorld(probeTipOffset.transform);
    }

    public override (Vector3 startCoordWorld, Vector3 endCoordWorld) GetRecordingRegionWorld(Transform tipTransform)
    {
        if (TPManager.GetSetting_ShowRecRegionOnly())
        {
            // only rec region
            (float mmStartPos, float mmRecordingSize) = GetRecordingRegionHeight();

            Vector3 startCoordWorld = tipTransform.position + tipTransform.up * mmStartPos;//WorldT2WorldU(tipTransform.position + tipTransform.up * mmStartPos);
            Vector3 endCoordWorld = tipTransform.position + tipTransform.up * (mmStartPos + mmRecordingSize);//WorldT2WorldU(tipTransform.position + tipTransform.up * (mmStartPos + mmRecordingSize));

#if UNITY_EDITOR
            //GameObject.Find("recording_bot").transform.position = startCoordWorld;
            //GameObject.Find("recording_top").transform.position = endCoordWorld;
#endif
            return (startCoordWorld, endCoordWorld);
        }
        else
        {
            return (WorldT2WorldU(tipTransform.position), WorldT2WorldU(tipTransform.position));
        }
    }

    /// <summary>
    /// Return the height of the bottom in mm and the total height
    /// </summary>
    /// <returns>float array [0]=bottom, [1]=height</returns>
    public override (float, float) GetRecordingRegionHeight()
    {
        return (recordingRegionGOs[0].transform.localPosition.y - minRecordHeight, recordingRegionSizeY);
    }

    /// <summary>
    /// Return the current size of the recording region
    /// </summary>
    /// <returns>size of the recording region</returns>
    public override float GetRecordingRegionSize()
    {
        return recordingRegionSizeY;
    }

    #endregion

}
