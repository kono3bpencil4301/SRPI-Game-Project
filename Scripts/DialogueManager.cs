using System;
using System.Collections.Generic;
using Godot;

namespace SRPI
{
    [GlobalClass]
    public partial class DialogueManager : Control
    {
        // UI 组件
        [Export] public Label character_name_text { get; set; }
        [Export] public Label text_box { get; set; }
        [Export] public RichTextLabel RichTextBox { get; set; }
        [Export] public TextureRect role_avatar { get; set; }
        [Export] public VBoxContainer options_container { get; set; }
        [Export] public PackedScene option_button_scene { get; set; }

        // 按钮尺寸配置
        [Export] public float option_button_width = 400;
        [Export] public float option_button_height = 80;
        [Export] public float option_button_spacing = 10;
        [Export] public int option_button_font_size = 24;
        [Export] public Font OptionButtonFont { get; set; }

        // JSON 文件路径
        [Export] public string json_file_path { get; set; } = "";

        // 样式配置
        [Export] public Color default_text_color = Colors.White;
        [Export] public int default_font_size = 24;
        [Export] public int indent_pixels = 40;

        // 内部状态
        private Tween typing_tween;
        private bool is_typing = false;

        // 数据格式枚举
        private enum DataFormat { FiveLayer, Flat, Old }
        private DataFormat current_format = DataFormat.FiveLayer;

        // 数据引用
        private FiveLayerData five_layer_data;
        private DialogueData current_dialogue;
        private DialogueJson json_data;
        private FlatDialogueJson flat_json_data;
        private string current_node_id = "";
        private bool has_options = false;

        public override void _Ready()
        {
            SetProcessInput(true);
            if (!string.IsNullOrEmpty(json_file_path))
                LoadJsonDialogue();
            CallDeferred(nameof(DisplayNextDialogue));
        }

        private void LoadJsonDialogue()
        {
            try
            {
                string path = json_file_path;
                if (!FileAccess.FileExists(path))
                {
                    GD.PrintErr($"DialogueManager: JSON file not found: {path}");
                    return;
                }

                using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
                string json_text = file.GetAsText();

                var parser = new Json();
                Error error = parser.Parse(json_text);
                if (error != Error.Ok)
                {
                    GD.PrintErr($"DialogueManager: JSON parse error: {parser.GetErrorMessage()}");
                    return;
                }

                var rawData = (Godot.Collections.Dictionary)parser.Data;
                if (rawData == null)
                {
                    GD.PrintErr("DialogueManager: Invalid JSON format");
                    return;
                }

                // 检测 JSON 格式并解析
                if (rawData.ContainsKey("scenes"))
                {
                    five_layer_data = DialogueJsonParser.ParseFiveLayerJson(rawData);
                    current_format = DataFormat.FiveLayer;
                    InitFirstDialogue(five_layer_data?.Dialogues);
                    GD.Print($"DialogueManager: Loaded five-layer dialogue: {current_dialogue?.DialogueId}");
                }
                else if (rawData.ContainsKey("dialogue_nodes"))
                {
                    flat_json_data = DialogueJsonParser.ParseFlatDialogueJson(rawData);
                    current_format = DataFormat.Flat;
                    current_node_id = flat_json_data.Dialogues[0].StartNode;
                    GD.Print($"DialogueManager: Loaded flat JSON dialogue: {flat_json_data.Dialogues[0].DialogueId}");
                }
                else
                {
                    json_data = DialogueJsonParser.ParseDialogueJson(rawData);
                    current_format = DataFormat.Old;
                    current_node_id = json_data.StartNode;
                    GD.Print($"DialogueManager: Loaded JSON dialogue: {json_data.DialogueId}");
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"DialogueManager: Error loading JSON: {e.Message}");
            }
        }

