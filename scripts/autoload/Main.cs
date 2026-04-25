using Godot;

public partial class Main : Node
{
	[Export] public PackedScene MainMenuScene { get; set; }

	public override void _Ready()
	{
		LoadScene(MainMenuScene);
	}

	public void LoadScene(PackedScene scene)
	{
		var current = GetNode<Node>("CurrentLocation");
		
		// Remove old scene
		foreach (Node child in current.GetChildren())
			child.QueueFree();
		
		// Load new scene
		if (scene != null)
			current.AddChild(scene.Instantiate());
	}
}
