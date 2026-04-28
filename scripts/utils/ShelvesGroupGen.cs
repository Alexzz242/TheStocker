using Godot;
public static class ShelvesGroupGen
{
    public static void CreateShelvesGroup(Node _scene, string _objectsPrefix = "Shelves_")
    {
        foreach (Node _child in _scene.GetChildren())
        {
            if (_child.Name.ToString().StartsWith(_objectsPrefix))
            {
                _child.AddToGroup("ShelvesGroup");
            }
        }
    }
}
