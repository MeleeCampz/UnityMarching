using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace SDFEditor
{
	//Making the base class a scriptable object has quite some overhead in terms of overhead.
	//The upside is that we can serialze derived classes properly
	[Serializable]
	public abstract class SDFNode : ScriptableObject
	{
		public Rect rect;
		public string title;

		//Fields are not serialized by default
		public bool IsDragged { get; private set; }
		public bool IsSelected { get; private set; }

		public List<ConnectionPoint> inPoints = new List<ConnectionPoint>();
		public List<ConnectionPoint> outPoints = new List<ConnectionPoint>();


		[NonSerialized] protected GUIStyle style;
		[NonSerialized] protected GUIStyle defaultNodeStyle;
		[NonSerialized] protected GUIStyle selectedNodeStyle;

		[NonSerialized] protected GUIStyle inPointStyle;
		[NonSerialized] protected GUIStyle outPointStyle;
		protected Action<ConnectionPoint> OnClickInPoint;
		protected Action<ConnectionPoint> OnClickOutPoint;

		public Action<SDFNode> OnRemoveNode;

		//Used for creating new Nodes
		public void Init(Vector2 position, GUIStyle nodeStyle, GUIStyle selectedStyle,
			GUIStyle inPointStyle, GUIStyle outPointStyle,
			Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint, Action<SDFNode> OnClickRemoveNode)
		{
			rect = new Rect(position.x, position.y, 10, 10);
			title = "renameMe!";

			Init(nodeStyle, selectedStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode);
		}

		//Used for loading nodes
		public void Init(GUIStyle nodeStyle, GUIStyle selectedStyle,
			GUIStyle inPointStyle, GUIStyle outPointStyle,
			Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint, Action<SDFNode> OnClickRemoveNode)
		{
			style = nodeStyle;
			defaultNodeStyle = nodeStyle;
			selectedNodeStyle = selectedStyle;

			this.inPointStyle = inPointStyle;
			this.outPointStyle = outPointStyle;

			this.OnClickInPoint = OnClickInPoint;
			this.OnClickOutPoint = OnClickOutPoint;

			OnRemoveNode = OnClickRemoveNode;

			OnFinishedInit();
		}

		//Use this to initialize content
		protected abstract void OnFinishedInit();


		public void Drag(Vector2 delta)
		{
			rect.position += delta;
		}

		public virtual void Draw()
		{
			GUI.Box(rect, title, style);

			Rect inRect = rect;
			inRect.width = 20;

			Rect outRect = inRect;
			outRect.x = rect.x + rect.width - inRect.width;

			int i = 0;
			foreach (var point in inPoints)
			{
				point.Draw(i++);
			}

			int j = 0;
			foreach (var point in outPoints)
			{
				point.Draw(j++);
			}
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