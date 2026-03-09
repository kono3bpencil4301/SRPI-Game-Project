using System;
using Godot;

[GlobalClass]
public partial class DialogueManager : Control
{
	[Export] public Label character_name_text { get; set; }
	[Export] public Label text_box { get; set; }
	[Export] public TextureRect role_avatar { get; set; }
	[Export] public DialogueGroup main_dialogue { get; set; }

	private int dialogue_index = 0;

	public override void _Ready()
	{
		SetProcessInput(true); // 确保接收输入事件
		DisplayNextDialogue();
	}

	public void DisplayNextDialogue()
	{
		if (main_dialogue == null)
		{
			GD.PrintErr("DialogueManager: main_dialogue is null");
			return;
		}

		if (main_dialogue.Dialogues == null)
		{
			GD.PrintErr("DialogueManager: main_dialogue.Dialogues is null");
			return;
		}

		if (main_dialogue.Dialogues.Count == 0)
		{
			GD.PrintErr("DialogueManager: main_dialogue.Dialogues is empty");
			return;
		}

		if (dialogue_index < 0 || dialogue_index >= main_dialogue.Dialogues.Count)
		{
			GD.PrintErr($"DialogueManager: dialogue_index out of range: {dialogue_index}");
			return;
		}

		var dialogue = main_dialogue.Dialogues[dialogue_index];
		if (dialogue == null)
		{
			GD.PrintErr($"DialogueManager: dialogue at index {dialogue_index} is null");
			return;
		}

		if (character_name_text != null)
			character_name_text.Text = dialogue.CharacterName;

		if (text_box != null)
			text_box.Text = dialogue.Content;

		if (role_avatar != null)
			role_avatar.Texture = dialogue.Avatar;

		dialogue_index++;
	}
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
			{
				DisplayNextDialogue();
			}
		}
	}
}