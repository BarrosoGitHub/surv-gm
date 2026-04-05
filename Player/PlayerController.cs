using UnityEngine;

public class PlayerController : Controller
{
    private PlayerControls inputControls;

    [SerializeField] private Transform cam;

    private Vector2 inputDirection;
    private Vector3 horizontalDirection;
    private Vector3 moveDirection;

    public bool isGrounded;
    public bool IsGrounded
    {
        get
        {
            return isGrounded;
        }
        set
        {
            if (isGrounded == value) return;

            isGrounded = value;
        }
    }

    public State movingState;

    public LayerMask groundMask;

    protected override void Awake()
    {
        base.Awake();

        inputControls = new PlayerControls();
    }

    public override void Start()
    {
        SetStates();
        base.Start();

        
    }

    private void SetStates()
    {
        movingState = new State();

        State = movingState;

    }
    
    public override void SetOnEnterState()
    {
       
    }

    public override void SetOnExitState()
    {
        
    }

    public override void SetOnUpdateState()
    {
        movingState.OnUpdate += () =>
        {
            MovementDirection();
        };
    }

    public override void SetOnFixedUpdateState()
    {
        movingState.OnFixedUpdate += () =>
        {
            PhysicsUpdate(new Vector3(0.95f, 1, 0.95f), entity.stats.MaxSpeed);
        };
    }

    private void Update()
    {
        State.OnUpdate?.Invoke();

        ManageInput();
    }

    private void FixedUpdate()
    {
        State.OnFixedUpdate?.Invoke();
    }

    private void PhysicsUpdate(Vector3 drag, float speed)
    {
        // Ground Check
        var velocityVector = Mathf.Clamp01(rb.linearVelocity.y - 1f) / 5 * Vector3.up; // temp fix?
        IsGrounded = Physics.CheckSphere(transform.position - new Vector3(0, 1, 0) + velocityVector, 0.275f, groundMask);

        rb.AddForce(10f * speed * moveDirection, ForceMode.Acceleration);

        Vector3 velocity = rb.linearVelocity;
        velocity.x *= drag.x;
        velocity.y *= drag.y;
        velocity.z *= drag.z;
        rb.linearVelocity = velocity;
    }

    private void ManageInput()
    {
        inputDirection = inputControls.Player.Move.ReadValue<Vector2>();
        inputDirection = Vector2.ClampMagnitude(inputDirection, 1f);

        horizontalDirection = new Vector3(inputDirection.x, 0f, inputDirection.y).normalized;
    }

    private void MovementDirection()
    {
        float angle = Mathf.Atan2(horizontalDirection.x, horizontalDirection.z) * Mathf.Rad2Deg + cam.eulerAngles.y;

        if (horizontalDirection.magnitude >= 0.1f)
        {
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            moveDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;
        }
        else
        {
            moveDirection = Vector3.zero;
        }
    }

    private void OnEnable()
    {
        inputControls.Enable();
    }

    private void OnDisable()
    {
        inputControls.Disable();
    }
}
