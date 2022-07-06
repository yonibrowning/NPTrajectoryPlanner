using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProbeSelector : MonoBehaviour
{
    private TrajectoryPlannerManager tpmanager;
    [SerializeField] private GameObject buttonPrefab;

    [SerializeField] private List<GameObject> buttonList;

    // Start is called before the first frame update
    void Start()
    {
        GameObject main = GameObject.Find("main");
        tpmanager = main.GetComponent<TrajectoryPlannerManager>();
    }

    public void AddButton()
    {
        GameObject newButton = GameObject.Instantiate(buttonPrefab, this.transform);
    }
}