        private void InitFirstDialogue(List<DialogueData> dialogues)
        {
            if (dialogues == null || dialogues.Count == 0) return;
            current_dialogue = dialogues[0];
            current_node_id = current_dialogue.StartNodeId;
        }

        public void DisplayNextDialogue() => DisplayJsonDialogue();

        private void DisplayJsonDialogue()
        {
            string speaker = "", avatar = "", content = "", next = "";
            List<OptionData> options = null;

            switch (current_format)
            {
                case DataFormat.FiveLayer:
                    var fiveLayerNode = five_layer_data?.Nodes?.Find(n => n.NodeId == current_node_id);
                    if (fiveLayerNode == null) { Visible = false; return; }
                    speaker = fiveLayerNode.Speaker;
                    avatar = fiveLayerNode.Avatar;
                    content = fiveLayerNode.Content;
                    next = fiveLayerNode.NextNodeId ?? "";
                    var fiveLayerOptions = five_layer_data?.Options?.FindAll(o => o.BranchNodeId == current_node_id);
                    if (fiveLayerOptions?.Count > 0) options = fiveLayerOptions;
                    break;

                case DataFormat.Flat:
                    int nodeIndex = ParseNodeIndex(current_node_id);
                    if (nodeIndex >= 0 && flat_json_data?.DialogueNodes != null && nodeIndex < flat_json_data.DialogueNodes.Count)
                    {
                        var node = flat_json_data.DialogueNodes[nodeIndex];
                        speaker = node.Character;
                        avatar = node.Avatar;
                        content = node.Content;
                        next = node.Next ?? "";
                        var nodeOptions = flat_json_data.DialogueOptions?.FindAll(o => o.NodeId == current_node_id);
                        if (nodeOptions?.Count > 0)
                            options = nodeOptions.ConvertAll(o => new OptionData { BranchNodeId = o.NodeId, Text = !string.IsNullOrEmpty(o.Opinion) ? o.Opinion : o.Text, NextNodeId = o.Next });
                    }
                    break;

                case DataFormat.Old:
                    if (json_data?.Nodes == null || !json_data.Nodes.ContainsKey(current_node_id)) { Visible = false; return; }
                    var oldNode = json_data.Nodes[current_node_id];
                    if (oldNode == null) { Visible = false; return; }
                    speaker = oldNode.Character;
                    avatar = oldNode.Avatar;
                    content = oldNode.Content;
                    next = oldNode.Next ?? "";
                    break;
            }

            // 更新 UI
            if (character_name_text != null) character_name_text.Text = speaker;
            UpdateAvatar(avatar);
            DisplayTextContent(content);
            ClearOptions();
            if (options != null && options.Count > 0) { has_options = true; CreateFiveLayerOptions(options); }
        }

        private void UpdateAvatar(string avatar)
        {
            if (role_avatar == null) return;
            if (string.IsNullOrEmpty(avatar) || string.Equals(avatar, "NULL", StringComparison.OrdinalIgnoreCase))
                role_avatar.Visible = false;
            else
            {
                var texture = GD.Load<Texture2D>(avatar);
                if (texture != null) { role_avatar.Texture = texture; role_avatar.Visible = true; }
                else { role_avatar.Visible = false; GD.PrintErr($"DialogueManager: Failed to load avatar: {avatar}"); }
            }
        }

