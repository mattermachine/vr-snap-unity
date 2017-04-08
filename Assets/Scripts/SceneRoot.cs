using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneRoot : MonoBehaviour
{
    public float rotationSpeed = .25f;
    private Vector3 previousMousePosition;
    private Vector3 rotationCenter;
    private float rotationY;
    private bool rotating = false;

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

	// Called from SnapEngine.
	public bool Rotate ()
	{
	    if (Input.GetMouseButtonDown(2))
	    {
	        if (snapEngine.rayCastSuccess)
	        {
	            rotationCenter = snapEngine.hitPoint;
	        }
	        else
	        {
	            rotationCenter = Vector3.zero;
	        }
	        previousMousePosition = Input.mousePosition;
	        rotating = true;
	    }

	    if (rotating)
	    {
	        rotationY = rotationSpeed * (Input.mousePosition.x - previousMousePosition.x);
	        previousMousePosition = Input.mousePosition;
	        transform.RotateAround(rotationCenter,Vector3.up,rotationY);
	        // Transform construction plane identically.
	        snapEngine.constructionPlane.transform.position = transform.position;
	        snapEngine.constructionPlane.transform.rotation = transform.rotation;
	    }

	    if (Input.GetMouseButtonUp(2))
	    {
	        rotating = false;
	    }

	    return rotating;
	}
}
