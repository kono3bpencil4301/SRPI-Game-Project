using System;
using System.Collections.Generic;
using Godot;

namespace SRPI
{
    // JSON 解析辅助类
    public class DialogueJsonParser
    {
        // 解析五层结构 JSON
        public static FiveLayerData ParseFiveLayerJson(Godot.Collections.Dictionary data)
        {
            var fiveLayer = new FiveLayerData();

            if (data.ContainsKey("scenes"))
            {
                var scenesArr = (Godot.Collections.Array)data["scenes"];
                fiveLayer.Scenes = new List<SceneData>();
                foreach (var s in scenesArr)
                {
                    var sDict = (Godot.Collections.Dictionary)s;
                    fiveLayer.Scenes.Add(new SceneData
                    {
                        SceneId = GetStringValue(sDict, "scene_id"),
                        SceneName = GetStringValue(sDict, "scene_name"),
                        BgPath = GetStringValue(sDict, "bg_path"),
                        BgmPath = GetStringValue(sDict, "bgm_path"),
                        Description = GetStringValue(sDict, "description")
                    });
                }
            }

            if (data.ContainsKey("events"))
            {
                var eventsArr = (Godot.Collections.Array)data["events"];
                fiveLayer.Events = new List<EventData>();
                foreach (var e in eventsArr)
                {
                    var eDict = (Godot.Collections.Dictionary)e;
                    fiveLayer.Events.Add(new EventData
                    {
                        EventId = GetStringValue(eDict, "event_id"),
                        SceneId = GetStringValue(eDict, "scene_id"),
                        EventName = GetStringValue(eDict, "event_name"),
                        TriggerType = GetStringValue(eDict, "trigger_type"),
                        TriggerCondition = GetStringValue(eDict, "trigger_condition"),
                        StartDialogueId = GetStringValue(eDict, "start_dialogue_id"),
                        Priority = GetIntValue(eDict, "priority")
                    });
                }
            }

            if (data.ContainsKey("dialogues"))
            {
                var dialoguesArr = (Godot.Collections.Array)data["dialogues"];
                fiveLayer.Dialogues = new List<DialogueData>();
                foreach (var d in dialoguesArr)
                {
                    var dDict = (Godot.Collections.Dictionary)d;
                    fiveLayer.Dialogues.Add(new DialogueData
                    {
                        DialogueId = GetStringValue(dDict, "dialogue_id"),
                        EventId = GetStringValue(dDict, "event_id"),
                        DialogueName = GetStringValue(dDict, "dialogue_name"),
                        StartNodeId = GetStringValue(dDict, "start_node_id"),
                        EndType = GetStringValue(dDict, "end_type")
                    });
                }
            }

            if (data.ContainsKey("nodes"))
            {
                var nodesArr = (Godot.Collections.Array)data["nodes"];
                fiveLayer.Nodes = new List<NodeData>();
                foreach (var n in nodesArr)
                {
                    var nDict = (Godot.Collections.Dictionary)n;
                    fiveLayer.Nodes.Add(new NodeData
                    {
                        NodeId = GetStringValue(nDict, "node_id"),
                        DialogueId = GetStringValue(nDict, "dialogue_id"),
                        NodeType = GetStringValue(nDict, "node_type"),
                        Speaker = GetStringValue(nDict, "speaker"),
                        Avatar = GetStringValue(nDict, "avatar"),
                        Content = GetStringValue(nDict, "content"),
                        NextNodeId = GetStringValueOrNull(nDict, "next_node_id"),
                        HasOptions = GetBoolValue(nDict, "has_options")
                    });
                }
            }

            if (data.ContainsKey("options"))
            {
                var optionsArr = (Godot.Collections.Array)data["options"];
                fiveLayer.Options = new List<OptionData>();
                foreach (var o in optionsArr)
                {
                    var oDict = (Godot.Collections.Dictionary)o;
                    fiveLayer.Options.Add(new OptionData
                    {
                        OptionId = GetIntValue(oDict, "option_id"),
                        BranchNodeId = GetStringValue(oDict, "branch_node_id"),
                        OptionIndex = GetIntValue(oDict, "option_index"),
                        Text = GetStringValue(oDict, "text"),
                        NextNodeId = GetStringValue(oDict, "next_node_id"),
                        Condition = GetStringValue(oDict, "condition"),
                        Effect = GetStringValue(oDict, "effect")
                    });
                }
            }

            return fiveLayer;
        }

