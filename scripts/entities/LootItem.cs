using Godot;

public partial class LootItem : RigidBody3D
{
	[Export] public string ItemName { get; set; } = "Unknown Item";
	[Export] public string BarcodeId { get; set; } = "000000";

	private bool _isHeld = false;
	private bool _isScanned = false;
	private Node3D _worldParent = null;

	public bool IsHeld => _isHeld;
	public bool IsScanned => _isScanned;

	public void PickUp(Node3D holdPoint)
	{
		_isHeld = true;

		// Disable physics while held
		Freeze = true;
		CollisionLayer = 0;
		CollisionMask = 0;

		// Reparent to hold point
		Node currentParent = GetParent();
		currentParent.RemoveChild(this);
		holdPoint.AddChild(this);

		// Sit at hold point center
		Position = Vector3.Zero;
		Rotation = Vector3.Zero;
	}

	public void Drop(Node3D worldParent)
	{
		_isHeld = false;
		_worldParent = worldParent;

		// Defer the actual drop to avoid mid-frame physics issues
		CallDeferred(nameof(FinalizeDrop));
	}

	private void FinalizeDrop()
	{
	Vector3 globalPos = GlobalPosition;

	Node currentParent = GetParent();
	currentParent.RemoveChild(this);
	_worldParent.AddChild(this);

	GlobalPosition = globalPos;

	// Layer 2 = loot (value 2), collide with layer 4 = environment (value 8)
	Freeze = false;
	CollisionLayer = 2;
	CollisionMask = 8;

	_worldParent = null;
	}

	public void Scan()
	{
		_isScanned = true;
		GD.Print($"Scanned: {ItemName} — ID: {BarcodeId}");
	}
}
