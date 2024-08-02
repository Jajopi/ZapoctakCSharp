using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    Camera playerCamera;
    float horizontalMouseSpeed = 4f;
    float verticalMouseSpeed = 4f;
    float cameraAngleHeight = 0f;

    float forwardMoveSpeed = 5f;
    float sidewaysMoveSpeed = 3f;
    float backwardMoveSpeed = 2f;

    float jumpPower = 500f;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    Vector2 GetStandardViewRotationCoordinates()
    {
        float horizontalMovement = horizontalMouseSpeed * Input.GetAxis("Mouse X");
        float verticalMovement = (-1) * verticalMouseSpeed * Input.GetAxis("Mouse Y");
        return new Vector2(horizontalMovement, verticalMovement);
    }

    void RotateView(Vector2 viewRotationCoordinates)
    {
        transform.Rotate(0, viewRotationCoordinates.x, 0);
        

        if (Mathf.Abs(viewRotationCoordinates.y) > 90)
        {
            return;
        }

        cameraAngleHeight += viewRotationCoordinates.y;
        if (cameraAngleHeight < -90f)
        {
            cameraAngleHeight = -90f;
        }
        if (cameraAngleHeight > 90f)
        {
            cameraAngleHeight = 90f;
        }
        
        Vector3 newViewVector = new Vector3(
            cameraAngleHeight,
            playerCamera.transform.eulerAngles.y,
            playerCamera.transform.eulerAngles.z);

        playerCamera.transform.Rotate(
            newViewVector - playerCamera.transform.eulerAngles);

        /*playerCamera.transform.rotation = Quaternion.LookRotation(
    Vector3.RotateTowards(playerCamera.transform.forward,
        new Vector3(0, cameraAngleHeight, 0),
        Time.deltaTime * lookRotatingSpeedRadians,
        0f)
    );*/
        /*
        playerCamera.transform.eulerAngles = new Vector3(
            Mathf.Clamp(playerCamera.transform.eulerAngles.x, 190f, 350f),
            playerCamera.transform.eulerAngles.y,
            playerCamera.transform.eulerAngles.z);*/

        //Debug.Log(playerCamera.transform.localRotation.eulerAngles.x);
        /*if (playerCamera.transform.eulerAngles.x > 75)
        {
            playerCamera.transform.Rotate((-1) * viewRotationCoordinates.x, 0, 0);
        }
        /*if (playerCamera.transform.eulerAngles.x < 90)
        {
            playerCamera.transform.eulerAngles = new Vector3(
                90,
                playerCamera.transform.eulerAngles.y,
                playerCamera.transform.eulerAngles.z);
        }*/
    }
    /*
    void SetMousePosition(Vector3 screenPosition)
    {
        Mouse.current.WarpCursorPosition(screenPosition);
    }

    void CenterMousePosition()
    {
        SetMousePosition(new Vector3(Screen.width / 2, Screen.height / 2, 0));
    }*/

    Vector3 GetMovementCoordinates()
    {
        float forwardBackwardAxis = Input.GetAxis("Vertical");
        float forwardBackwardMovement = 0;
        if (forwardBackwardAxis < 0)
        {
            forwardBackwardMovement = forwardBackwardAxis * backwardMoveSpeed * Time.deltaTime;
        }
        else
        {
            forwardBackwardMovement = forwardBackwardAxis * forwardMoveSpeed * Time.deltaTime;
        }

        float sidewaysMovement = Input.GetAxis("Horizontal") * sidewaysMoveSpeed * Time.deltaTime;

        return new Vector3(sidewaysMovement, 0, forwardBackwardMovement);
    }

    void MovePlayer(Vector3 movementCoordinates)
    {
        transform.Translate(movementCoordinates);
    }

    bool CanJumpNow()
    {
        return true;
    }

    void TryJump()
    {
        if (Input.GetButton("Jump") && CanJumpNow())
        {
            transform.GetComponent<Rigidbody>().AddForce(new Vector3(0, jumpPower, 0));
        }
    }

    void Update()
    {
        Vector2 viewRotationCoordinates = GetStandardViewRotationCoordinates();
        RotateView(viewRotationCoordinates);

        Vector3 movementCoordinates = GetMovementCoordinates();
        MovePlayer(movementCoordinates);

        TryJump();
    }
}
