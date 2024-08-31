using UnityEngine;
using UnityEngine.InputSystem;
using GameEvents;

public class PlayerMovement : MonoBehaviour
{
    bool movementEnabled;

    Transform playerHead;
    float horizontalMouseSpeed = 4f;
    float verticalMouseSpeed = 4f;
    float cameraAngleHeight = 0f;

    float forwardMoveSpeed = 5f;
    float sidewaysMoveSpeed = 3f;
    float backwardMoveSpeed = 2f;

    float jumpPower = 300f;
    bool canJump = false;
    float jumpCooldown = 0.1f;
    float jumpCooldownRemainingTime;

    float sendTransformEvery_seconds = 0.1f;
    float sendTransformTimer = 0f;
    GameTaskObject gameTask;

    float interactionDistance = 4f;

    void Start()
    {
        movementEnabled = true;

        playerHead = transform.Find("Head").Find("Camera");

        gameTask = gameObject.GetComponent<GameTaskObject>();

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
            playerHead.eulerAngles.y,
            playerHead.transform.eulerAngles.z);

        playerHead.transform.Rotate(
            newViewVector - playerHead.transform.eulerAngles);
    }

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

    public void SetLegsTouchingFloor(bool touching)
    {
        if (canJump == touching)
        {
            return;
        }
        canJump = touching;
        jumpCooldownRemainingTime = jumpCooldown;
    }

    void Jump()
    {
        Rigidbody rigidbody = transform.GetComponent<Rigidbody>();

        rigidbody.linearVelocity = new Vector3(rigidbody.linearVelocity.x,
                                               0,
                                               rigidbody.linearVelocity.z);

        rigidbody.AddForce(new Vector3(0,
                                       jumpPower,
                                       0));
    }

    void TryJump()
    {
        if (Input.GetButton("Jump") && canJump && jumpCooldownRemainingTime <= 0)
        {
            Jump();
            jumpCooldownRemainingTime = jumpCooldown;
        }
    }

    void PerformUpdateMovement()
    {
        Vector2 viewRotationCoordinates = GetStandardViewRotationCoordinates();
        RotateView(viewRotationCoordinates);

        Vector3 movementCoordinates = GetMovementCoordinates();
        MovePlayer(movementCoordinates);

        TryJump();
    }

    void TrySendTransform()
    {
        sendTransformTimer += Time.deltaTime;
        if (sendTransformTimer > sendTransformEvery_seconds)
        {
            sendTransformTimer -= sendTransformEvery_seconds;

            gameTask.SendTransformUpdate();
        }
    }

    void TryInteract()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Mouse0))
        {
            RaycastHit hit;
            if (Physics.Raycast(playerHead.position, playerHead.TransformDirection(Vector3.forward), out hit, interactionDistance))
            {
                GameTaskObject gameTaskObject = hit.collider.gameObject.GetComponent<GameTaskObject>();
                if (gameTaskObject is not null)
                {
                    gameTaskObject.Activate();
                }
            }
        }
    }

    void Update()
    {
        jumpCooldownRemainingTime -= Time.deltaTime;

        if (movementEnabled)
        {
            PerformUpdateMovement();
        }

        TrySendTransform();

        TryInteract();

        if (transform.position.y < -5)
        {
            transform.position = new Vector3(0, 0, 0);
        }
    }

    void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    void OnTriggerEnter(Collider other)
    {
        GameTaskObject gameTaskObject = other.gameObject.GetComponent<GameTaskObject>();
        if (gameTaskObject is not null)
        {
            gameTaskObject.ActivateOnTouch();
        }
    }
}
