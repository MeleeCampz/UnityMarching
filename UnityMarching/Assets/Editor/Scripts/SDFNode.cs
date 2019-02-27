using System;
using UnityEditor;
using UnityEngine;

namespace SDFEditor
{
	[Serializable]
	public struct SDFNodeData
	{
		public Rect rect;
		public ConnectionPointData inPoint;
		public ConnectionPointData outPoint;
	}

	public class SDFNode
	{
		//Contains all data that needs to be serialized
		public SDFNodeData Data
		{
			//TODO: Chache this?
			get
			{
				return new SDFNodeData()
				{
					rect = rect,
					inPoint = inPoint.Data,
					outPoint = outPoint.Data
				};
			}
		}

		public Rect rect;

		public string title;
		public bool IsDragged { get; private set; }
		public bool IsSelected { get; private set; }

		public ConnectionPoint inPoint;
		public ConnectionPoint outPoint;

		public GUIStyle style;
		public GUIStyle defaultNodeStyle;
		public GUIStyle selectedNodeStyle;

		public Action<SDFNode> OnRemoveNode;

		//Used for creating new Nodes
		public SDFNode(Vector2 position, float width, float height,
			GUIStyle nodeStyle, GUIStyle selectedStyle,
			GUIStyle inPointStyle, GUIStyle outPointStyle,
			Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint,
			Action<SDFNode> OnClickRemoveNode)
		{
			rect = new Rect(position.x, position.y, width, height);

			style = nodeStyle;
			defaultNodeStyle = nodeStyle;
			selectedNodeStyle = selectedStyle;

			inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint);
			outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint);

			OnRemoveNode = OnClickRemoveNode;
		}

		//Used when loading a node from a saved file
		public SDFNode(SDFNodeData savedData,
			GUIStyle nodeStyle, GUIStyle selectedStyle,
			GUIStyle inPointStyle, GUIStyle outPointStyle,
			Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint,
			Action<SDFNode> OnClickRemoveNode)
		{
			rect = savedData.rect;

			style = nodeStyle;
			defaultNodeStyle = nodeStyle;
			selectedNodeStyle = selectedStyle;

			inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint,
				savedData.inPoint.id);
			outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint,
				savedData.outPoint.id);

			OnRemoveNode = OnClickRemoveNode;
		}

		public void Drag(Vector2 delta)
		{
			rect.position += delta;
		}

		public void Draw()
		{
			inPoint.Draw();
			outPoint.Draw();
			GUI.Box(rect, title, style);
		}

		public bool ProcessEvents(Event e)
		{
			switch (e.type)
			{
				case EventType.MouseDown:
					if (e.button == 0)
					{
						if (rect.Contains(e.mousePosition))
						{
							IsDragged = true;
							GUI.changed = true;
							IsSelected = true;
							style = selectedNodeStyle;
						}
						else
						{
							GUI.changed = true;
							IsSelected = true;
							style = defaultNodeStyle;
						}
					}
					else if (e.button == 1 && IsSelected && rect.Contains(e.mousePosition))
					{
						ProcessContextMenu();
						e.Use();
					}
					break;

				case EventType.MouseUp:
					IsDragged = false;
					break;

				case EventType.MouseDrag:
					if (e.button == 0 && IsDragged)
					{
						Drag(e.delta);
						e.Use();
						return true;
					}
					break;
			}

			return false;
		}

		private void ProcessContextMenu()
		{
			GenericMenu genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
			genericMenu.ShowAsContext();
		}

		private void OnClickRemoveNode()
		{
			OnRemoveNode?.Invoke(this);
		}
	}
}