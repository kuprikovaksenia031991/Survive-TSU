using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CameraDoorScript
{
public class CameraOpenDoor : MonoBehaviour {
	public float DistanceOpen=3;
	public GameObject text;

	private PlayerInputs playerInput;
	// Use this for initialization
	void Start () {
		
	}
	void Awake()
    {
        playerInput = new PlayerInputs();
    }

    void OnEnable() { playerInput.OnFoot.Enable(); }
    void OnDisable() { playerInput.OnFoot.Disable(); }

	// Update is called once per frame
    void Update () {
		RaycastHit hit;
		if (Physics.Raycast (transform.position, transform.forward, out hit, DistanceOpen)) {
                
				var door = hit.transform.GetComponent<DoorScript.Door>();
				if (door != null) {
					text.SetActive (true);
					if (playerInput.OnFoot.Interact.triggered)
					{
					    door.OpenDoor();
					}
                }
                else{
				text.SetActive (false);
			}
		}else{
			text.SetActive (false);
		}
	}
}
}
