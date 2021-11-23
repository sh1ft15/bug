using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    GameObject _store;

    void Awake() {
        // GameObject[] objs = GameObject.FindGameObjectsWithTag("DontDestroy");

        // if (objs.Length > 1) { Destroy(this.gameObject); }

        if (_store != null) { DontDestroyOnLoad(_store); }
    }


    public void StoreObject(GameObject obj){
        _store = obj;
        DontDestroyOnLoad(_store);
    }

    public GameObject GetObject() { return _store; }
}
