using System;
using System.Collections.Generic;

namespace SRPI
{
    // ========== 五层结构数据类 ==========

    // Scene（场景）
    public class SceneData
    {
        public string SceneId { get; set; } = "";
        public string SceneName { get; set; } = "";
        public string BgPath { get; set; } = "";
        public string BgmPath { get; set; } = "";
        public string Description { get; set; } = "";
    }

    // Event（事件）
    public class EventData
    {
        public string EventId { get; set; } = "";
        public string SceneId { get; set; } = "";
        public string EventName { get; set; } = "";
        public string TriggerType { get; set; } = "";
        public string TriggerCondition { get; set; } = "";
        public string StartDialogueId { get; set; } = "";
        public int Priority { get; set; }
    }

    // Dialogue（对话）
    public class DialogueData
    {
        public string DialogueId { get; set; } = "";
        public string EventId { get; set; } = "";
        public string DialogueName { get; set; } = "";
        public string StartNodeId { get; set; } = "";
        public string EndType { get; set; } = "";
    }

    // Node（节点）
    public class NodeData
    {
        public string NodeId { get; set; } = "";
        public string DialogueId { get; set; } = "";
        public string NodeType { get; set; } = "";
        public string Speaker { get; set; } = "";
        public string Avatar { get; set; } = "";
        public string Content { get; set; } = "";
        public string NextNodeId { get; set; }
        public bool HasOptions { get; set; }
    }

    // Option（选项）
    public class OptionData
    {
        public int OptionId { get; set; }
        public string BranchNodeId { get; set; } = "";
        public int OptionIndex { get; set; }
        public string Text { get; set; } = "";
        public string NextNodeId { get; set; } = "";
        public string Condition { get; set; } = "";
        public string Effect { get; set; } = "";
    }

    // 五层结构根数据
    public class FiveLayerData
    {
        public List<SceneData> Scenes { get; set; }
        public List<EventData> Events { get; set; }
        public List<DialogueData> Dialogues { get; set; }
        public List<NodeData> Nodes { get; set; }
        public List<OptionData> Options { get; set; }
    }

    // 旧格式兼容 - DialogueNodeJson
    public class DialogueNodeJson
    {
        public string NodeId { get; set; } = "";
        public string Character { get; set; } = "";
        public string Avatar { get; set; } = "";
        public string Content { get; set; } = "";
        public string Next { get; set; }
        public bool HasOptions { get; set; }
        public List<DialogueOptionJson> Options { get; set; }
    }

    // JSON 选项结构
    public class DialogueOptionJson
    {
        public int OptionId { get; set; }
        public string NodeId { get; set; } = "";
        public int OptionIndex { get; set; }
        public string Text { get; set; } = "";
        public string Opinion { get; set; } = "";
        public string Next { get; set; } = "";
    }

    // JSON 对话文件根结构（旧格式）
    public class DialogueJson
    {
        public string DialogueId { get; set; } = "";
        public string StartNode { get; set; } = "";
        public Dictionary<string, DialogueNodeJson> Nodes { get; set; }
    }

    // 扁平化 JSON 结构（旧格式）
    public class DialogueInfo
    {
        public string DialogueId { get; set; } = "";
        public string StartNode { get; set; } = "";
    }

    public class FlatDialogueJson
    {
        public List<DialogueInfo> Dialogues { get; set; }
        public List<DialogueNodeJson> DialogueNodes { get; set; }
        public List<DialogueOptionJson> DialogueOptions { get; set; }
    }
}
