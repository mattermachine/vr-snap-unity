using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectLibrary : MonoBehaviour
{

    public float librarySwitchAngle = 20;

    private bool visible = false;
    public bool Visible
    {
        get { return visible; }
    }

    // Use this for initialization
	void Start ()
	{
	    Hide();
	}
	
	// Update is called once per frame
	void Update () {

	    // Show library when looking up or pressing spacebar.
	    bool spacebarPressed = Input.GetKeyDown(KeyCode.Space);
        Debug.Log(SnapEngine.mainCamera.transform.eulerAngles.x);
	    bool vrLookingUp = (SnapEngine.mainCamera.transform.eulerAngles.x - 360) < -librarySwitchAngle;
	    vrLookingUp &= SnapEngine.mainCamera.transform.eulerAngles.x > 180;
	    if (visible && spacebarPressed)
	    {
	        Hide();
	    }
	    else if (!SnapEngine.dragging && (vrLookingUp || spacebarPressed))
	    {
	        Show();
	    }
	    else if (visible && !SnapEngine.dragging && !SnapEngine.rayCastSuccess &&
	             (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)))
	    {
	        Hide();
	    }
	}

    public void Hide()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        visible = false;
    }

    public void Show()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        visible = true;
    }
}
