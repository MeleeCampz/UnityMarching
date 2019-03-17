using System;
using System.Linq;
using System.Collections.Generic;


using UnityEngine;
using UnityEditor;

namespace SDFEditor
{

	public class SDFEditor : EditorWindow
	{
		public static GUIStyle NodeStyle { get; private set; }
		public static GUIStyle SelectedNodeStyle { get; private set; }
		public static GUIStyle InPointStyle { get; private set; }
		public static GUIStyle OutPointStyle { get; private set; }

		private IConnectionPoint _selectedInPoint;
		private IConnectionPoint _selectedOutPoint;

		private Vector2 _offset;
		private Vector2 _drag;

		private readonly float _menuBarHeight = 20f;
		private Rect _menuBar;

		private SDFEditorGraph _graph;

		[MenuItem("Window/SDF Editor")]
		private static void OpenWindow()
		{
			SDFEditor window = GetWindow<SDFEditor>();
			window.titleContent = new GUIContent("SDF Editor");
		}

		private void OnEnable()
		{
			NodeStyle = new GUIStyle();
			NodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
			NodeStyle.border = new RectOffset(12, 12, 12, 12);

			SelectedNodeStyle = new GUIStyle();
			SelectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
			SelectedNodeStyle.border = new RectOffset(12, 12, 12, 12);

			InPointStyle = new GUIStyle();
			InPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
			InPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
			InPointStyle.border = new RectOffset(4, 4, 12, 12);

			OutPointStyle = new GUIStyle();
			OutPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
			OutPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
			OutPointStyle.border = new RectOffset(4, 4, 12, 12);


		}

		private void OnGUI()
		{
			DrawGrid(20, 0.2f, Color.gray);
			DrawGrid(100, 0.4f, Color.gray);
			DrawMenuBar();

			DrawControlButtons();

			if (_graph == null)
			{
				return;
			}


			DrawNodes();
			DrawConnections();

			DrawConnectionLine(Event.current);
			ProcessNodeEvents(Event.current);
			ProcessEvents(Event.current);

			if (GUI.changed) Repaint();
		}

		public void OnClickRemoveNode(SDFNode node)
		{
			if (_graph.connections != null)
			{
				List<Connection> connectionsToRemove = new List<Connection>();

				foreach (var connection in _graph.connections)
				{
					if (node.inPoints.Any(x => x.GetNode() == connection.outPoint.GetNode()) 
						|| node.outPoints.Any(x => x.GetNode() == connection.inPoint.GetNode()))
					{
						connectionsToRemove.Add(connection);
					}
				}

				foreach (var conToRemove in connectionsToRemove)
				{
					_graph.connections.Remove(conToRemove);
				}

				connectionsToRemove = null;
			}
			_graph.nodes.Remove(node);
			DestroyImmediate(node, true);
			AssetDatabase.SaveAssets();
		}

		public void OnClickInPoint(IConnectionPoint inPoint)
		{
			_selectedInPoint = inPoint;

			if (_selectedOutPoint != null)
			{
				if (_selectedOutPoint.GetNode() != _selectedInPoint.GetNode())
				{
					CreateConnection();
					ClearConnectionSelection();
				}
				else
				{
					ClearConnectionSelection();
				}
			}
		}

		public void OnClickOutPoint(IConnectionPoint outPoint)

		{
			_selectedOutPoint = outPoint;

			if (_selectedInPoint != null)
			{
				if (_selectedOutPoint.GetNode() != _selectedInPoint.GetNode())
				{
					CreateConnection();
					ClearConnectionSelection();
				}
				else
				{
					ClearConnectionSelection();
				}
			}
		}

		public void OnClickRemoveConnection(Connection connection)
		{
			_graph.connections.Remove(connection);
		}

		private void DrawMenuBar()
		{
			_menuBar = new Rect(0, 0, position.width, _menuBarHeight);

			GUILayout.BeginArea(_menuBar, EditorStyles.toolbar);
			GUILayout.BeginHorizontal();

			if (GUILayout.Button(new GUIContent("Save"), EditorStyles.toolbarButton, GUILayout.Width(50)))
			{
				Save();
			}
			GUILayout.Space(5);
			if (GUILayout.Button(new GUIContent("Clear"), EditorStyles.toolbarButton, GUILayout.Width(50)))
			{
				Clear();
			}


			SDFEditorGraph before = _graph;
			_graph = EditorGUILayout.ObjectField(_graph, typeof(SDFEditorGraph), false) as SDFEditorGraph;

			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}

