using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigController : MonoBehaviour
{
    private static RigController _instance;
    public static RigController Instance {get{return _instance;}}

    //Ensure Singleton
    //https://gamedev.stackexchange.com/questions/116009/in-unity-how-do-i-correctly-implement-the-singleton-pattern
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    void SetRigCenter(Vector3 new_center){
        this.gameObject.transform.position = new_center;
    }

    public Vector3 GetRigCenter(){
        return this.gameObject.transform.position;
    }

}
