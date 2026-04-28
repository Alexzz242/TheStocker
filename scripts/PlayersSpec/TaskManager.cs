using Godot;
using System.Collections.Generic;


public partial class TaskManager : Area3D
{
	private Node3D _targetNode;
	private List<Node3D> _detectedShelves;
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

	}

	private CollisionShape3D CreateCircle()
	{
		CollisionShape3D _collisionShape = new CollisionShape3D();
		SphereShape3D _sphere = new SphereShape3D();
		_sphere.Radius = _radius;
		_collisionShape.Shape = _sphere;

		return _collisionShape;
	}


}
