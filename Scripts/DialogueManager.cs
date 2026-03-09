using System;
using System.Collections.Generic;
using Godot;

namespace SRPI
{
    // JSON 对话节点结构
    public class DialogueNodeJson
    {
        public string Character { get; set; } = "";
        public string Avatar { get; set; } = "";
        public string Content { get; set; } = "";
        public string Next { get; set; }
        public List<DialogueOptionJson> Options { get; set; }
    }

    // JSON 选项结构
    public class DialogueOptionJson
    {
        public string Text { get; set; } = "";
        public string Opinion { get; set; } = "";
        public string Next { get; set; } = "";
    }

    // JSON 对话文件根结构
    public class DialogueJson
    {
        public string DialogueId { get; set; } = "";
        public string StartNode { get; set; } = "";
        public Dictionary<string, DialogueNodeJson> Nodes { get; set; }
    }

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
        private DialogueJson json_data;
        private string current_node_id = "";
        private bool has_options = false;

        public override void _Ready()
        {
            SetProcessInput(true);

            // 判断使用哪种模式
            if (!string.IsNullOrEmpty(json_file_path))
            {
                LoadJsonDialogue();
            }

            // 延迟一帧调用，确保子节点已准备好
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

                // 使用 Godot JSON 解析
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

                json_data = ParseDialogueJson(rawData);
                if (json_data == null || json_data.Nodes == null)
                {
                    GD.PrintErr("DialogueManager: Failed to parse JSON");
                    return;
                }

                current_node_id = json_data.StartNode;
                GD.Print($"DialogueManager: Loaded JSON dialogue: {json_data.DialogueId}");
            }
            catch (Exception e)
            {
                GD.PrintErr($"DialogueManager: Error loading JSON: {e.Message}");
            }
        }

        private DialogueJson ParseDialogueJson(Godot.Collections.Dictionary data)
        {
            var dialogue = new DialogueJson();

            if (data.ContainsKey("dialogue_id"))
                dialogue.DialogueId = data["dialogue_id"].AsString();
            if (data.ContainsKey("start_node"))
                dialogue.StartNode = data["start_node"].AsString();

            if (data.ContainsKey("nodes"))
            {
                var nodesVariant = data["nodes"];
                var nodesDict = (Godot.Collections.Dictionary)nodesVariant;
                dialogue.Nodes = new Dictionary<string, DialogueNodeJson>();
                foreach (var key in nodesDict.Keys)
                {
                    string nodeId = key.ToString();
                    var nodeData = nodesDict[key];
                    var nodeDict = (Godot.Collections.Dictionary)nodeData;

                    var node = new DialogueNodeJson();
                    if (nodeDict.ContainsKey("character"))
                        node.Character = nodeDict["character"].AsString();
                    if (nodeDict.ContainsKey("avatar"))
                        node.Avatar = nodeDict["avatar"].AsString();
                    if (nodeDict.ContainsKey("content"))
                        node.Content = nodeDict["content"].AsString();
                    if (nodeDict.ContainsKey("next"))
                        node.Next = nodeDict["next"].AsString();

                    if (nodeDict.ContainsKey("options"))
                    {
                        var optionsVariant = nodeDict["options"];
                        var optionsArr = (Godot.Collections.Array)optionsVariant;
                        node.Options = new List<DialogueOptionJson>();
                        foreach (var opt in optionsArr)
                        {
                            var optDict = (Godot.Collections.Dictionary)opt;
                            var option = new DialogueOptionJson();
                            if (optDict.ContainsKey("text"))
                                option.Text = optDict["text"].AsString();
                            if (optDict.ContainsKey("opinion"))
                                option.Opinion = optDict["opinion"].AsString();
                            if (optDict.ContainsKey("next"))
                                option.Next = optDict["next"].AsString();
                            node.Options.Add(option);
                        }
                    }

                    dialogue.Nodes[nodeId] = node;
                }
            }

            return dialogue;
        }

        public void DisplayNextDialogue()
        {
            // JSON 模式
            DisplayJsonDialogue();
        }

        private void DisplayJsonDialogue()
        {
            if (json_data == null || json_data.Nodes == null || !json_data.Nodes.ContainsKey(current_node_id))
            {
                GD.PrintErr($"DialogueManager: Invalid node: {current_node_id}");
                Visible = false;
                return;
            }

            var node = json_data.Nodes[current_node_id];
            if (node == null)
            {
                Visible = false;
                return;
            }

            // 设置角色名和头像
            if (character_name_text != null)
                character_name_text.Text = node.Character;

            if (role_avatar != null && !string.IsNullOrEmpty(node.Avatar))
            {
                var avatarTexture = GD.Load<Texture2D>(node.Avatar);
                role_avatar.Texture = avatarTexture;
            }

            // 显示文本内容
            DisplayTextContent(node.Content);

            // 处理选项
            ClearOptions();
            if (node.Options != null && node.Options.Count > 0)
            {
                has_options = true;
                CreateOptions(node.Options);
            }
        }

        private void DisplayTextContent(string content)
        {
            // Animate text typing effect
            if (text_box != null && text_box.IsInsideTree())
            {
                text_box.Text = content;
                text_box.VisibleRatio = 0.0f;
                is_typing = true;

                typing_tween?.Kill();
                typing_tween = GetTree().CreateTween();

                float duration = content.Length * 0.05f;
                typing_tween.TweenProperty(text_box, "visible_ratio", 1.0f, duration);
                typing_tween.TweenCallback(Callable.From(() => { is_typing = false; }));
            }
            else if (RichTextBox != null && RichTextBox.IsInsideTree())
            {
                RichTextBox.BbcodeEnabled = true;

                // 检查内容是否包含格式标签
                bool hasColorTag = content.Contains("[color=") || content.Contains("[colour=");
                bool hasFontSizeTag = content.Contains("[font_size=");

                // 如果没有格式标签，设置 RichTextLabel 的默认样式
                if (!hasColorTag)
                {
                    RichTextBox.AddThemeColorOverride("default_color", default_text_color);
                }
                if (!hasFontSizeTag)
                {
                    RichTextBox.AddThemeFontSizeOverride("normal_font_size", default_font_size);
                }

                // 处理文本格式：仅添加缩进
                string processedText = ProcessRichText(content);
                RichTextBox.Text = processedText;
                RichTextBox.VisibleRatio = 0.0f;
                is_typing = true;

                typing_tween?.Kill();
                typing_tween = GetTree().CreateTween();

                float duration = content.Length * 0.05f;
                typing_tween.TweenProperty(RichTextBox, "visible_ratio", 1.0f, duration);
                typing_tween.TweenCallback(Callable.From(() => { is_typing = false; }));
            }
            else
            {
                GD.PrintErr("DialogueManager: Neither text_box nor RichTextBox is in the scene tree");
            }
        }

        private void CreateOptions(List<DialogueOptionJson> options)
        {
            if (options_container == null)
            {
                GD.PrintErr("DialogueManager: options_container is null");
                return;
            }

            // 设置按钮间距
            if (options_container is VBoxContainer vbox)
            {
                vbox.AddThemeConstantOverride("separation", (int)option_button_spacing);
            }

            foreach (var option in options)
            {
                Button btn;
                if (option_button_scene != null)
                {
                    btn = option_button_scene.Instantiate<Button>();
                }
                else
                {
                    btn = new Button();
                }

                // 优先使用 Opinion 字段作为按钮文本，否则使用 Text 字段
                string buttonText = !string.IsNullOrEmpty(option.Opinion) ? option.Opinion : option.Text;
                btn.Text = buttonText;

                // 设置按钮字体大小
                if (option_button_font_size > 0)
                {
                    btn.AddThemeFontSizeOverride("font_size", option_button_font_size);
                }

                // 设置按钮字体
                if (OptionButtonFont != null)
                {
                    btn.AddThemeFontOverride("font", OptionButtonFont);
                }

                // 设置按钮尺寸（无论是否使用预制体都生效）
                btn.CustomMinimumSize = new Vector2(option_button_width, option_button_height);
                // 设置按钮可以扩展
                btn.SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill;

                // 使用 lambda 捕获 option.Next
                string nextNode = option.Next;
                btn.Pressed += () => OnOptionSelected(nextNode);
                options_container.AddChild(btn);
            }
        }

        private void ClearOptions()
        {
            if (options_container == null) return;

            foreach (var child in options_container.GetChildren())
            {
                if (child is Button btn)
                {
                    btn.QueueFree();
                }
            }
        }

        private void OnOptionSelected(string nextNodeId)
        {
            if (string.IsNullOrEmpty(nextNodeId))
            {
                // 结束对话
                Visible = false;
                return;
            }

            current_node_id = nextNodeId;
            has_options = false;
            DisplayNextDialogue();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent)
            {
                if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
                {
                    // 有选项时，点击不推进对话
                    if (has_options) return;

                    if (is_typing)
                    {
                        typing_tween?.Kill();
                        if (text_box != null)
                            text_box.VisibleRatio = 1.0f;
                        else if (RichTextBox != null)
                            RichTextBox.VisibleRatio = 1.0f;
                        is_typing = false;
                    }
                    else
                    {
                        // JSON 模式：通过 next 字段跳转
                        if (json_data != null && json_data.Nodes.ContainsKey(current_node_id))
                        {
                            var node = json_data.Nodes[current_node_id];
                            if (!string.IsNullOrEmpty(node.Next))
                            {
                                current_node_id = node.Next;
                                DisplayNextDialogue();
                            }
                            else
                            {
                                Visible = false;
                            }
                        }
                    }
                }
            }
        }

        private static string ProcessRichText(string content)
        {
            // 添加缩进（使用空格）
            string indent = new(' ', 4); // 4个英文字母 ≈ 2个汉字宽度
            return $"{indent}{content}";
        }
    }
}