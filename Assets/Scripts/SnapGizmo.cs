using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapGizmo : MonoBehaviour {

    public Snap snap
    {
        get
        {
            return new Snap(transform.position, transform.up);
        }
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
