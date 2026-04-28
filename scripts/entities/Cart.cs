using Godot;
using System.Collections.Generic;

public partial class Cart : RigidBody3D
{
	[Export] public int MaxItems { get; set; } = 10;
	[Export] public float FollowDistance { get; set; } = 1.4f;
	[Export] public float FollowSpeed { get; set; } = 15f;

	private Area3D _loadArea;
	private List<LootItem> _loadedItems = new List<LootItem>();

	private bool _isGrabbed = false;
	private CharacterBody3D _grabber = null;

	public int ItemCount => _loadedItems.Count;
	public List<LootItem> LoadedItems => _loadedItems;

	public override void _Ready()
	{
		_loadArea = GetNode<Area3D>("LoadArea");
		_loadArea.BodyEntered += OnBodyEntered;

		Mass = 20f;
		AngularDamp = 20f;
		LinearDamp = 5f;
	}

	private void OnBodyEntered(Node3D body)
	{
		if (body is LootItem item && !_loadedItems.Contains(item))
		{
			if (_loadedItems.Count >= MaxItems)
			{
				GD.Print("Cart full!");
				return;
			}
			_loadedItems.Add(item);
			GD.Print($"Item added: {item.ItemName} ({_loadedItems.Count}/{MaxItems})");
		}
	}

	public void RemoveItem(LootItem item)
	{
		if (_loadedItems.Contains(item))
			_loadedItems.Remove(item);
	}

	public void Grab(CharacterBody3D grabber)
	{
		_isGrabbed = true;
		_grabber = grabber;

		// Switch to kinematic while grabbed so we control position directly
		FreezeMode = FreezeModeEnum.Kinematic;
		Freeze = true;

		GD.Print("Cart grabbed");
	}

	public void Release()
	{
		_isGrabbed = false;

		// Return to full physics
		Freeze = false;

		// Small velocity so it doesn't teleport
		LinearVelocity = Vector3.Zero;
		AngularVelocity = Vector3.Zero;

		_grabber = null;
		GD.Print("Cart released");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_isGrabbed || _grabber == null) return;

		// Target position — directly in front of player
		Vector3 forward = -_grabber.GlobalTransform.Basis.Z;
		Vector3 targetPos = _grabber.GlobalPosition + forward * FollowDistance;
		targetPos.Y = GlobalPosition.Y; // stay on floor

		// Smoothly move to target
		GlobalPosition = GlobalPosition.Lerp(targetPos, (float)delta * FollowSpeed);

		// Rotate to match player facing
		Vector3 targetRot = new Vector3(0, _grabber.Rotation.Y, 0);
		Rotation = Rotation.Lerp(targetRot, (float)delta * FollowSpeed);
	}

	public bool HasItem(string barcodeId)
		=> _loadedItems.Exists(i => i.BarcodeId == barcodeId);

	public bool IsEmpty() => _loadedItems.Count == 0;
}
