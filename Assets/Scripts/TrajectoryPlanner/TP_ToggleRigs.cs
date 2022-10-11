using System.Collections.Generic;
using UnityEngine;
using TrajectoryPlanner;

public class TP_ToggleRigs : MonoBehaviour
{
    [SerializeField] TrajectoryPlannerManager tpmanager;

    // Exposed the list of rigs
    [SerializeField] List<GameObject> rigGOs;

    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject go in rigGOs)
            go.SetActive(false);
    }

    public void ToggleRigVisibility(int rigIdx)
    {
        rigGOs[rigIdx].SetActive(!rigGOs[rigIdx].activeSelf);
        Collider[] colliders = rigGOs[rigIdx].transform.GetComponentsInChildren<Collider>();
        tpmanager.UpdateRigColliders(colliders, rigGOs[rigIdx].activeSelf);
    }

    public void AddRigGO(GameObject newRigGO){
        rigGOs.Add(newRigGO);
        //New obj will be active; add collider tracking
        Collider[] colliders = newRigGO.transform.GetComponentsInChildren<Collider>();
        tpmanager.UpdateRigColliders(colliders, true);
    }

    public int GetRigListLength(){
        return rigGOs.Count;
    }
}
