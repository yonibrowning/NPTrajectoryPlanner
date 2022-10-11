using UnityEngine;
using TrajectoryPlanner;
using UnityEngine.EventSystems;

public class DefaultProbeCollider : MonoBehaviour
{
    [SerializeField] ProbeManager probeManager;
    private TrajectoryPlannerManager tpmanager;

    private void Start()
    {
        tpmanager = GameObject.Find("main").GetComponent<TrajectoryPlannerManager>();
    }

    private void OnDestroy()
    {
        //tpmanager.
    }

    private void OnMouseDown()
    {
        // If someone clicks on a probe, immediately make that the active probe and claim probe control
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        tpmanager.SetActiveProbe(probeManager);
        try{
            ((DefaultProbeController)probeManager.GetProbeController()).DragMovementClick();
        }catch{};
    }

    private void OnMouseDrag()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        try{
            ((DefaultProbeController)probeManager.GetProbeController()).DragMovementDrag();
        }catch{};
    }

    private void OnMouseUp()
    {
        try{
            ((DefaultProbeController)probeManager.GetProbeController()).DragMovementRelease();
        }catch{};
    }
}
