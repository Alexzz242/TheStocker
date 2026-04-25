using Godot;

public partial class PlayerController : CharacterBody3D
{
	// Movement
	[Export] public float WalkSpeed { get; set; } = 4.0f;
	[Export] public float SprintSpeed { get; set; } = 7.0f;
	[Export] public float Gravity { get; set; } = 9.8f;

	// Mouse look
	[Export] public float MouseSensitivity { get; set; } = 0.003f;
	[Export] public float MaxLookAngle { get; set; } = 85.0f;

	// Stamina
	[Export] public float MaxStamina { get; set; } = 100.0f;
	[Export] public float StaminaDrainRate { get; set; } = 20.0f;
	[Export] public float StaminaRegenRate { get; set; } = 10.0f;
	[Export] public float StaminaRegenDelay { get; set; } = 2.0f;

	// Private
	private Node3D _head;
	private float _currentStamina;
	private float _regenTimer;
	private bool _isSprinting;

	public override void _Ready()
	{
		_head = GetNode<Node3D>("Head");
		_currentStamina = MaxStamina;
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		// Mouse look
		if (@event is InputEventMouseMotion mouseMotion &&
			Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			// Left/right — rotate whole body
			RotateY(-mouseMotion.Relative.X * MouseSensitivity);

			// Up/down — rotate head only
			float newRotation = _head.RotationDegrees.X
				- mouseMotion.Relative.Y * Mathf.RadToDeg(MouseSensitivity);
			newRotation = Mathf.Clamp(newRotation, -MaxLookAngle, MaxLookAngle);
			_head.RotationDegrees = new Vector3(
				newRotation,
				_head.RotationDegrees.Y,
				_head.RotationDegrees.Z);
		}

		// Release mouse during dev
		if (@event is InputEventKey key && key.Pressed &&
			key.Keycode == Key.Escape)
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		Vector3 velocity = Velocity;

		// Gravity
		if (!IsOnFloor())
			velocity.Y -= Gravity * dt;

		// Sprint check
		_isSprinting = Input.IsActionPressed("sprint") && _currentStamina > 0;

		// Stamina
		HandleStamina(dt);

		// Movement input
		Vector2 inputDir = Input.GetVector(
			"move_left", "move_right",
			"move_forward", "move_back");

		Vector3 direction = (Transform.Basis *
			new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		float speed = _isSprinting ? SprintSpeed : WalkSpeed;

		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * speed;
			velocity.Z = direction.Z * speed;
		}
		else
		{
			// Snappy stop
			velocity.X = 0;
			velocity.Z = 0;
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	private void HandleStamina(float delta)
	{
		bool isMoving = Input.GetVector(
			"move_left", "move_right",
			"move_forward", "move_back") != Vector2.Zero;

		if (_isSprinting && isMoving)
		{
			_currentStamina -= StaminaDrainRate * delta;
			_currentStamina = Mathf.Max(_currentStamina, 0);
			_regenTimer = 0;
		}
		else
		{
			_regenTimer += delta;
			if (_regenTimer >= StaminaRegenDelay)
			{
				_currentStamina += StaminaRegenRate * delta;
				_currentStamina = Mathf.Min(_currentStamina, MaxStamina);
			}
		}
	}

	// Other systems read stamina through this
	public float GetStaminaPercent() => _currentStamina / MaxStamina;
}
