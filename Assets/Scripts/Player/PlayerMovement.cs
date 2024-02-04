using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private static float ACCELERATION_DEGRADATION_SPEED = 5;

    private CharacterController _characterController;

    [Header("Camera")]
    private Transform _cameraTransform;
    private Transform _cameraHolderTransform;
    public float maximumHeadSway = 0.07f;
    public float headSwaySpeed = 5;
    public float maximumHeadBob = 0.1f;
    public float headBobSpeed = 1;
    private float _headBobTime;
    private Vector3 _tempHeadBobDirection;

    //--------------------movement
    [Header("Character")]
    private Vector2 _movementDirection;
    private Vector3 _movementVector;
    private Vector3 _movementSpeedAffectedByAcceleration;
    private Vector3 _movementSpeedGravity;
    private float _currentMovementSpeed;
    public float movementSpeed;
    
    private Vector2 _mouseDelta;
    private Vector2 _headRotation;
    public float mouseSensitivity;

    //--------------------------------------------------general

    private void PlayerAcceleration(float deltaTime)
    {
        _movementSpeedAffectedByAcceleration -= 
            ACCELERATION_DEGRADATION_SPEED * _movementSpeedAffectedByAcceleration * deltaTime;

        //-----------------acceleration
        if (!_characterController.isGrounded)
        {
            //add gravity
            _movementSpeedGravity.y -= deltaTime * 9.81f;
        }
        else
        {
            //make sure when the player is grounded he doesn't go through the floor
            if (_movementSpeedAffectedByAcceleration.y < 0)
            {
                _movementSpeedAffectedByAcceleration.y = 0;
            }

            if (_movementSpeedGravity.y < 0)
            {
                _movementSpeedGravity.y = 0;
            }
        }
    }

    private void UpdatePlayerPosition(float deltaTime)
    {
        PlayerAcceleration(deltaTime);

        //---------------normal movement
        _movementVector = transform.right * _movementDirection.x + transform.forward * _movementDirection.y;
        _movementVector *= deltaTime *_currentMovementSpeed;

        _movementVector += (_movementSpeedAffectedByAcceleration+_movementSpeedGravity) * deltaTime;

        _characterController.Move(_movementVector);
    }

    private void PlayerCameraMouvement(float deltaTime)
    {
        //--------------head turning
        _mouseDelta *= Time.deltaTime * mouseSensitivity;
        _headRotation += _mouseDelta;
        _headRotation.y = Mathf.Clamp(_headRotation.y, -90f, 90f);

        _cameraHolderTransform.localEulerAngles = Vector3.right * -_headRotation.y;
        transform.localEulerAngles = Vector3.up * _headRotation.x;

        CalculateSway(new Vector3(_movementDirection.x, 0, _movementDirection.y), deltaTime);
        CalculateBob(deltaTime);
    }

    //--------------------------------------------------unity events
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
   
        PlayerScore.ResetScore();

        _cameraHolderTransform = transform.Find("CameraAndGunHolder").transform;

        _cameraTransform = _cameraHolderTransform.Find("PlayerCamera").transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _movementDirection = new Vector2();
        _currentMovementSpeed = movementSpeed;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        UpdatePlayerPosition(deltaTime);
        PlayerCameraMouvement(deltaTime);
        CalculateSway(_movementDirection, deltaTime);
    }

    //---------------------------------------------------------------------ui
    private void CalculateSway(Vector3 direction, float deltaTime)
    {
        direction = direction.normalized;

        _cameraTransform.localPosition += (direction * maximumHeadSway - _cameraTransform.localPosition) * headSwaySpeed * deltaTime;
    }

    private void CalculateBob(float deltaTime)
    {
        _headBobTime += deltaTime * headBobSpeed * _characterController.velocity.magnitude;
        if (_headBobTime >= 2*Mathf.PI) { _headBobTime = 0; }

        _tempHeadBobDirection.x = Mathf.Cos(_headBobTime);
        _tempHeadBobDirection.y = Mathf.Sin(2 * _headBobTime);

        _cameraTransform.localPosition += maximumHeadBob*_tempHeadBobDirection;
    }
    
    //----------------------------input events
    public void SetMovementDirectionFunction(InputAction.CallbackContext context)
    {
        _movementDirection = context.ReadValue<Vector2>();
    }
    
    public void SetMouseDeltaFunction(InputAction.CallbackContext context)
    {
        _mouseDelta = context.ReadValue<Vector2>();
    }
}

