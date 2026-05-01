using Godot;
using System;
using System.Collections.Generic;


public partial class TaskManager(Node3D target) : Area3D
{
    public int IDOfShelves { get; private set; } // the ID of the shelves the player needs to walk to (write after name of Node3D)
    public bool HasTask { get; private set; } = false; // whether the player has a task to complete
    private float _radius = 20.0f; // radius of the task choosing area

    private Node3D _playerNode = target; // the target node, can add on any objects
    private Node3D marker;
    public Node3D TargetShelves { get; private set; }

    private List<Node3D> _detectedShelves = new List<Node3D>(); // list of detected shelves

    private Random rnd = new Random(); // to choose a random shelf from the detected list

    private PackedScene _markerScene; // the marker scene to instantiate when adding a marker

    public override void _Ready()
    {
        ConfigureMarker(); // configure marker that will be used to mark the target shelves
        CreateCircle(); // create the circle shape on target node
    }

    public void GetNewTask() // first detect shelves, then choose a random one to mark as the target
    {
        DetectShelves();
        if (_detectedShelves.Count == 0)
        {
            GD.Print("No detected shelves");
            return;
        }
        int index = rnd.Next(_detectedShelves.Count); // choose a random index from the detected shelves
        AddMarker(_detectedShelves[index]); // add a marker to the detected shelf
        TargetShelves = _detectedShelves[index];
        IDOfShelves = int.Parse(_detectedShelves[index].Name.ToString().Split('_')[1]); // parse the shelf ID from the node name
        GD.Print("Walk to shelf " + IDOfShelves); // debug where the player needs to walk
        HasTask = true;
    }

    public void TaskCompleted() // after completing the task, remove the marker and clear the detected shelves
    {
        HasTask = false;
        marker.GetParent()?.RemoveChild(marker);
        TargetShelves = null;
        _detectedShelves.Clear();
    }

    private void DetectShelves() // detect shelves within the task choosing area
    {
        _detectedShelves.Clear();
        foreach (var body in GetOverlappingBodies()) // iterate through all overlapping bodies and add any shelves to the detected list
        {
            if (body is Node3D node && node.GetParent()?.Name.ToString().StartsWith("Shelves") == true) // check if the body is a "Shelves" and add it to the detected list
            {
                _detectedShelves.Add(node.GetParent() as Node3D);
            }
        }
    }

    private void CreateCircle() // create a circle shape for the task choosing area and add on target node
    {
        CollisionShape3D _collisionShape = new CollisionShape3D();
        SphereShape3D _sphere = new SphereShape3D();
        _sphere.Radius = _radius;
        _collisionShape.Shape = _sphere;

        AddChild(_collisionShape);

    }

    private void AddMarker(Node3D body) // check if the body already has a marker, and remove it if so, then add a new marker to the body
    {
        marker.GetParent()?.RemoveChild(marker);
        marker.Position = body.Position;
        body.AddChild(marker);
        marker.Position = Vector3.Zero;
    }

    private void ConfigureMarker() // load prefab of marker
    {
        _markerScene = GD.Load<PackedScene>("res://assets/prefab/TaskMark.tscn"); // go to res://assets/prefab/TaskMark.tscn for config marker
        marker = _markerScene.Instantiate<Node3D>();
    }

}
