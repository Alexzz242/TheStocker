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

	public override void _Ready()
	{
		// Layer 2 = loot, Mask 8 = layer 4 (environment)
		CollisionLayer = 2;
		CollisionMask = 8;
	}

	public void PickUp(Node3D holdPoint)
	{
		_isHeld = true;
		Freeze = true;
		CollisionLayer = 0;
		CollisionMask = 0;

		Node currentParent = GetParent();
		currentParent.RemoveChild(this);
		holdPoint.AddChild(this);

		Position = Vector3.Zero;
		Rotation = Vector3.Zero;
	}

	public void Drop(Node3D worldParent)
	{
		_isHeld = false;
		_worldParent = worldParent;
		CallDeferred(nameof(FinalizeDrop));
	}

	private void FinalizeDrop()
	{
		Vector3 globalPos = GlobalPosition;

		Node currentParent = GetParent();
		currentParent.RemoveChild(this);
		_worldParent.AddChild(this);

		GlobalPosition = globalPos;

		// Restore collision then unfreeze
		CollisionLayer = 2;
		CollisionMask = 8;
		Freeze = false;

		_worldParent = null;
	}

	public void Scan()
	{
		_isScanned = true;
		GD.Print($"Scanned: {ItemName} — ID: {BarcodeId}");
	}
}
