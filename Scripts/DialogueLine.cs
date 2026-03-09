using Godot;

[GlobalClass]
public partial class DialogueLine : Resource
{
	[Export]
	public string CharacterName { get; set; } = "";

	[Export(PropertyHint.MultilineText)]
	public string Content { get; set; } = "";

	[Export]
	public Texture2D Avatar { get; set; }

	public DialogueLine() { }
}