        private void DisplayTextContent(string content)
        {
            if (text_box != null && text_box.IsInsideTree())
            {
                text_box.Text = content;
                text_box.VisibleRatio = 0.0f;
                is_typing = true;
                typing_tween?.Kill();
                typing_tween = GetTree().CreateTween();
                typing_tween.TweenProperty(text_box, "visible_ratio", 1.0f, content.Length * 0.05f);
                typing_tween.TweenCallback(Callable.From(() => { is_typing = false; }));
            }
            else if (RichTextBox != null && RichTextBox.IsInsideTree())
            {
                RichTextBox.BbcodeEnabled = true;
                bool hasColorTag = content.Contains("[color=") || content.Contains("[colour=");
                bool hasFontSizeTag = content.Contains("[font_size=");
                if (!hasColorTag) RichTextBox.AddThemeColorOverride("default_color", default_text_color);
                if (!hasFontSizeTag) RichTextBox.AddThemeFontSizeOverride("normal_font_size", default_font_size);
                RichTextBox.Text = ProcessRichText(content);
                RichTextBox.VisibleRatio = 0.0f;
                is_typing = true;
                typing_tween?.Kill();
                typing_tween = GetTree().CreateTween();
                typing_tween.TweenProperty(RichTextBox, "visible_ratio", 1.0f, content.Length * 0.05f);
                typing_tween.TweenCallback(Callable.From(() => { is_typing = false; }));
            }
            else
                GD.PrintErr("DialogueManager: Neither text_box nor RichTextBox is in the scene tree");
        }

        private void CreateFiveLayerOptions(List<OptionData> options)
        {
            if (options_container == null) return;
            if (options_container is VBoxContainer vbox) vbox.AddThemeConstantOverride("separation", (int)option_button_spacing);

            foreach (var option in options)
            {
                var btn = CreateOptionButton(option.Text, option.NextNodeId);
                options_container.AddChild(btn);
            }
        }

        private Button CreateOptionButton(string text, string nextNodeId)
        {
            Button btn = option_button_scene?.Instantiate<Button>() ?? new Button();
            btn.Text = text;
            if (option_button_font_size > 0) btn.AddThemeFontSizeOverride("font_size", option_button_font_size);
            if (OptionButtonFont != null) btn.AddThemeFontOverride("font", OptionButtonFont);
            btn.CustomMinimumSize = new Vector2(option_button_width, option_button_height);
            btn.SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
            string next = nextNodeId;
            btn.Pressed += () => OnOptionSelected(next);
            return btn;
        }

        private void ClearOptions()
        {
            if (options_container == null) return;
            foreach (var child in options_container.GetChildren())
                if (child is Button btn) btn.QueueFree();
        }

        private void OnOptionSelected(string nextNodeId)
        {
            if (string.IsNullOrEmpty(nextNodeId)) { Visible = false; return; }
            current_node_id = nextNodeId;
            has_options = false;
            DisplayNextDialogue();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (has_options) return;
                if (is_typing)
                {
                    typing_tween?.Kill();
                    if (text_box != null) text_box.VisibleRatio = 1.0f;
                    else if (RichTextBox != null) RichTextBox.VisibleRatio = 1.0f;
                    is_typing = false;
                }
                else
                {
                    string nextNode = GetCurrentNodeNext();
                    if (!string.IsNullOrEmpty(nextNode)) { current_node_id = nextNode; DisplayNextDialogue(); }
                    else Visible = false;
                }
            }
        }

        private string GetCurrentNodeNext()
        {
            switch (current_format)
            {
                case DataFormat.FiveLayer:
                    var node1 = five_layer_data?.Nodes?.Find(n => n.NodeId == current_node_id);
                    return node1?.NextNodeId ?? "";

                case DataFormat.Flat:
                    int idx = ParseNodeIndex(current_node_id);
                    if (idx >= 0 && flat_json_data?.DialogueNodes != null && idx < flat_json_data.DialogueNodes.Count)
                        return flat_json_data.DialogueNodes[idx].Next ?? "";
                    break;

                case DataFormat.Old:
                    if (json_data?.Nodes?.ContainsKey(current_node_id) == true)
                        return json_data.Nodes[current_node_id].Next ?? "";
                    break;
            }
            return "";
        }

        private static int ParseNodeIndex(string nodeId)
        {
            if (nodeId.StartsWith("node_") && int.TryParse(nodeId.Replace("node_", ""), out int idx))
                return idx - 1;
            return 0;
        }

        private static string ProcessRichText(string content) => $"    {content}";
    }
}
