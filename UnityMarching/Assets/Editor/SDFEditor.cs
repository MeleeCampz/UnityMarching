using System;
using System.Linq;
using System.Collections.Generic;


using UnityEngine;
using UnityEditor;

namespace SDFEditor
{

	public class SDFEditor : EditorWindow
	{
		private GUIStyle _nodeStyle;
		private GUIStyle _selectedNodeStyle;
		private GUIStyle _inPointStyle;
		private GUIStyle _outPointStyle;

		private ConnectionPoint _selectedInPoint;
		private ConnectionPoint _selectedOutPoint;

		private Vector2 _offset;
		private Vector2 _drag;

		private readonly float _menuBarHeight = 20f;
		private Rect _menuBar;

		private SDFGraphAsset _graph;

		[MenuItem("Window/SDF Editor")]
		private static void OpenWindow()
		{
			SDFEditor window = GetWindow<SDFEditor>();
			window.titleContent = new GUIContent("SDF Editor");
		}

		private void OnEnable()
		{
			_nodeStyle = new GUIStyle();
			_nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
			_nodeStyle.border = new RectOffset(12, 12, 12, 12);

			_selectedNodeStyle = new GUIStyle();
			_selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
			_selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);

			_inPointStyle = new GUIStyle();
			_inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
			_inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
			_inPointStyle.border = new RectOffset(4, 4, 12, 12);

			_outPointStyle = new GUIStyle();
			_outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
			_outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
			_outPointStyle.border = new RectOffset(4, 4, 12, 12);
		}

		private void OnGUI()
		{
			DrawGrid(20, 0.2f, Color.gray);
			DrawGrid(100, 0.4f, Color.gray);
			DrawMenuBar();

			DrawControlButtons();

			if(_graph == null)
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

		private void DrawMenuBar()
		{
			_menuBar = new Rect(0, 0, position.width, _menuBarHeight);

			GUILayout.BeginArea(_menuBar, EditorStyles.toolbar);
			GUILayout.BeginHorizontal();

			if (GUILayout.Button(new GUIContent("Save"), EditorStyles.toolbarButton, GUILayout.Width(50)))
			{
				Save();
			}
			//GUILayout.Space(5);
			//if (GUILayout.Button(new GUIContent("Load"), EditorStyles.toolbarButton, GUILayout.Width(50)))
			//{
			//	Load();
			//}
			GUILayout.Space(5);
			if(GUILayout.Button(new GUIContent("Clear"), EditorStyles.toolbarButton, GUILayout.Width(50)))
			{
				Clear();
			}

			using (var check = new EditorGUI.ChangeCheckScope())
			{
				_graph = EditorGUILayout.ObjectField(_graph, typeof(SDFGraphAsset), false) as SDFGraphAsset;

				//if (check.changed)
				//{
				//	Load();
				//}
			}

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

		private void DrawNodes()
		{
			if ((_graph.nodes != null))
			{
				foreach (var node in _graph.nodes)
				{
					node.Draw();
				}
			}
		}

		private void DrawConnections()
		{
			if (_graph.connections != null)
			{
				foreach (var con in _graph.connections)
				{
					con.Draw();
				}
			}
		}

		private void DrawConnectionLine(Event e)
		{
			if (_selectedInPoint != null && _selectedOutPoint == null)
			{
				Handles.DrawBezier(
					_selectedInPoint.rect.center,
					e.mousePosition,
					_selectedInPoint.rect.center + Vector2.left * 50f,
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
					_selectedOutPoint.rect.center,
					e.mousePosition,
					_selectedOutPoint.rect.center - Vector2.left * 50f,
					e.mousePosition + Vector2.left * 50f,
					Color.white,
					null,
					2f
				);

				GUI.changed = true;
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
					else if(e.button == 0)
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
					bool guiChanged = _graph.nodes[i].ProcessEvents(e);

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
			//genericMenu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode<SDFNode>(mousePosition));
			genericMenu.AddItem(new GUIContent("Add output node"), false, () => OnClickAddNode<SDFNode_Output>(mousePosition));
			genericMenu.ShowAsContext();
		}

		private void OnClickAddNode<T>(Vector2 mousePosition) where T : SDFNode, new()
		{
			if (_graph.nodes == null)
			{
				_graph.nodes = new List<SDFNode>();
			}

			SDFNode newNode = CreateInstance<T>();
			newNode.Init(mousePosition, _nodeStyle, _selectedNodeStyle,
				_inPointStyle, _outPointStyle, OnClickInPoint, OnClickOutPoint,
				OnClickRemoveNode);

			AssetDatabase.AddObjectToAsset(newNode, _graph);
			AssetDatabase.SaveAssets();
			_graph.nodes.Add(newNode);
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

		private void OnClickRemoveNode(SDFNode node)
		{
			if (_graph.connections != null)
			{
				List<Connection> connectionsToRemove = new List<Connection>();

				foreach (var connection in _graph.connections)
				{
					if (node.inPoints.Any(x => x.node == connection.inPoint.node) || node.outPoints.Any(x => x.node == connection.outPoint.node))
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

		private void OnClickInPoint(ConnectionPoint inPoint)
		{
			_selectedInPoint = inPoint;

			if (_selectedOutPoint != null)
			{
				if (_selectedOutPoint.node != _selectedInPoint.node)
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

		private void OnClickOutPoint(ConnectionPoint outPoint)
		{
			_selectedOutPoint = outPoint;

			if (_selectedInPoint != null)
			{
				if (_selectedOutPoint.node != _selectedInPoint.node)
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

		private void OnClickRemoveConnection(Connection connection)
		{
			_graph.connections.Remove(connection);
		}

		private void CreateConnection()
		{
			if (_graph.connections == null)
			{
				_graph.connections = new List<Connection>();
			}

			Connection connection = new Connection(_selectedInPoint, _selectedOutPoint, OnClickRemoveConnection);
			_graph.connections.Add(connection);
		}

		private void ClearConnectionSelection()
		{
			_selectedInPoint = null;
			_selectedOutPoint = null;
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

		//private void Load()
		//{
		//	//TODO: Prompt user to save or discard if there are current changes?
		//	Clear();

		//	if (!_graph)
		//	{
		//		return;
		//	}

		//	foreach(var node in _graph.nodes)
		//	{
		//		var newNode = Instantiate(node);
		//		newNode.Init(_nodeStyle, _selectedNodeStyle, _inPointStyle, _outPointStyle,
		//			OnClickInPoint, OnClickOutPoint, OnClickRemoveNode);
		//		_nodes.Add(newNode);
		//	}

		//	foreach (var connection in _graph.connections)
		//	{
		//		var inPoint = _nodes.First(n => n.inPoint.id == connection.inPoint.id).inPoint;
		//		var outPoint = _nodes.First(n => n.outPoint.id == connection.outPoint.id).outPoint;
		//		var newConnection = Instantiate(connection);
		//		newConnection.Init(inPoint, outPoint, OnClickRemoveConnection);
		//		_connections.Add(newConnection);
		//	}
		//}

		//Check if we have any ciclic connections
		private bool IsTree(ConnectionPoint newOut, ConnectionPoint newIn)
		{
			bool isTree = true;



			return isTree;
		}
	}
}