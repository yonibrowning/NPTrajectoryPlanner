using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using Dummiesman;
using UnityEngine.UI;

public class RigObjLoader : MonoBehaviour
{
    [SerializeField] private GameObject rigPartParent;
    [SerializeField] private TP_ToggleRigs toggleRigs;
    [SerializeField] private TMP_InputField objectFileLocation;
    [SerializeField] private GameObject toggleObjectTemplate;
    [SerializeField] private GameObject rigMenuGrid;
    [SerializeField] private GameObject bregma;
 
    [SerializeField] private Material material;
    public void LoadNew(){
        if (!File.Exists(objectFileLocation.text)){
            Debug.LogError("File " +objectFileLocation.text + " does not exist and cannot be loaded." );
        }else{
            //Load the game object
            GameObject loadedObject  = new OBJLoader().Load(objectFileLocation.text);


            //Add to the rig components list/object
            loadedObject.transform.SetParent(rigPartParent.transform);
            loadedObject.layer = 7; //Add to rig layer
            foreach (Transform childTransform in loadedObject.transform){
                childTransform.gameObject.layer = 7;
                childTransform.gameObject.GetComponent<MeshRenderer>().material = material;
                childTransform.gameObject.AddComponent<MeshCollider>();
            }
            toggleRigs.AddRigGO(loadedObject);
            loadedObject.transform.Rotate(new Vector3(0,-90,0));
            loadedObject.transform.position = bregma.transform.position;
            
            
            //Create a button
            GameObject loadedToggleObject = GameObject.Instantiate(toggleObjectTemplate);
            loadedToggleObject.transform.SetParent(rigMenuGrid.transform,false);            
            loadedToggleObject.GetComponentInChildren<TMP_Text>().text = Path.GetFileName(objectFileLocation.text);
            Toggle toggle = loadedToggleObject.GetComponent<Toggle>() as Toggle;
            toggle.isOn = true;

            toggle.onValueChanged.AddListener(delegate{toggleRigs.ToggleRigVisibility(toggleRigs.GetRigListLength()-1);});
        }
    }
}
