using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneRoot : MonoBehaviour
{
    private Vector3 mouseDownPosition;
    private Vector3 mouseDownRotation;
    private Vector3 rotationCenter;
    public float rotationSpeed = .25f;
    private Vector3 rotation;
    public static bool rotating = false;

    private SnapEngine snapEngine
    {
        get
        {
            return SnapEngine.singleton;
        }
    }

	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
//	    if (snapEngine.rayCastSuccess) return;

	    if (Input.GetMouseButtonDown(0))
	    {
	        if (snapEngine.rayCastSuccess)
	        {
	            rotationCenter = snapEngine.hitPoint;
	        }
	        else
	        {
	            rotationCenter = Vector3.zero;
	        }
	        mouseDownPosition = Input.mousePosition;
	        mouseDownRotation = transform.localEulerAngles;
	        rotating = true;
	    }

	    if (rotating)
	    {
//	        rotation = mouseDownRotation + Vector3.up * rotationSpeed * (Input.mousePosition.x - mouseDownPosition.x);
	        rotation = Vector3.up * rotationSpeed * (Input.mousePosition.x - mouseDownPosition.x);
//	        transform.localEulerAngles = rotation;
	        transform.RotateAround(rotationCenter,Vector3.up,rotation.y);
	        // Transform construction plane identically.
	        snapEngine.constructionPlane.transform.position = transform.position;
	        snapEngine.constructionPlane.transform.rotation = transform.rotation;
	    }

	    if (Input.GetMouseButtonUp(0))
	    {
	        rotating = false;
	    }
	}
}
