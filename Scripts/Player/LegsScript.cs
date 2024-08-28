using UnityEngine;

public class LegsScript : MonoBehaviour
{
    PlayerMovement playerMovementScript;
    int objectsTouched = 0;

    void Start()
    {
        playerMovementScript = transform.parent.GetComponent<PlayerMovement>();
    }

    void OnTriggerEnter(Collider other)
    {
        objectsTouched++;
    }

    void OnTriggerExit(Collider other)
    {
        objectsTouched--;
    }

    void Update()
    {
        playerMovementScript.SetLegsTouchingFloor(objectsTouched > 1);
    }
}
