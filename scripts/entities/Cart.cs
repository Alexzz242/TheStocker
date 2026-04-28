using Godot;
using System.Collections.Generic;

public partial class Cart : RigidBody3D
{
	[Export] public int MaxItems { get; set; } = 10;

	private Area3D _loadArea;
	private Node3D _itemContainer;
	private List<LootItem> _loadedItems = new List<LootItem>();

	// Cart grab state
	private bool _isGrabbed = false;
	private Node3D _grabber = null;

	public int ItemCount => _loadedItems.Count;
	public List<LootItem> LoadedItems => _loadedItems;

	public override void _Ready()
	{
		_loadArea = GetNode<Area3D>("LoadArea");
		_itemContainer = GetNode<Node3D>("ItemContainer");

		_loadArea.BodyEntered += OnBodyEntered;
		_loadArea.BodyExited += OnBodyExited;

		// Cart sits on floor, moderate friction
		PhysicsMaterialOverride = new PhysicsMaterial();
		PhysicsMaterialOverride.Friction = 0.8f;
		PhysicsMaterialOverride.Rough = true;
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
			GD.Print($"Item added to cart: {item.ItemName} ({_loadedItems.Count}/{MaxItems})");
		}
	}

	private void OnBodyExited(Node3D body)
	{
		if (body is LootItem item && _loadedItems.Contains(item))
		{
			_loadedItems.Remove(item);
			GD.Print($"Item removed from cart: {item.ItemName} ({_loadedItems.Count}/{MaxItems})");
		}
	}

	public void Grab(Node3D grabber)
	{
		_isGrabbed = true;
		_grabber = grabber;

		// Reduce mass while grabbed so it feels pushable
		Mass = 5f;
		GD.Print("Cart grabbed");
	}

	public void Release()
	{
		_isGrabbed = false;
		_grabber = null;

		// Restore mass
		Mass = 20f;
		GD.Print("Cart released");

		// Kill velocity so cart doesn't slide away
		LinearVelocity = Vector3.Zero;
		AngularVelocity = Vector3.Zero;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_isGrabbed || _grabber == null) return;

		// Target point is in front of the grabber, not at the grabber
		Vector3 grabPos = _grabber.GlobalPosition +
			(-_grabber.GlobalTransform.Basis.Z * 1.5f);
		 grabPos.Y = GlobalPosition.Y;

		Vector3 direction = grabPos - GlobalPosition;
		float distance = direction.Length();

		if (distance > 0.05f)
		{
			// Apply force toward grabber
			LinearVelocity = direction.Normalized() * Mathf.Min(distance * 8f, 6f);
		}
		else
		{
			LinearVelocity = Vector3.Zero;
		}

		// Prevent cart from rotating while grabbed
		AngularVelocity = Vector3.Zero;
	}

	public bool HasItem(string barcodeId)
	{
		return _loadedItems.Exists(item => item.BarcodeId == barcodeId);
	}

	public bool IsEmpty() => _loadedItems.Count == 0;
}
