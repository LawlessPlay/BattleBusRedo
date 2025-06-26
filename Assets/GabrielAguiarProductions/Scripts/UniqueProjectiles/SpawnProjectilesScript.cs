//
//NOTES:
//This script is used for DEMONSTRATION porpuses of the Projectiles. I recommend everyone to create their own code for their own projects.
//This is just a basic example.
//

using System.Collections;
using System.Collections.Generic;
using TacticsToolkit;
using UnityEngine;
using UnityEngine.UI;

public class SpawnProjectilesScript : MonoBehaviour {

    public bool useTarget;
	public bool use2D;
	public bool cameraShake;
	public Text effectName;
	public RotateToMouseScript rotateToMouse;
	public GameObject firePoint;
	public GameObject cameras;
    public GameObject target;
    public List<GameObject> VFXs = new List<GameObject> ();

	private int count = 0;
	private float timeToFire = 0f;
	private GameObject effectToSpawn;
	private List<Camera> camerasList = new List<Camera> ();
	private Camera singleCamera;

	public GameEvent triggerAction;

	void Start () {

		if (cameras.transform.childCount > 0) {
			for (int i = 0; i < cameras.transform.childCount; i++) {
				camerasList.Add (cameras.transform.GetChild (i).gameObject.GetComponent<Camera> ());
			}
			if(camerasList.Count == 0){
				Debug.Log ("Please assign one or more Cameras in inspector");
			}
		} else {
			singleCamera = cameras.GetComponent<Camera> ();
			if (singleCamera != null)
				camerasList.Add (singleCamera);
			else
				Debug.Log ("Please assign one or more Cameras in inspector");
		}

		if(VFXs.Count>0)
			effectToSpawn = VFXs[0];
		else
			Debug.Log ("Please assign one or more VFXs in inspector");
		
		if (effectName != null) effectName.text = effectToSpawn.name;

		if (camerasList.Count > 0) {
			rotateToMouse.SetCamera (camerasList [camerasList.Count - 1]);
			if(use2D)
				rotateToMouse.Set2D (true);
			rotateToMouse.StartUpdateRay ();
		}
		else
			Debug.Log ("Please assign one or more Cameras in inspector");

        if (useTarget && target != null)
        {
            var collider = target.GetComponent<BoxCollider>();
            if (!collider)
            {
                target.AddComponent<BoxCollider>();
            }
        }
    }

	public void SpawnVFX () {
		GameObject vfx;

		var cameraShakeScript = cameras.GetComponent<CameraShakeSimpleScript> ();

		if (cameraShake && cameraShakeScript != null)
			cameraShakeScript.ShakeCamera ();

		if (firePoint != null) {
			var testTransform = new Vector3(firePoint.transform.position.x, firePoint.transform.position.y+2, firePoint.transform.position.z);
			vfx = Instantiate (effectToSpawn, testTransform, Quaternion.identity);
            if (!useTarget)
            {
                if (rotateToMouse != null)
                {
                    vfx.transform.localRotation = rotateToMouse.GetRotation();
                }
                else Debug.Log("No RotateToMouseScript found on firePoint.");
            }
            else
            {
                if (target != null)
                {
	                var targetTransform = new Vector3(target.transform.position.x, target.transform.position.y+2, target.transform.position.z);
                    vfx.GetComponent<ProjectileMoveScript>().SetTarget(target, rotateToMouse, triggerAction);
                }
                else
                {
                    Destroy(vfx);
                    Debug.Log("No target assigned.");
                }
            }
		}
		else
			vfx = Instantiate (effectToSpawn);		
	}
	
	public void SetEffectToSpawn(GameObject effectToSpawn)
	{
		if (effectToSpawn == null) return;
		
		this.effectToSpawn = effectToSpawn;
		SpawnVFX();
	}
	
	public void SetTarget(GameObject location)
	{
		var position = new Vector3(location.transform.position.x, location.transform.position.y + 0.5f, location.transform.position.z);
		this.target.transform.position = position;
	}

	public void SetFirepoint(GameObject firePoint)
	{
		this.firePoint = firePoint;
	}
}
