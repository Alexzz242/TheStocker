using Godot;
using System.Collections.Generic;


public partial class TaskManager : Area3D
{
	private Node3D _targetNode;
	private List<Node3D> _detectedShelves = new List<Node3D>();
	private float _radius = 10.0f;

	public TaskManager(Node3D target)
	{
		_targetNode = target;
	}

	public void GetNewTask()
	{

	}

	private void DetectShelves(CollisionShape3D _sphere)
	{
		AddChild(_sphere);
		BodyEntered += OnBodyEntered;

	}

	private CollisionShape3D CreateCircle()
	{
		CollisionShape3D _collisionShape = new CollisionShape3D();
		SphereShape3D _sphere = new SphereShape3D();
		_sphere.Radius = _radius;
		_collisionShape.Shape = _sphere;

		return _collisionShape;
	}

	private void OnBodyEntered(Node3D body)
	{
	    if(body.Name.ToString().StartsWith("Shelves"))
	    {
	        _detectedShelves.Add(body);
	    }
	}
}