		private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
		{
			int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
			int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

			Handles.BeginGUI();
			Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

			_offset += _drag * 0.5f;
			Vector3 newOffset = new Vector3(_offset.x % gridSpacing, _offset.y % gridSpacing, 0);

			for (int i = 0; i < widthDivs; i++)
			{
				Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
			}

			for (int j = 0; j < heightDivs; j++)
			{
				Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
			}

			Handles.color = Color.white;
			Handles.EndGUI();
		}

		private void DrawControlButtons()
		{
			GUILayout.BeginVertical(new GUILayoutOption[] { GUILayout.Width(100) });
			GUILayout.EndVertical();
		}

		private void DrawConnectionLine(Event e)
		{
			if (_selectedInPoint != null && _selectedOutPoint == null)
			{
				Handles.DrawBezier(
					_selectedInPoint.GetRect().center,
					e.mousePosition,
					_selectedInPoint.GetRect().center + Vector2.left * 50f,
					e.mousePosition - Vector2.left * 50f,
					Color.white,
					null,
					2f
				);

				GUI.changed = true;
			}

			if (_selectedOutPoint != null && _selectedInPoint == null)
			{
				Handles.DrawBezier(
					_selectedOutPoint.GetRect().center,
					e.mousePosition,
					_selectedOutPoint.GetRect().center - Vector2.left * 50f,
					e.mousePosition + Vector2.left * 50f,
					Color.white,
					null,
					2f
				);

				GUI.changed = true;
			}
		}

		private void DrawNodes()
		{
			if ((_graph.nodes != null))
			{
				foreach (var node in _graph.nodes)
				{
					node.Draw(this);
				}
			}
		}

		private void DrawConnections()
		{
			if (_graph.connections != null)
			{
				//Reverse traveser array in case we delete something
				for (int i = _graph.connections.Count -1; i>=0; i--)
				{
					_graph.connections[i].Draw(this);
				}
			}
		}

		private void ProcessEvents(Event e)
		{
			_drag = Vector2.zero;

			switch (e.type)
			{
				case EventType.MouseDown:
					if (e.button == 1)
					{
						ProcessContextMenu(e.mousePosition);
					}
					else if (e.button == 0)
					{
						//Deselect if we click nothing
						_selectedInPoint = null;
						_selectedOutPoint = null;
					}
					break;

				case EventType.MouseDrag:
					if (e.button == 0)
					{
						OnDrag(e.delta);
					}
					break;
			}
		}

		private void ProcessNodeEvents(Event e)
		{
			if (_graph.nodes != null)
			{
				for (int i = _graph.nodes.Count - 1; i >= 0; i--)
				{
					bool guiChanged = _graph.nodes[i].ProcessEvents(e, this);

					if (guiChanged)
					{
						GUI.changed = true;
					}
				}
			}
		}

		private void ProcessContextMenu(Vector2 mousePosition)
		{
			GenericMenu genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Add example node"), false, () => OnClickAddNode<SDFNode_Example>(mousePosition));
			genericMenu.AddItem(new GUIContent("Add output node"), false, () => OnClickAddNode<SDFNode_Output>(mousePosition));
			genericMenu.AddItem(new GUIContent("Add + node"), false, () => OnClickAddNode<SDFNode_Add>(mousePosition));
			genericMenu.AddItem(new GUIContent("Add int node"), false, () => OnClickAddNode<SDFNode_IntField>(mousePosition));
			genericMenu.ShowAsContext();
		}

		public void OnClickAddNode<T>(Vector2 mousePosition) where T : SDFNode
		{
			if (_graph.nodes == null)
			{
				_graph.nodes = new List<SDFNode>();
			}

			SDFNode newNode = CreateInstance<T>();
			newNode.Setup(mousePosition, _graph);

			AssetDatabase.AddObjectToAsset(newNode, _graph);
			AssetDatabase.SaveAssets();
			_graph.nodes.Add(newNode);
		}

		private void CreateConnection()
		{
			if (_graph.connections == null)
			{
				_graph.connections = new List<Connection>();
			}

			//Output not is always start of connection Node->OutPoint->Inpoint->Node
			Connection connection = new Connection(_selectedOutPoint, _selectedInPoint);
			_graph.connections.Add(connection);
		}

		private void ClearConnectionSelection()
		{
			_selectedInPoint = null;
			_selectedOutPoint = null;
		}

		private void OnDrag(Vector2 delta)
		{
			_drag = delta;

			if (_graph.nodes != null)
			{
				foreach (var node in _graph.nodes)
				{
					node.Drag(delta);
				}
			}

			GUI.changed = true;
		}

		private void Save()
		{
			EditorUtility.SetDirty(_graph);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		private void Clear()
		{
			foreach (var node in _graph.nodes)
			{
				DestroyImmediate(node, true);
			}
			_graph.nodes = new List<SDFNode>();
			_graph.connections = new List<Connection>();

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}
}