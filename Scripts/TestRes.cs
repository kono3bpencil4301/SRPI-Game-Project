using Godot;

[GlobalClass]
public partial class TestRes : Resource
{
    [Export]
    public string Name { get; set; } = "";

    public TestRes() { }
}