        // 解析扁平化 JSON
        public static FlatDialogueJson ParseFlatDialogueJson(Godot.Collections.Dictionary data)
        {
            var flatData = new FlatDialogueJson();

            if (data.ContainsKey("dialogues"))
            {
                var dialoguesArr = (Godot.Collections.Array)data["dialogues"];
                flatData.Dialogues = new List<DialogueInfo>();
                foreach (var d in dialoguesArr)
                {
                    var dDict = (Godot.Collections.Dictionary)d;
                    flatData.Dialogues.Add(new DialogueInfo
                    {
                        DialogueId = dDict["dialogue_id"].AsString(),
                        StartNode = dDict["start_node"].AsString()
                    });
                }
            }

            if (data.ContainsKey("dialogue_nodes"))
            {
                var nodesArr = (Godot.Collections.Array)data["dialogue_nodes"];
                flatData.DialogueNodes = new List<DialogueNodeJson>();
                foreach (var n in nodesArr)
                {
                    var nDict = (Godot.Collections.Dictionary)n;
                    flatData.DialogueNodes.Add(new DialogueNodeJson
                    {
                        NodeId = GetDictString(nDict, "node_id"),
                        Character = GetDictString(nDict, "character"),
                        Avatar = GetDictString(nDict, "avatar"),
                        Content = GetDictString(nDict, "content"),
                        Next = GetDictStringOrNull(nDict, "next"),
                        HasOptions = GetDictBool(nDict, "has_options")
                    });
                }
            }

            if (data.ContainsKey("dialogue_options"))
            {
                var optionsArr = (Godot.Collections.Array)data["dialogue_options"];
                flatData.DialogueOptions = new List<DialogueOptionJson>();
                foreach (var o in optionsArr)
                {
                    var oDict = (Godot.Collections.Dictionary)o;
                    flatData.DialogueOptions.Add(new DialogueOptionJson
                    {
                        OptionId = GetDictInt(oDict, "option_id"),
                        NodeId = GetDictString(oDict, "node_id"),
                        OptionIndex = GetDictInt(oDict, "option_index"),
                        Text = GetDictString(oDict, "text"),
                        Opinion = GetDictString(oDict, "opinion"),
                        Next = GetDictString(oDict, "next")
                    });
                }
            }

            return flatData;
        }

        // 解析旧格式 JSON
        public static DialogueJson ParseDialogueJson(Godot.Collections.Dictionary data)
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

                    var node = new DialogueNodeJson
                    {
                        Character = GetDictString(nodeDict, "character"),
                        Avatar = GetDictString(nodeDict, "avatar"),
                        Content = GetDictString(nodeDict, "content"),
                        Next = GetDictStringOrNull(nodeDict, "next")
                    };

                    if (nodeDict.ContainsKey("options"))
                    {
                        var optionsVariant = nodeDict["options"];
                        var optionsArr = (Godot.Collections.Array)optionsVariant;
                        node.Options = new List<DialogueOptionJson>();
                        foreach (var opt in optionsArr)
                        {
                            var optDict = (Godot.Collections.Dictionary)opt;
                            node.Options.Add(new DialogueOptionJson
                            {
                                Text = GetDictString(optDict, "text"),
                                Opinion = GetDictString(optDict, "opinion"),
                                Next = GetDictString(optDict, "next")
                            });
                        }
                    }

                    dialogue.Nodes[nodeId] = node;
                }
            }

            return dialogue;
        }

        // ========== 辅助方法 ==========

        private static string GetStringValue(Godot.Collections.Dictionary dict, string key) =>
            dict.ContainsKey(key) ? dict[key].ToString() : "";

        private static string GetStringValueOrNull(Godot.Collections.Dictionary dict, string key)
        {
            if (!dict.ContainsKey(key)) return null;
            var value = dict[key];
            return value.VariantType == Variant.Type.Nil ? null : value.ToString();
        }

        private static int GetIntValue(Godot.Collections.Dictionary dict, string key)
        {
            if (!dict.ContainsKey(key)) return 0;
            var value = dict[key];
            if (value.VariantType == Variant.Type.Int) return (int)value;
            return int.TryParse(value.ToString(), out int result) ? result : 0;
        }

        private static bool GetBoolValue(Godot.Collections.Dictionary dict, string key)
        {
            if (!dict.ContainsKey(key)) return false;
            var value = dict[key];
            if (value.VariantType == Variant.Type.Bool) return (bool)value;
            return bool.TryParse(value.ToString(), out bool result) && result;
        }

        private static string GetDictString(Godot.Collections.Dictionary dict, string key) =>
            dict.ContainsKey(key) ? dict[key].AsString() : "";

        private static string GetDictStringOrNull(Godot.Collections.Dictionary dict, string key) =>
            dict.ContainsKey(key) ? dict[key].AsString() : null;

        private static int GetDictInt(Godot.Collections.Dictionary dict, string key) =>
            dict.ContainsKey(key) ? (int)dict[key] : 0;

        private static bool GetDictBool(Godot.Collections.Dictionary dict, string key) =>
            dict.ContainsKey(key) ? (bool)dict[key] : false;
    }
}
