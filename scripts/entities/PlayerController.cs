using Godot;

public partial class PlayerController : CharacterBody3D
{
	[Export] public float WalkSpeed { get; set; } = 4.0f;
	[Export] public float SprintSpeed { get; set; } = 7.0f;
	[Export] public float Gravity { get; set; } = 9.8f;
	[Export] public float MouseSensitivity { get; set; } = 0.003f;
	[Export] public float MaxLookAngle { get; set; } = 85.0f;
	[Export] public float MaxStamina { get; set; } = 100.0f;
	[Export] public float StaminaDrainRate { get; set; } = 20.0f;
	[Export] public float StaminaRegenRate { get; set; } = 10.0f;
	[Export] public float StaminaRegenDelay { get; set; } = 2.0f;
	[Export] public float PickupRange { get; set; } = 2.5f;
	[Export] public float InspectRotateSpeed { get; set; } = 0.01f;

	// Node refs
	private Node3D _head;
	private Flashlight _flashlight;
	private RayCast3D _interactRay;
	private Marker3D _holdPoint;
	private Marker3D _inspectPoint;
	private Camera3D _camera;
	private TsdCornerUi _tsdCornerUi;

	// State
	private LootItem _heldItem = null;
	private LootItem _inspectedItem = null;
	private bool _isInspecting = false;
	private bool _isDragging = false;

	private float _currentStamina;
	private float _regenTimer;
	private bool _isSprinting;

	public override void _Ready()
	{
		_head = GetNode<Node3D>("Head");
		_flashlight = GetNode<Flashlight>("Head/FlashLight");
		_interactRay = GetNode<RayCast3D>("Head/InteractRay");
		_holdPoint = GetNode<Marker3D>("Head/HoldPoint");
		_inspectPoint = GetNode<Marker3D>("Head/InspectPoint");
		_camera = GetNode<Camera3D>("Head/Camera3D");

		// TSD UI lives in main UI — find it in the tree
		_tsdCornerUi = GetTree().Root.FindChild("TSDCorner", true, false) as TsdCornerUi;

		_currentStamina = MaxStamina;
		Input.MouseMode = Input.MouseModeEnum.Captured;

		GetTree().Root.FocusExited += () =>
			Input.MouseMode = Input.MouseModeEnum.Visible;

		GetTree().Root.FocusEntered += () =>
		{
			if (!GetTree().Paused)
				Input.MouseMode = Input.MouseModeEnum.Captured;
		};
	}

	public override void _Input(InputEvent @event)
	{
		// ---- INSPECT MODE INPUT ----
		if (_isInspecting)
		{
			HandleInspectInput(@event);
			return;
		}

		// ---- NORMAL MODE INPUT ----
		if (@event is InputEventMouseMotion mouseMotion &&
			Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			GetViewport().SetInputAsHandled();
			RotateY(-mouseMotion.Relative.X * MouseSensitivity);
			float newRotation = _head.RotationDegrees.X
				- mouseMotion.Relative.Y * Mathf.RadToDeg(MouseSensitivity);
			newRotation = Mathf.Clamp(newRotation, -MaxLookAngle, MaxLookAngle);
			_head.RotationDegrees = new Vector3(
				newRotation,
				_head.RotationDegrees.Y,
				_head.RotationDegrees.Z);
		}

		if (@event is InputEventKey key && key.Pressed)
		{
			if (key.Keycode == Key.Escape)
				Input.MouseMode = Input.MouseModeEnum.Visible;

			if (Input.IsActionJustPressed("flashlight"))
				_flashlight?.Toggle();

			if (Input.IsActionJustPressed("grab"))
				HandleGrab();

			if (Input.IsActionJustPressed("drop"))
				HandleDrop();

			if (Input.IsActionJustPressed("interact"))
				HandleInteract();
		}
	}

	private void HandleInspectInput(InputEvent @event)
	{
		// Exit inspect
		if (@event is InputEventKey key && key.Pressed)
		{
			if (key.Keycode == Key.Escape ||
				Input.IsActionJustPressed("interact"))
			{
				ExitInspectEarly();
				return;
			}
		}

		// Left click on TSD corner UI — handled by UI itself via OnTsdClicked()
		// Left click drag to rotate item
		if (@event is InputEventMouseButton mouseBtn)
		{
			if (mouseBtn.ButtonIndex == MouseButton.Left)
				_isDragging = mouseBtn.Pressed;

			// Scan click — only when scan mode active and left clicking
			if (mouseBtn.ButtonIndex == MouseButton.Left &&
				mouseBtn.Pressed &&
				_tsdCornerUi != null &&
				_tsdCornerUi.IsScanModeActive())
			{
				TryClickScanBarcode();
				return;
			}
		}

		// Rotate item with drag
		if (@event is InputEventMouseMotion motion && _isDragging)
		{
			if (_inspectedItem != null)
			{
				_inspectedItem.RotateObjectLocal(
					new Vector3(0, 1, 0),
					-motion.Relative.X * InspectRotateSpeed);
				_inspectedItem.RotateObjectLocal(
					new Vector3(1, 0, 0),
					-motion.Relative.Y * InspectRotateSpeed);
			}
		}
	}

