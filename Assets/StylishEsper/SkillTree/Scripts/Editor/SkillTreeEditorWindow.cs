//***************************************************************************************
// Writer: Stylish Esper
// Last Updated: August 2024
// Description: Skill Tree editor window.
//***************************************************************************************

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.IO;
using System;
using Esper.SkillTree.Stats;

namespace Esper.SkillTree.Editor
{
    public class SkillTreeEditorWindow : EditorWindow
    {
        private SplitView mainSplitView;
        private SplitView menuSplitView;
        private VisualElement graphContent;
        private VisualElement connectionContent;
        private VisualElement graphParent;
        private VisualElement treeList;
        private VisualElement treeProperties;
        private TextField treeNameField;
        private Vector2IntField treeDimensionsField;
        private FloatField nodeSizeField;
        private EnumField nodeShapeField;
        private VisualElement nodeProperties;
        private IntegerField nodePositionField;
        private TextField nodeKeyField;
        private TextField nodeNameField;
        private TextField nodeDescriptionField;
        private ObjectField nodeLockedSpriteField;
        private ObjectField nodeUnlockedSpriteField;
        private ObjectField nodeObtainedSpriteField;
        private IntegerField nodeMaxLevelField;
        private IntegerField nodeLevelReqField;
        private Toggle nodeUniqueField;
        private EnumField nodeUniqueShapeField;
        private FloatField nodeUniqueSizeField;
        private Button addButton;
        private Button duplicateButton;
        private Button deleteButton;
        private Button dimensionsLockButton;
        private Button openDocsButton;
        private Label dimensionsWarning;
        private VisualElement connectionProperties;
        private Label connectionMessage;
        private Button deleteConnectionButton;
        private Toggle twoWayField;
        private Button switchDirectionButton;
        private IntegerField nodeTotalPointsReqField;
        private EnumField treeVisibilityField;
        private DropdownField nodeTypeField;
        private FloatField nodeEffectDurationField;
        private FloatField nodeCooldownField;
        private Toggle nodeEOTField;
        private Toggle nodeSingleTargetField;
        private Toggle nodeCancelableField;
        private FloatField nodeWindupField;
        private Button openStatsButton;
        private Toggle treeEnabledField;
        private FloatField nodeRangeField;
        private Toggle forPlayerField;
        private EnumField graphVisibilityField;
        private FloatField nodeDurationField;
        private FloatField nodeSpeedField;
        private Button deleteNodeButton;
        private FloatField nodeCostField;
        private ObjectField treeCustomObjectField;
        private ObjectField nodeCustomObjectField;
        private Label skillLabel;
        private Label skillTreeLabel;

        private SkillGraph selectedGraph;
        private SkillNode selectedNode;
        private SkillConnection selectedConnection;
        private Stat selectedStat;

        private List<TreeButton> trees = new();
        private Dictionary<int, SkillNodeElement> nodeInstances = new();
        private List<SkillConnectionElement> nodeConnections = new();

        private StyleBackground lockBackground;
        private StyleBackground unlockBackground;
        private Vector2 prevParentSize = Vector2.zero;
        private TreeVisibility treeVisibility;

        private SkillConnection connectionBeingCreated;

        public bool creatingConnection;

        [MenuItem("Window/Skill Tree/Skill Tree Editor")]
        public static void Open()
        {
            SkillTreeEditorWindow wnd = GetWindow<SkillTreeEditorWindow>();
            wnd.titleContent = new GUIContent("Skill Tree");
        }

