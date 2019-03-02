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
		[NonSerialized]
		public SDFEditorGraph graph;

		public Rect rect;
		public Rect titleRect;
		public string title;

		//Fields are not serialized by default
		public bool IsDragged { get; private set; }
		public bool IsSelected { get; private set; }

		public List<ConnectionPoint> inPoints = new List<ConnectionPoint>();
		public List<ConnectionPoint> outPoints = new List<ConnectionPoint>();

		public Action<SDFNode> OnRemoveNode;

		//Used for creating new Nodes
		public void Init(Vector2 position)
		{
			rect = new Rect(position.x, position.y, 10, 10);
			Init();
		}

		//Used for loading nodes
		private void Init()
		{
			OnFinishedInit();
		}

		//Use this to initialize content
		protected abstract void OnFinishedInit();

		//Called after graph asses was deserialized
		public virtual void OnAfterDeserialize(SDFEditorGraph graph)
		{
			Init();

			this.graph = graph;
			foreach(var connectionPoint in inPoints)
			{
				connectionPoint.OnAfterDeserialize(this);
			}
			foreach (var connectionPoint in outPoints)
			{
				connectionPoint.OnAfterDeserialize(this);
			}
		}

		public void Drag(Vector2 delta)
		{
			rect.position += delta;
		}

		public virtual void Draw(SDFEditor editor)
		{
			GUI.Box(rect, "");

			const int titelHeight = 20;

			titleRect = new Rect(rect.x, rect.y, rect.width, titelHeight);

			GUI.Box(titleRect,title);

			Rect contentRect = rect;
			contentRect.height -= titelHeight;

			Rect inRect = rect;
			inRect.width = 20;

			Rect outRect = inRect;
			outRect.x = rect.x + rect.width - inRect.width;

			int i = 0;
			foreach (var point in inPoints)
			{
				point.Draw(i++, contentRect, editor);
			}

			int j = 0;
			foreach (var point in outPoints)
			{
				point.Draw(j++, contentRect, editor);
			}
		}

		public bool ProcessEvents(Event e, SDFEditor editor)
		{
			switch (e.type)
			{
				case EventType.MouseDown:
					if (e.button == 0)
					{
						if (rect.Contains(e.mousePosition))
						//if (titleRect.Contains(e.mousePosition))
						{
							IsDragged = true;
							GUI.changed = true;
							IsSelected = true;
						}
						//else if(rect.Contains(e.mousePosition))
						//{
						//	GUI.changed = true;
						//	IsSelected = true;
						//}
						else
						{
							GUI.changed = true;
							IsSelected = false;
						}
					}
					else if (e.button == 1 && IsSelected && rect.Contains(e.mousePosition))
					{
						ProcessContextMenu(editor);
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

		private void ProcessContextMenu(SDFEditor editor)
		{
			GenericMenu genericMenu = new GenericMenu();
			genericMenu.AddItem(new GUIContent("Remove node"), false, ()=>OnClickRemoveNode(editor));
			AddOptionsToContextMenu(genericMenu);
			genericMenu.ShowAsContext();
		}

		//override this to add custom Context menu options
		protected virtual void AddOptionsToContextMenu(GenericMenu menu) { }

		private void OnClickRemoveNode(SDFEditor editor)
		{
			OnRemoveNode?.Invoke(this);
			editor.OnClickRemoveNode(this);
		}
	}
}