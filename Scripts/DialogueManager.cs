using System;
using Godot;

namespace SRPI
{
    [GlobalClass]
    public partial class DialogueManager : Control
    {
        [Export] public Label character_name_text { get; set; }
        [Export] public Label text_box { get; set; }
        [Export] public TextureRect role_avatar { get; set; }
        [Export] public DialogueGroup main_dialogue { get; set; }

        private int dialogue_index = 0;
        private Tween typing_tween;
        private bool is_typing = false;

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

            if (dialogue_index >= main_dialogue.Dialogues.Count)
            {
                Visible = false;
                return;
            }

            var dialogue = main_dialogue.Dialogues[dialogue_index];
            if (dialogue == null)
            {
                GD.PushWarning($"DialogueManager: dialogue at index {dialogue_index} is null, skipping.");
                dialogue_index++;
                DisplayNextDialogue();
                return;
            }

            if (character_name_text != null)
                character_name_text.Text = dialogue.CharacterName;

            if (role_avatar != null)
                role_avatar.Texture = dialogue.Avatar;

            // Animate text typing effect
            if (text_box != null)
            {
                text_box.Text = dialogue.Content;
                text_box.VisibleRatio = 0.0f;
                is_typing = true;

                typing_tween?.Kill();
                typing_tween = GetTree().CreateTween();

                float duration = dialogue.Content.Length * 0.05f;
                typing_tween.TweenProperty(text_box, "visible_ratio", 1.0f, duration);
                typing_tween.TweenCallback(Callable.From(() => { is_typing = false; }));
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent)
            {
                if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
                {
                    if (is_typing)
                    {
                        typing_tween?.Kill();
                        if (text_box != null)
                            text_box.VisibleRatio = 1.0f;
                        is_typing = false;
                    }
                    else
                    {
                        dialogue_index++;
                        DisplayNextDialogue();
                    }
                }
            }
        }
    }
}