using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioListener))]
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public String botName;
    public TerrainMesh terrainMesh;
    public Image screenShotMask;

    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float jumpSpeed = 8.0f;
    public float flySpeed = 0.1f;
    public float gravity = 20.0f;
    public Camera playerCamera;
    public AudioListener audioListener;
    public float lookSpeed = 2.0f;
    public float lookYLimit = 45.0f;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    float rotationY = 0;

    [HideInInspector]
    public bool canMove = true;

    bool ableToDig = true;

    [Range(1, 10)]
    public int drawRange = 5;

    //Input

    // Flags
    bool startCoroutineF;

    // Others
    public LayerMask playerMask;

    Coroutine c = null;
    public CapsuleCollider capsuleCollider;

    AudioSource audioSource;
    public AudioClip Cam_35mm;
    int JoystickHorizontal;
    int JoystickVertical;
    Vector2 joystickInputVector;
    PlayerInputActions playerInputActions;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // audioListener = GetComponent<AudioListener>();
        characterController = GetComponent<CharacterController>();

        LockCursor();
        ableToDig = true;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }

    // Land on planet initially
    void TryToLand()
    {
        float rayLength = 2000f;

        // actual Ray
        Ray ray = new Ray(transform.position + new Vector3(0, 30f, 0), Vector3.down);

        // debug Ray
        Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.green);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayLength, playerMask))
        {
            transform.position = hit.point + new Vector3(0, 10f, 0);
        }
    }

    private void FixedUpdate()
    {
        // CheckRay();
    }

    private void Update()
    {
        if (!terrainMesh)
            terrainMesh = GameObject.Find("TerrainMesh").GetComponent<TerrainMesh>();

        if (Cursor.lockState == CursorLockMode.None)
        {
            if (PlayerWantsToLockCursor())
                LockCursor();
            Movement(false);
        }
        else if (Cursor.lockState == CursorLockMode.Locked)
        {
            if (PlayerWantsToUnlockCursor())
                UnlockCursor();
            Movement(true);
            // CheckRay();
            CheckScreenShot();
        }
    }

    IEnumerator DiggingCountdown()
    {
        yield return new WaitForEndOfFrame();
        ableToDig = true;
    }

    IEnumerator TakePhoto()
    {
        String desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        String filename =
            desktopPath
            + "/"
            + botName.ToUpper()
            + "_"
            + UnityEngine.Random.Range(100, 1000).ToString()
            + ".png";
        ScreenCapture.CaptureScreenshot(filename, 1);
        yield return new WaitForSeconds(0.05f);
        audioSource.PlayOneShot(Cam_35mm);
        yield return new WaitForSeconds(0.05f);
        Color tempColForMask = screenShotMask.color;
        tempColForMask.a = 1;
        screenShotMask.color = tempColForMask;
        yield return new WaitForSeconds(0.008f);
        while (tempColForMask.a > 0)
        {
            tempColForMask.a -= 0.02f;
            screenShotMask.color = tempColForMask;
            yield return new WaitForSeconds(0.008f);
        }
    }

    private void CheckRay()
    {
        Vector3 rayOrigin = new Vector3(0.5f, 0.5f, 0f); // center of the screen
        float rayLength = 3000f;

        // actual Ray
        Ray ray = Camera.main.ViewportPointToRay(rayOrigin);

        // debug Ray
        Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.red);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayLength))
        {
            // Get Left Mouse Button

            // The direct hit is a chunk
            if (hit.collider.tag == "Chunk")
            {
                if (Input.GetMouseButton(0))
                {
                    if (ableToDig)
                    {
                        terrainMesh.DrawOnChunk(hit.point, drawRange, 0);
                        ableToDig = false;
                        startCoroutineF = true;
                    }
                    else if (startCoroutineF)
                    {
                        RestartCoroutine();
                    }
                }
                // Right Mouse Btn
                else if (Input.GetMouseButton(1))
                {
                    if (ableToDig)
                    {
                        terrainMesh.DrawOnChunk(hit.point, drawRange, 1);
                        NotifyTerrainChanged(hit.point, drawRange);
                        ableToDig = false;
                        startCoroutineF = true;
                    }
                    else if (startCoroutineF)
                    {
                        RestartCoroutine();
                    }
                }
            }
        }
    }

    void RestartCoroutine()
    {
        startCoroutineF = false;
        if (c != null)
        {
            StopCoroutine(c);
        }
        c = StartCoroutine(DiggingCountdown());
    }

    private void CheckScreenShot()
    {
        if (Input.GetKeyDown(KeyCode.F))
            StartCoroutine(TakePhoto());

        if (Input.GetMouseButtonUp(0))
        {
            RestartCoroutine();
        }
    }

    private bool PlayerWantsToLockCursor()
    {
        return (
            (playerInputActions.Player.Return.ReadValue<float>() == 1)
            || (playerInputActions.Player.Movement.ReadValue<Vector2>() != new Vector2(0, 0))
        )
            ? true
            : false;
    }

    private bool PlayerWantsToUnlockCursor()
    {
        return (playerInputActions.Player.Exit.ReadValue<float>() == 1) ? true : false;
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Movement(bool takeControl)
    {
        Vector2 inputVector = new Vector2(0, 0);
        float xDrift = 0;
        float yDrift = 0;
        bool jumpPressed = false;
        bool sprintPressed = false;

        if (takeControl)
        {
            inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
            xDrift += Input.GetAxis("Mouse X");
            xDrift += playerInputActions.Player.Rotation.ReadValue<Vector2>().x * 0.2f;
            yDrift += -Input.GetAxis("Mouse Y");
            sprintPressed = Input.GetKey(KeyCode.LeftShift);
            jumpPressed = (playerInputActions.Player.Jump.ReadValue<float>() == 1) ? true : false;
        }

        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        // Press Left Shift to run
        float curSpeedX = canMove
            ? (sprintPressed ? runningSpeed : walkingSpeed) * inputVector.y
            : 0;
        float curSpeedY = canMove
            ? (sprintPressed ? runningSpeed : walkingSpeed) * inputVector.x
            : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Apply gravity
        if (!characterController.isGrounded)
        {
            moveDirection.y = movementDirectionY;
            moveDirection.y -= gravity * Time.deltaTime;
        }

        if (jumpPressed && canMove)
            moveDirection.y += flySpeed * Time.deltaTime;

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove)
        {
            rotationY += yDrift * lookSpeed;
            rotationY = Mathf.Clamp(rotationY, -lookYLimit, lookYLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationY, 0, 0);
            transform.rotation *= Quaternion.Euler(0, xDrift * lookSpeed, 0);
        }
    }

    public void NotifyTerrainChanged(Vector3 point, float radius)
    {
        float dstFromCam = (point - transform.position).magnitude;
        if (dstFromCam < radius)
        {
            // terraUpdate = true;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
    }
}
