using UnityEngine;

public class LegsScript : MonoBehaviour
{
    PlayerMovement playerMovementScript;
    int objectsTouched = 0;

    void Awake()
    {
        playerMovementScript = transform.parent.GetComponent<PlayerMovement>();
    }

    void OnTriggerEnter(Collider other)
    {
        objectsTouched++;
        playerMovementScript.SetLegsTouchingFloor(objectsTouched > 1);
    }

    void OnTriggerExit(Collider other)
    {
        objectsTouched--;
        playerMovementScript.SetLegsTouchingFloor(objectsTouched > 1);
    }

    void Update()
    {
        
    }
}
