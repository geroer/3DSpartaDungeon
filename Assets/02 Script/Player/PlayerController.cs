using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float runSpeed;
    private float addSpeed;
    public float jumpPower;
    private Vector2 curMovementInput;
    public LayerMask groundLayerMask;
    public float runStamina;
    public float jumpStamina;
    private bool isRun = false;

    [Header("Look")]
    public Transform cameraContainer;
    public float minXLook;
    public float maxXLook;
    private float camCurXRot;
    public float lookSensitivity;
    private Vector2 mouseDelta;
    public bool canLook = true;
    public float minZoom;
    public float maxZoom;
    public float minZoomY;
    public float maxZoomY;
    public float minZoomX;
    public float maxZoomX;
    public float zoomSpeed;
    private float currentZoom = 0f;

    public Action inventory;
    private Rigidbody _rigidbody;
    private bool isSpeedBoosted = false;
    public float boostDuration;
    private Interaction interaction;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        interaction = GetComponent<Interaction>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        Vector3 newCameraPos = cameraContainer.localPosition;
        newCameraPos.z = -currentZoom;
        newCameraPos.y = Mathf.Lerp(minZoomY, maxZoomY, (currentZoom - minZoom) / (maxZoom - minZoom));
        newCameraPos.x = Mathf.Lerp(minZoomX, maxZoomX, (currentZoom - minZoom) / (maxZoom - minZoom));
        cameraContainer.localPosition = Vector3.Lerp(cameraContainer.localPosition, newCameraPos, Time.deltaTime * zoomSpeed);
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void LateUpdate()
    {
        if (canLook)
        {
            CameraLook();
        }
    }

    void Move()
    {
        Vector3 dir = transform.forward * curMovementInput.y + transform.right * curMovementInput.x;
        dir *= moveSpeed + addSpeed;
        dir.y = _rigidbody.velocity.y;

        _rigidbody.velocity = dir;

        if (isRun)
        {
            CharacterManager.Instance.Player.condition.UseStamina(runStamina);

            if (CharacterManager.Instance.Player.condition.uiCondition.stamina.curValue <= 0)
            {
                addSpeed = 0;
                isRun = false;
            }
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            curMovementInput = context.ReadValue<Vector2>();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            curMovementInput = Vector2.zero;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (CharacterManager.Instance.Player.condition.uiCondition.stamina.curValue <= 0) return;

        if (context.phase == InputActionPhase.Started && IsGrounded())
        {
            _rigidbody.AddForce(Vector2.up * jumpPower, ForceMode.Impulse);
            CharacterManager.Instance.Player.condition.UseStamina(jumpStamina);
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (CharacterManager.Instance.Player.condition.uiCondition.stamina.curValue <= 0)
        {
            addSpeed = 0;
            isRun = false;
        }

        if (context.phase == InputActionPhase.Performed)
        {
            addSpeed = runSpeed;
            isRun = true;
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            addSpeed = 0;
            isRun = false;
        }
    }

    bool IsGrounded()
    {
        Ray[] ray = new Ray[4]
        {
            new Ray(transform.position + (transform.forward * 0.2f) + (transform.up * 0.01f), Vector3.down),
            new Ray(transform.position + (-transform.forward * 0.2f) + (transform.up * 0.01f), Vector3.down),
            new Ray(transform.position + (transform.right * 0.2f) + (transform.up * 0.01f), Vector3.down),
            new Ray(transform.position + (-transform.right * 0.2f) + (transform.up * 0.01f), Vector3.down)
        };

        for (int i = 0; i < ray.Length; i++)
        {
            if (Physics.Raycast(ray[i], 0.1f, groundLayerMask))
            {
                return true;
            }
        }

        return false;
    }

    void CameraLook()
    {
        camCurXRot += mouseDelta.y * lookSensitivity;
        camCurXRot = Mathf.Clamp(camCurXRot, minXLook, maxXLook);
        cameraContainer.localEulerAngles = new Vector3(-camCurXRot, 0, 0);

        transform.eulerAngles += new Vector3(0, mouseDelta.x * lookSensitivity, 0);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>();
    }

    public void BoostSpeed(float amount)
    {
        if (!isSpeedBoosted)
        {
            StartCoroutine(SpeedBoostCoroutine(amount, boostDuration));
        }
    }

    IEnumerator SpeedBoostCoroutine(float amount, float duration)
    {
        isSpeedBoosted = true;
        moveSpeed += amount;
        yield return new WaitForSeconds(duration);
        moveSpeed -= amount;
        isSpeedBoosted = false;
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Started)
        {
            inventory?.Invoke();
            ToggleCursor();
        }
    }

    void ToggleCursor()
    {
        bool toggle = Cursor.lockState == CursorLockMode.Locked;
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
        canLook = !toggle;
    }

    public void OnCameraZoom(InputAction.CallbackContext context)
    {
        float scroolInput = context.ReadValue<float>();

        if (scroolInput > 0 )
        {
            currentZoom--;
            interaction.maxCheckDistance--;
        }
        else if (scroolInput < 0)
        {
            currentZoom++;
            interaction.maxCheckDistance++;
        }

        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
    }
}