        public void CreateGUI()
        {
            minSize = new Vector2(800, 600);
            selectedConnection = null;
            selectedNode = null;
            selectedConnection = null;

            VisualElement root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/StylishEsper/SkillTree/Scripts/Editor/SkillTreeEditorWindow.uxml");
            visualTree.CloneTree(root);

            trees.Clear();

            titleContent = new GUIContent("Skill Tree");

            mainSplitView = root.Q<SplitView>("SplitViewMain");
            menuSplitView = root.Q<SplitView>("SplitViewMenu");
            treeList = root.Q<VisualElement>("TreeList");
            graphContent = root.Q<VisualElement>("Graph");
            addButton = root.Q<Button>("AddButton");
            duplicateButton = root.Q<Button>("DuplicateButton");
            deleteButton = root.Q<Button>("DeleteButton");
            dimensionsLockButton = root.Q<Button>("DimensionsLockButton");
            treeProperties = root.Q<VisualElement>("TreeProperties");
            treeEnabledField = root.Q<Toggle>("TreeEnabledField");
            treeNameField = root.Q<TextField>("TreeNameField");
            treeDimensionsField = root.Q<Vector2IntField>("TreeDimensionsField");
            nodeSizeField = root.Q<FloatField>("NodeSizeField");
            nodeShapeField = root.Q<EnumField>("NodeShapeField");
            nodeProperties = root.Q<VisualElement>("NodeProperties");
            nodePositionField = root.Q<IntegerField>("NodePositionField");
            nodeKeyField = root.Q<TextField>("NodeKeyField");
            nodeNameField = root.Q<TextField>("NodeNameField");
            nodeDescriptionField = root.Q<TextField>("NodeDescriptionField");
            nodeLockedSpriteField = root.Q<ObjectField>("NodeLockedSpriteField");
            nodeUnlockedSpriteField = root.Q<ObjectField>("NodeUnlockedSpriteField");
            nodeObtainedSpriteField = root.Q<ObjectField>("NodeObtainedSpriteField");
            nodeMaxLevelField = root.Q<IntegerField>("NodeMaxLevelField");
            nodeUniqueField = root.Q<Toggle>("NodeUniqueField");
            nodeUniqueShapeField = root.Q<EnumField>("NodeUniqueShapeField");
            nodeUniqueSizeField = root.Q<FloatField>("NodeUniqueSizeField");
            dimensionsWarning = root.Q<Label>("DimensionsWarning");
            connectionProperties = root.Q<VisualElement>("ConnectionProperties");
            connectionMessage = root.Q<Label>("ConnectionMessage");
            deleteConnectionButton = root.Q<Button>("DeleteConnectionButton");
            nodeLevelReqField = root.Q<IntegerField>("NodeLevelRequirementField");
            twoWayField = root.Q<Toggle>("TwoWayField");
            switchDirectionButton = root.Q<Button>("SwitchDirectionButton");
            nodeTotalPointsReqField = root.Q<IntegerField>("NodeTotalPointsRequirementField");
            treeVisibilityField = root.Q<EnumField>("TreeVisibilityField");
            nodeTypeField = root.Q<DropdownField>("NodeTypeField");
            nodeEffectDurationField = root.Q<FloatField>("NodeEffectDurationField");
            nodeCooldownField = root.Q<FloatField>("NodeCooldownField");
            nodeEOTField = root.Q<Toggle>("NodeEOTField");
            nodeSingleTargetField = root.Q<Toggle>("NodeSingleTargetField");
            nodeCancelableField = root.Q<Toggle>("NodeCancelableField");
            nodeWindupField = root.Q<FloatField>("NodeWindupField");
            openStatsButton = root.Q<Button>("OpenStatsButton");
            nodeRangeField = root.Q<FloatField>("NodeRangeField");
            forPlayerField = root.Q<Toggle>("ForPlayerField");
            graphVisibilityField = root.Q<EnumField>("GraphVisibilityField");
            nodeDurationField = root.Q<FloatField>("NodeDurationField");
            nodeSpeedField = root.Q<FloatField>("NodeSpeedField");
            openDocsButton = root.Q<Button>("OpenDocsButton");
            deleteNodeButton = root.Q<Button>("DeleteNodeButton");
            nodeCostField = root.Q<FloatField>("NodeCostField");
            treeCustomObjectField = root.Q<ObjectField>("TreeCustomObjectField");
            nodeCustomObjectField = root.Q<ObjectField>("NodeCustomObjectField");
            skillLabel = root.Q<Label>("SkillLabel");
            skillTreeLabel = root.Q<Label>("SkillTreeLabel");
            connectionContent = new VisualElement();
            graphParent = graphContent.parent;

            openDocsButton.clicked += () =>
            {
                Application.OpenURL("https://stylishesper.gitbook.io/skill-tree/systems/skill-tree/editor-window/skills-skill-nodes#adding-sprites");
            };

            deleteNodeButton.clicked += () =>
            {
                DeleteNode(selectedNode);
            };

            addButton.clicked += () =>
            {
                CreateTree();
            };

            duplicateButton.clicked += () =>
            {
                DuplicateTree();
            };

            deleteButton.clicked += () =>
            {
                DeleteTree();
            };

            openStatsButton.clicked += () =>
            {
                if (selectedNode)
                {
                    SkillStatsEditor.Open(selectedNode, () => nodeInstances[selectedNode.positionIndex].Reload());
                }
            };

            dimensionsLockButton.clicked += () =>
            {
                DimensionsLock(treeDimensionsField.enabledSelf);
            };

            deleteConnectionButton.clicked += () =>
            {
                DeleteConnection(selectedConnection);
            };

            switchDirectionButton.clicked += () =>
            {
                SwitchConnectionDirection(selectedConnection);
            };

#if !UNITY_2021
            var bgSize = new StyleBackgroundSize(new BackgroundSize(Length.Percent(80), Length.Percent(80)));
            dimensionsLockButton.style.backgroundSize = bgSize;
#endif
            lockBackground = new StyleBackground(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/StylishEsper/SkillTree/Editor/Icons/lock_icon.png"));
            unlockBackground = new StyleBackground(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/StylishEsper/SkillTree/Editor/Icons/unlock_icon.png"));

            treeNameField.RegisterValueChangedCallback(x =>
            {
                if (!selectedGraph)
                {
                    return;
                }

                if (x.newValue == string.Empty)
                {
                    selectedGraph.displayName = x.previousValue;
                    return;
                }

                selectedGraph.UpdateAssetName();
                SetTreeButtonName(selectedGraph, x.newValue);
            });

            nodePositionField.RegisterValueChangedCallback(x => 
            {
                bool movingUp = x.newValue > x.previousValue;

                if (x.newValue < 0)
                {
                    selectedNode.positionIndex = 0;
                }
                else if (x.newValue >= selectedGraph.NodeCount)
                {
                    selectedNode.positionIndex = selectedGraph.NodeCount - 1;
                }
                else if (movingUp)
                {
                    bool positionFound = false;

                    for (int i = x.newValue; i < selectedGraph.NodeCount; i++)
                    {
                        if (!NodePositionExists(selectedNode, i))
                        {
                            selectedNode.positionIndex = i;
                            positionFound = true;
                            break;
                        }
                    }

                    if (!positionFound)
                    {
                        selectedNode.positionIndex = selectedGraph.GetFirstAvailablePositionIndex();
                    }
                }
                else if (!movingUp)
                {
                    bool positionFound = false;

                    for (int i = x.newValue; i < selectedGraph.NodeCount; i--)
                    {
                        if (!NodePositionExists(selectedNode, i))
                        {
                            selectedNode.positionIndex = i;
                            positionFound = true;
                            break;
                        }
                    }

                    if (!positionFound)
                    {
                        selectedNode.positionIndex = selectedGraph.GetFirstAvailablePositionIndex();
                    }
                }

                UpdateGraphDisplay(true);
            });

            nodeKeyField.RegisterValueChangedCallback(x =>
            {
                if (x.newValue == string.Empty || NodeKeyExists(selectedNode))
                {
                    selectedNode.key = x.previousValue;
                    return;
                }

                selectedNode.UpdateAssetName();
            });

            treeDimensionsField.RegisterValueChangedCallback(x =>
            {
                UpdateGraphDisplay(true);
            });
            DimensionsLock(true);

            nodeSizeField.RegisterValueChangedCallback(x =>
            {
                UpdateGraphDisplay(true);
            });

            nodeShapeField.RegisterValueChangedCallback(x =>
            {
                UpdateGraphDisplay(true);
            });

            nodeTypeField.RegisterValueChangedCallback(x =>
            {
                selectedNode.skillTypeIndex = SkillTreeUtility.StringToSkillTypeIndex(x.newValue);
            });

            nodeLockedSpriteField.RegisterValueChangedCallback(x =>
            {
                UpdateGraphDisplay(true);
            });

            nodeUnlockedSpriteField.RegisterValueChangedCallback(x =>
            {
                UpdateGraphDisplay(true);
            });

            nodeObtainedSpriteField.RegisterValueChangedCallback(x =>
            {
                UpdateGraphDisplay(true);
            });

            nodeUniqueField.RegisterValueChangedCallback(x =>
            {
                UpdateNodeProperties();
                UpdateGraphDisplay(true);
            });

            nodeUniqueSizeField.RegisterValueChangedCallback(x =>
            {
                UpdateGraphDisplay(true);
            });

            nodeUniqueShapeField.RegisterValueChangedCallback(x =>
            {
                UpdateGraphDisplay(true);
            });

            nodeEOTField.RegisterValueChangedCallback(x =>
            {
                UpdateNodeProperties();
            });

            twoWayField.RegisterValueChangedCallback(x =>
            {
                selectedConnection.twoWay = x.newValue;
                selectedGraph.Save();
                UpdateConnectionProperties();
                UpdateGraphDisplay(true);
            });

            treeVisibilityField.RegisterValueChangedCallback(x =>
            {
                treeVisibility = (TreeVisibility)x.newValue;
                UpdateGraphDisplay(true);
            });

            ReloadTrees();
            selectedGraph = null;
            UpdateGraphDisplay(true);

            root.schedule.Execute(() =>
            {
                ResetSplitViews();
            }).ExecuteLater(1);
        }

        private void OnDestroy()
        {
            if (selectedGraph)
            {
                selectedGraph.Save();
            }

            if (HasOpenInstances<SkillStatsEditor>())
            {
                GetWindow<SkillStatsEditor>().Close();
            }
        }

        private void OnGUI()
        {
            UpdateGraphDisplay();
            HandleShortcuts();
        }

        private void DimensionsLock(bool locked)
        {
            if (locked)
            {
                treeDimensionsField.SetEnabled(false);
                treeDimensionsField.style.opacity = 0.5f;
                dimensionsLockButton.style.backgroundImage = lockBackground;
                dimensionsWarning.style.display = DisplayStyle.None;
            }
            else
            {
                treeDimensionsField.SetEnabled(true);
                treeDimensionsField.style.opacity = 1f;
                dimensionsLockButton.style.backgroundImage = unlockBackground;
                dimensionsWarning.style.display = DisplayStyle.Flex;
            }
        }

        private void SetTreeButtonName(SkillGraph graph, string name)
        {
            foreach (var item in trees)
            {
                if (item.graph == graph)
                {
                    item.button.text = name;
                }
            }
        }

        private void UpdateGraphDisplay(bool forceUpdate = false)
        {
            if (graphParent == null || graphContent == null)
            {
                return;
            }

            var rect = graphParent.worldBound;
            var size = rect.size;

            if (!forceUpdate && size == prevParentSize)
            {
                return;
            }

            prevParentSize = size;

            size.x = 16f / 9f * size.y;

            graphContent.style.width = size.x;
            graphContent.style.height = size.y;

            if (selectedGraph != null)
            {
                float padding = 60f / (1920f / size.x);
                float contentWidth = graphContent.style.width.value.value - padding * 2;
                float contentHeight = graphContent.style.height.value.value - padding * 2;
                float nodeSize = selectedGraph.nodeSize / (1920f / size.x);
                int nodeCount = selectedGraph.gridDimensions.x * selectedGraph.gridDimensions.y;
                float spacingX = (contentWidth - nodeSize * selectedGraph.gridDimensions.x) / (selectedGraph.gridDimensions.x - 1);
                float spacingY = (contentHeight - nodeSize * selectedGraph.gridDimensions.y) / (selectedGraph.gridDimensions.y - 1);

                if (spacingX < 0)
                {
                    spacingX = 0;
                }

                if (spacingY < 0)
                {
                    spacingY = 0;
                }

                graphContent.Clear();
                nodeInstances.Clear();
                connectionContent.Clear();
                graphContent.Add(connectionContent);

                Vector2 position = new Vector2(padding, padding);
                var graph = SkillGraph.GetSkillGraph(selectedGraph.id);
                for (int i = 0; i < nodeCount; i++)
                {
                    if (i != 0 && i % selectedGraph.gridDimensions.x == 0)
                    {
                        position.y += spacingY + nodeSize;
                        position.x = padding;
                    }

                    int index = i;
                    var node = selectedGraph.GetNodeByPosition(index);

                    if (node)
                    {
                        var nodeElement = new SkillNodeElement(graph, node);
                        nodeElement.clicked += () =>
                        {
                            SelectNode(nodeElement);

                            if (HasOpenInstances<SkillStatsEditor>())
                            {
                                SkillStatsEditor.Open(node, () => nodeInstances[index].Reload());
                            }
                        };
                        graphContent.Add(nodeElement);
                        nodeElement.UpdateShape(selectedGraph);
                        nodeElement.SetPosition(position);

                        if (node.isUnique)
                        {
                            var uniqueSize = node.uniqueSize / (1920f / size.x);
                            nodeElement.SetSize(uniqueSize, uniqueSize);
                        }
                        else
                        {
                            nodeElement.SetSize(nodeSize, nodeSize);
                        }

                        nodeInstances.Add(node.positionIndex, nodeElement);

                        if (treeVisibility == TreeVisibility.Complete)
                        {
                            nodeElement.SetVisible(node.IsValid());
                        }
                    }
                    else
                    {
                        var addSkillButton = new ToolbarButton();
                        addSkillButton.style.position = Position.Absolute;
                        addSkillButton.text = "+";
                        addSkillButton.style.width = nodeSize;
                        addSkillButton.style.height = nodeSize;
                        addSkillButton.style.paddingBottom = 3;
                        addSkillButton.style.paddingLeft = 0;
                        addSkillButton.style.paddingRight = 0;
                        addSkillButton.style.paddingTop = 0;
                        addSkillButton.style.fontSize = nodeSize / 2;
                        addSkillButton.style.unityTextAlign = TextAnchor.MiddleCenter;
                        addSkillButton.style.borderRightWidth = 2;
                        addSkillButton.style.borderLeftWidth = 2;
                        addSkillButton.style.borderTopWidth = 2;
                        addSkillButton.style.borderBottomWidth = 2;
                        addSkillButton.style.borderBottomLeftRadius = nodeSize;
                        addSkillButton.style.borderTopRightRadius = nodeSize;
                        addSkillButton.style.borderBottomRightRadius = nodeSize;
                        addSkillButton.style.borderTopLeftRadius = nodeSize;
                        addSkillButton.style.left = position.x;
                        addSkillButton.style.top = position.y;
                        graphContent.Add(addSkillButton);

                        addSkillButton.clicked += () =>
                        {
                            selectedGraph.AddEmptySkillNode(index);
                            UpdateGraphDisplay(true);
                            SelectNode(nodeInstances[index]);
                        };

                        if (treeVisibility == TreeVisibility.Complete)
                        {
                            addSkillButton.style.visibility = Visibility.Hidden;
                        }
                    }

                    position.x += spacingX + nodeSize;
                }

                nodeConnections.Clear();
                foreach (var connection in selectedGraph.connections)
                {
                    CreateConnection(selectedGraph, connection, true);
                }
            }
        }

        private void HandleShortcuts()
        {
            Event current = Event.current;
            if (current != null)
            {
                if (current.type == EventType.KeyDown)
                {
                    if (current.keyCode == KeyCode.Escape)
                    {
                        CancelConnectionCreation();
                    }
                    else if (current.control)
                    {
                        switch (current.keyCode)
                        {
                            case KeyCode.R:
                                ResetSplitViews();
                                current.Use();
                                break;
                        }
                    }
                }
            }
        }

        public Vector2 GetWindowSize()
        {
            return new Vector2(position.width, position.height);
        }

        public void ResetSplitViews()
        {
            Vector2 size = GetWindowSize();
            mainSplitView.fixedPaneInitialDimension = size.y * 0.65f;

            mainSplitView.CollapseChild(0);
            mainSplitView.UnCollapse();
            menuSplitView.CollapseChild(0);
            menuSplitView.UnCollapse();
        }

        public void ReloadTrees()
        {
            treeProperties.style.display = DisplayStyle.None;
            nodeProperties.style.display = DisplayStyle.None;
            connectionProperties.style.display = DisplayStyle.None;
            trees.Clear();
            treeList.Clear();

            if (!Directory.Exists(SkillGraph.directoryPath))
            {
                Directory.CreateDirectory(SkillGraph.directoryPath);
            }

            string[] files = Directory.GetFiles(SkillGraph.directoryPath, "*.asset", SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                var tree = AssetDatabase.LoadAssetAtPath<SkillGraph>(file);
                CreateTree(tree, true);
            }
        }

        public void StartConnectionCreation(SkillNodeElement skillNodeA)
        {
            creatingConnection = true;
            connectionBeingCreated = new SkillConnection();
            connectionBeingCreated.graphReferenceID = selectedGraph.id;
            connectionBeingCreated.skillNodeA = skillNodeA.node;
        }

        public void EndConnectionCreation(SkillNodeElement skillNodeB)
        {
            if (creatingConnection)
            {
                if (connectionBeingCreated.skillNodeA != skillNodeB.node)
                {
                    bool connectionExists = false;

                    foreach (var item in nodeConnections)
                    {
                        if (item.connection.skillNodeA == connectionBeingCreated.skillNodeA &&
                            item.connection.skillNodeB == skillNodeB.node)
                        {
                            connectionExists = true;
                        }
                    }

                    if (!connectionExists)
                    {
                        connectionBeingCreated.skillNodeB = skillNodeB.node;

                        if (connectionBeingCreated.IsValid())
                        {
                            CreateConnection(selectedGraph, connectionBeingCreated, false);
                        }
                    }

                    creatingConnection = false;
                }

                connectionBeingCreated = null;
            }
        }

        public void CreateConnection(SkillGraph graph, SkillConnection connection, bool existing)
        {
            if (!connection.skillNodeA || !connection.skillNodeB || !nodeInstances.ContainsKey(connection.skillNodeA.positionIndex) || !nodeInstances.ContainsKey(connection.skillNodeB.positionIndex))
            {
                return;
            }

            var skillGraph = SkillGraph.GetSkillGraph(graph.id);

            if (!existing)
            {
                foreach (var item in graph.connections)
                {
                    if ((item.skillNodeA == connection.skillNodeA || item.skillNodeA == connection.skillNodeB) &&
                        (item.skillNodeB == connection.skillNodeA || item.skillNodeB == connection.skillNodeB))
                    {
                        return;
                    }
                }

                graph.connections.Add(connection);
            }

            var rect = graphParent.worldBound;
            var size = rect.size;
            size.x = 16f / 9f * size.y;

            var nodeConnection = new SkillConnectionElement(skillGraph, connection, nodeInstances[connection.skillNodeA.positionIndex], nodeInstances[connection.skillNodeB.positionIndex], size, connectionContent);
            nodeConnections.Add(nodeConnection);

            nodeConnection.clicked += () =>
            {
                SelectConnection(nodeConnection.connection);
            };
        }

        public void ReloadAllConnections()
        {
            var rect = graphParent.worldBound;
            var size = rect.size;
            size.x = 16f / 9f * size.y;

            foreach (var item in nodeConnections)
            {
                item.Reload(size);
            }
        }

        public void CancelConnectionCreation()
        {
            creatingConnection = false;
            connectionBeingCreated = null;
        }

        public SkillConnectionElement GetConnectionElement(SkillConnection connection)
        {
            foreach (var item in nodeConnections)
            {
                if (item.connection.IsEqualTo(connection))
                {
                    return item;
                }
            }

            return null;
        }

        public void SelectConnection(SkillConnection connection)
        {
            treeProperties.style.display = DisplayStyle.None;
            DeselectNode();
            selectedConnection = connection;
            connectionProperties.style.display = DisplayStyle.Flex;
            UpdateConnectionProperties();
        }

        public void DeselectConnection()
        {
            selectedConnection = null;
            connectionProperties.style.display = DisplayStyle.None;
        }

        public void SwitchConnectionDirection(SkillConnection connection)
        {
            var temp = connection.skillNodeA;
            connection.skillNodeA = connection.skillNodeB;
            connection.skillNodeB = temp;
            UpdateGraphDisplay(true);
            UpdateConnectionProperties();
        }

        public void DeleteConnection(SkillConnection connection)
        {
            var element = GetConnectionElement(connection);
            element.RemoveFromHierarchy();
            nodeConnections.Remove(element);
            selectedGraph.connections.Remove(element.connection);
            DeselectConnection();
        }

        public ToolbarButton CreateTreeButton(SkillGraph graph)
        {
            var button = new ToolbarButton();
            button.style.borderTopWidth = 1;
            button.style.borderBottomWidth = 1;
            button.style.borderRightWidth = 1;
            button.style.borderLeftWidth = 1;
            button.style.marginBottom = 5;
            button.text = graph.displayName;

            var treeButton = new TreeButton()
            {
                graph = graph,
                button = button
            };

            button.clicked += () =>
            {
                SelectGraph(graph);
            };

            trees.Add(treeButton);
            treeList.Add(button);

            return button;
        }

        public void CreateTree(SkillGraph graph = null, bool createButton = true)
        {
            if (!graph)
            {
                string name = "New Skill Tree";
                graph = SkillGraph.Create(name);
                SelectGraph(graph);
            }

            if (createButton)
            {
                CreateTreeButton(graph);
            }
        }

        public void DuplicateTree()
        {
            if (!selectedGraph)
            {
                return;
            }

            var copy = SkillGraph.CreateCopy(selectedGraph);
            CreateTree(copy, true);
        }


        public void DeleteTree()
        {
            if (!selectedGraph)
            {
                return;
            }

            if (!EditorUtility.DisplayDialog("Are you sure?", $"Delete '{selectedGraph.displayName}'? This cannot be undone", "Yes", "Cancel"))
            {
                return;
            }

            List<string> paths = new();
            List<string> failed = new();

            foreach (var node in selectedGraph.nodes)
            {
                paths.Add(AssetDatabase.GetAssetPath(node));
            }

            AssetDatabase.DeleteAssets(paths.ToArray(), failed);
            AssetDatabase.DeleteAsset(Path.Combine(SkillNode.directoryPath, selectedGraph.id.ToString()));
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(selectedGraph));
            DeselectGraph();
            DeselectNode();
            ReloadTrees();
        }

        public void UpdateConnectionProperties()
        {
            twoWayField.SetValueWithoutNotify(selectedConnection.twoWay);

            var graph = SkillGraph.GetSkillGraph(selectedConnection.graphReferenceID);
            var skillA = selectedConnection.skillNodeA;
            var skillB = selectedConnection.skillNodeB;

            if (selectedConnection.twoWay)
            {
                connectionMessage.text = $"Connection from {skillA.key} to {skillB.key} and from {skillB.key} to {skillA.key}";
                switchDirectionButton.style.display = DisplayStyle.None;
            }
            else
            {
                connectionMessage.text = $"Connection from {skillA.key} to {skillB.key}";
                switchDirectionButton.style.display = DisplayStyle.Flex;
            }
        }

        public void SelectGraph(SkillGraph graph)
        {
            if (selectedGraph)
            {
                selectedGraph.Save();
            }

            var serializedObject = new SerializedObject(graph);
            treeEnabledField.BindProperty(serializedObject.FindProperty("enabled"));
            forPlayerField.BindProperty(serializedObject.FindProperty("forPlayer"));
            graphVisibilityField.BindProperty(serializedObject.FindProperty("visibility"));
            treeNameField.BindProperty(serializedObject.FindProperty("displayName"));
            treeDimensionsField.BindProperty(serializedObject.FindProperty("gridDimensions"));
            nodeSizeField.BindProperty(serializedObject.FindProperty("nodeSize"));
            nodeShapeField.BindProperty(serializedObject.FindProperty("nodeShape"));
            treeCustomObjectField.BindProperty(serializedObject.FindProperty("customObject"));
            skillTreeLabel.text = $"<u>Skill Tree (ID: {graph.id})</u>";

            DeselectConnection();
            DeselectNode();
            selectedGraph = graph;
            treeProperties.style.display = DisplayStyle.Flex;

            UpdateGraphDisplay(true);
        }

        public void DeselectGraph()
        {
            selectedGraph = null;
            graphContent.Clear();
            treeProperties.style.display = DisplayStyle.None;
        }

        public void UpdateNodeProperties()
        {
            nodeTypeField.choices.Clear();
            var types = SkillTree.Settings.skillTypes;

            foreach (var type in types)
            {
                nodeTypeField.choices.Add(type);
            }

            nodeTypeField.SetValueWithoutNotify(SkillTree.Settings.skillTypes[selectedNode.skillTypeIndex]);

            if (!selectedNode.isEffectOverTime)
            {
                nodeEffectDurationField.style.display = DisplayStyle.None;
            }
            else
            {
                nodeEffectDurationField.style.display = DisplayStyle.Flex;
            }

            if (!selectedNode.isUnique)
            {
                nodeUniqueShapeField.style.display = DisplayStyle.None;
                nodeUniqueSizeField.style.display = DisplayStyle.None;
            }
            else
            {
                nodeUniqueShapeField.style.display = DisplayStyle.Flex;
                nodeUniqueSizeField.style.display = DisplayStyle.Flex;
            }
        }

        public void SelectNode(SkillNodeElement nodeElement)
        {
            selectedNode = nodeElement.node;
            var serializedObject = new SerializedObject(selectedNode);

            nodePositionField.BindProperty(serializedObject.FindProperty("positionIndex"));
            nodeKeyField.BindProperty(serializedObject.FindProperty("key"));
            nodeNameField.BindProperty(serializedObject.FindProperty("displayName"));
            nodeDescriptionField.BindProperty(serializedObject.FindProperty("description"));
            nodeLockedSpriteField.BindProperty(serializedObject.FindProperty("lockedSprite"));
            nodeUnlockedSpriteField.BindProperty(serializedObject.FindProperty("unlockedSprite"));
            nodeObtainedSpriteField.BindProperty(serializedObject.FindProperty("obtainedSprite"));
            nodeMaxLevelField.BindProperty(serializedObject.FindProperty("maxLevel"));
            nodeUniqueField.BindProperty(serializedObject.FindProperty("isUnique"));
            nodeUniqueSizeField.BindProperty(serializedObject.FindProperty("uniqueSize"));
            nodeUniqueShapeField.BindProperty(serializedObject.FindProperty("uniqueShape"));
            nodeLevelReqField.BindProperty(serializedObject.FindProperty("playerLevelRequirement"));
            nodeTotalPointsReqField.BindProperty(serializedObject.FindProperty("treeTotalPointsSpentRequirement"));
            nodeEffectDurationField.BindProperty(serializedObject.FindProperty("effectDuration"));
            nodeDurationField.BindProperty(serializedObject.FindProperty("duration"));
            nodeCooldownField.BindProperty(serializedObject.FindProperty("cooldown"));
            nodeEOTField.BindProperty(serializedObject.FindProperty("isEffectOverTime"));
            nodeSingleTargetField.BindProperty(serializedObject.FindProperty("isSingleTarget"));
            nodeCancelableField.BindProperty(serializedObject.FindProperty("isCancelable"));
            nodeWindupField.BindProperty(serializedObject.FindProperty("windupDuration"));
            nodeRangeField.BindProperty(serializedObject.FindProperty("range"));
            nodeSpeedField.BindProperty(serializedObject.FindProperty("speed"));
            nodeCostField.BindProperty(serializedObject.FindProperty("cost"));
            nodeCustomObjectField.BindProperty(serializedObject.FindProperty("customObject"));

            if (creatingConnection)
            {
                EndConnectionCreation(nodeElement);
            }

            treeProperties.style.display = DisplayStyle.None;
            DeselectConnection();

            nodeProperties.style.display = DisplayStyle.Flex;
            UpdateNodeProperties();
        }

        public void DeselectNode()
        {
            selectedNode = null;
            nodeProperties.style.display = DisplayStyle.None;
        }

        public void DeleteNode(SkillNode node)
        {
            if (!selectedGraph)
            {
                return;
            }

            int count = selectedGraph.connections.Count;
            int removed = 0;
            for (int i = 0; i < count; i++)
            {
                int index = i - removed;
                var connection = selectedGraph.connections[index];

                if (connection.skillNodeA == node || connection.skillNodeB == node)
                {
                    selectedGraph.connections.Remove(connection);
                    removed++;
                }
            }

            selectedGraph.nodes.Remove(node);
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(node));
            UpdateGraphDisplay(true);
            DeselectNode();
        }

        public bool NodeKeyExists(SkillNode node)
        {
            foreach (var n in selectedGraph.nodes)
            {
                if (n != node && n.key == node.key)
                {
                    return true;
                }
            }

            return false;
        }

        public bool NodePositionExists(SkillNode node, int positionIndex)
        {
            foreach (var n in selectedGraph.nodes)
            {
                if (n != node && n.positionIndex == positionIndex)
                {
                    return true;
                }
            }

            return false;
        }


        public struct TreeButton
        {
            public SkillGraph graph;
            public ToolbarButton button;
        }

        public enum TreeVisibility
        {
            All,
            Complete
        }
    }
}
#endif