using System;
using System.Linq;
using System.Collections.Generic;


using UnityEngine;
using UnityEditor;

namespace SDFEditor
{

	public class SDFEditor : EditorWindow
	{
		private List<SDFNode> _nodes;
		private List<Connection> _connections;

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

		private SDFGraphAsset _currentAsset;

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
			GUILayout.Space(5);
			if (GUILayout.Button(new GUIContent("Load"), EditorStyles.toolbarButton, GUILayout.Width(50)))
			{
				Load();
			}
			GUILayout.Space(5);
			GUILayout.Button(new GUIContent("Apply"), EditorStyles.toolbarButton, GUILayout.Width(50));

			using (var check = new EditorGUI.ChangeCheckScope())
			{
				_currentAsset = EditorGUILayout.ObjectField(_currentAsset, typeof(SDFGraphAsset), false) as SDFGraphAsset;

				if (check.changed)
				{
					Load();
				}
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
			if ((_nodes != null))
			{
				foreach (var node in _nodes)
				{
					node.Draw();
				}
			}
		}

		private void DrawConnections()
		{
			if (_connections != null)
			{
				foreach (var con in _connections)
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
			if (_nodes != null)
			{
				for (int i = _nodes.Count - 1; i >= 0; i--)
				{
					bool guiChanged = _nodes[i].ProcessEvents(e);

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
			genericMenu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode(mousePosition));
			genericMenu.ShowAsContext();
		}

		private void OnClickAddNode(Vector2 mousePosition)
		{
			if (_nodes == null)
			{
				_nodes = new List<SDFNode>();
			}

			_nodes.Add(new SDFNode(mousePosition, 200, 50,
				_nodeStyle, _selectedNodeStyle,
				_inPointStyle, _outPointStyle, OnClickInPoint, OnClickOutPoint,
				OnClickRemoveNode));
		}

		private void OnDrag(Vector2 delta)
		{
			_drag = delta;

			if (_nodes != null)
			{
				foreach (var node in _nodes)
				{
					node.Drag(delta);
				}
			}

			GUI.changed = true;
		}

		private void OnClickRemoveNode(SDFNode node)
		{
			if (_connections != null)
			{
				List<Connection> connectionsToRemove = new List<Connection>();

				foreach (var connection in _connections)
				{
					if (connection.inPoint == node.inPoint || connection.outPoint == node.outPoint)
					{
						connectionsToRemove.Add(connection);
					}
				}

				foreach (var conToRemove in connectionsToRemove)
				{
					_connections.Remove(conToRemove);
				}

				connectionsToRemove = null;
			}
			_nodes.Remove(node);
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
			_connections.Remove(connection);
		}

		private void CreateConnection()
		{
			if (_connections == null)
			{
				_connections = new List<Connection>();
			}

			_connections.Add(new Connection(_selectedInPoint, _selectedOutPoint, OnClickRemoveConnection));
		}

		private void ClearConnectionSelection()
		{
			_selectedInPoint = null;
			_selectedOutPoint = null;
		}

		private void Save()
		{
			if (!_currentAsset)
			{
				return;
			}

			_currentAsset.sdfNodeData = new List<SDFNodeData>();
			_currentAsset.connectionData = new List<ConnectionData>();
			foreach (var node in _nodes)
			{
				_currentAsset.sdfNodeData.Add(node.Data);
			}
			foreach (var connection in _connections)
			{
				_currentAsset.connectionData.Add(connection.Data);
			}
		}

		private void Load()
		{
			//TODO: Prompt user to save or discard if there are current changes?
			_nodes = new List<SDFNode>();
			_connections = new List<Connection>();

			if (!_currentAsset)
			{
				return;
			}

			foreach(var node in _currentAsset.sdfNodeData)
			{
				_nodes.Add(new SDFNode(node,
				_nodeStyle, _selectedNodeStyle,
				_inPointStyle, _outPointStyle, OnClickInPoint, OnClickOutPoint,
				OnClickRemoveNode));
			}

			foreach (var connection in _currentAsset.connectionData)
			{
				var inPoint = _nodes.First(n => n.inPoint.id == connection.inPoint.id).inPoint;
				var outPoint = _nodes.First(n => n.outPoint.id == connection.outPoint.id).outPoint;
				_connections.Add(new Connection(inPoint, outPoint, OnClickRemoveConnection));
			}
		}
	}
}