	private void TryClickScanBarcode()
	{
		if (_camera == null || _inspectedItem == null) return;

		// Cast ray from mouse position into 3D
		Vector2 mousePos = GetViewport().GetMousePosition();
		PhysicsRayQueryParameters3D rayParams = PhysicsRayQueryParameters3D.Create(
			_camera.ProjectRayOrigin(mousePos),
			_camera.ProjectRayOrigin(mousePos) +
			_camera.ProjectRayNormal(mousePos) * 10f);

		var spaceState = GetWorld3D().DirectSpaceState;
		var result = spaceState.IntersectRay(rayParams);

		if (result.Count > 0)
		{
			GodotObject hit = result["collider"].AsGodotObject();
			GD.Print("Clicked on: ", hit);

			// Check if we hit the barcode mesh or its parent item
			Node hitNode = hit as Node;
			if (hitNode != null && (hitNode == _inspectedItem ||
				hitNode.GetParent() == _inspectedItem ||
				hitNode.Name.ToString().ToLower().Contains("barcode")))
			{
				ScanSuccess();
			}
			else
			{
				GD.Print("Missed barcode — rotate item to find it");
			}
		}
	}

	private void ScanSuccess()
	{
		_inspectedItem.Scan();
		_tsdCornerUi?.PlayScanSuccess();
		GD.Print("BEEP — scanned: ", _inspectedItem.ItemName);

		// After scan, item goes to hands automatically
		LootItem scanned = _inspectedItem;
		_inspectedItem = null;
		_isInspecting = false;
		_isDragging = false;

		Input.MouseMode = Input.MouseModeEnum.Captured;
		_tsdCornerUi?.Hide();

		// Put in hands
		scanned.StopInspect(GetTree().CurrentScene as Node3D);
		CallDeferred(nameof(PickUpAfterScan), scanned);
	}

	private void PickUpAfterScan(LootItem item)
	{
		_heldItem = item;
		_heldItem.PickUp(_holdPoint);
		GD.Print("Item now in hands");
	}

	private void HandleInteract()
	{
		if (_interactRay.IsColliding())
		{
			GodotObject collider = _interactRay.GetCollider();
			if (collider is LootItem item && !item.IsHeld && !item.IsScanned)
				EnterInspect(item);
		}
	}

	private void EnterInspect(LootItem item)
	{
		_isInspecting = true;
		_inspectedItem = item;
		_isDragging = false;

		item.StartInspect(_inspectPoint);

		// Show cursor for inspect interaction
		Input.MouseMode = Input.MouseModeEnum.Visible;

		// Show TSD corner UI
		_tsdCornerUi?.ShowForInspect();

		GD.Print("Inspect mode: ", item.ItemName);
	}

	private void ExitInspectEarly()
	{
		if (_inspectedItem == null) return;

		Node3D worldParent = GetTree().CurrentScene as Node3D;
		_inspectedItem.StopInspect(worldParent);

		_inspectedItem = null;
		_isInspecting = false;
		_isDragging = false;

		Input.MouseMode = Input.MouseModeEnum.Captured;
		_tsdCornerUi?.Hide();

		GD.Print("Exited inspect");
	}

	private void HandleGrab()
	{
		if (_isInspecting) return;

		if (_heldItem != null)
		{
			HandleDrop();
			return;
		}

		if (_interactRay.IsColliding())
		{
			GodotObject collider = _interactRay.GetCollider();
			if (collider is LootItem item && !item.IsHeld && item.IsScanned)
			{
				_heldItem = item;
				_heldItem.PickUp(_holdPoint);
				GD.Print("Picked up: ", _heldItem.ItemName);
			}
			else if (collider is LootItem unscanned && !unscanned.IsScanned)
			{
				GD.Print("Scan item first");
			}
		}
	}

	private void HandleDrop()
	{
		if (_heldItem == null) return;
		Node3D worldParent = GetTree().CurrentScene as Node3D;
		_heldItem.Drop(worldParent);
		_heldItem = null;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isInspecting)
		{
			Velocity = Vector3.Zero;
			return;
		}

		float dt = (float)delta;
		Vector3 velocity = Velocity;

		if (!IsOnFloor())
			velocity.Y -= Gravity * dt;

		_isSprinting = Input.IsActionPressed("sprint") && _currentStamina > 0;
		HandleStamina(dt);

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

	public float GetStaminaPercent() => _currentStamina / MaxStamina;

	public override void _ExitTree()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest || what == NotificationCrash)
			Input.MouseMode = Input.MouseModeEnum.Visible;
	}
}
