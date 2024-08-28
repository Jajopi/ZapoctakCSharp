using UnityEngine;

public class ElevatorScript : MonoBehaviour
{
    public float TargetHeight { get; private set; }
    public float maxSpeed = 2f;
    public float speedMultiplier = 1f;
    public float ActualSpeed { get { return maxSpeed * speedMultiplier; } }

    void Start()
    {
        TargetHeight = 100;
    }

    float GetMoveStep()
    {
        return Mathf.Max(Mathf.Abs(transform.position.y - TargetHeight), ActualSpeed) * Time.deltaTime *
                (transform.position.y > TargetHeight ? -1 : 1);
    }

    void Update()
    {
        if (transform.position.y != TargetHeight)
        {
            transform.position += new Vector3(0,
                                              GetMoveStep(),
                                              0);
        }
    }

    public void ChangeTargetHeight(float newTargetHeight)
    {
        TargetHeight = newTargetHeight;
    } 
}
