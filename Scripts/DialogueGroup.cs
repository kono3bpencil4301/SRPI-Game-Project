using Godot;

[GlobalClass]
public partial class DialogueGroup : Resource
{
    [Export]
    public Godot.Collections.Array<DialogueLine> Dialogues { get; set; } = new();

    public DialogueGroup() { }
}