using System;
using UnityEngine;

namespace SDFEditor
{
	[Serializable]
	public abstract class ConnectionPoint_Generic<T> : ScriptableObject, IConnectionPoint
	{
		public string id;
		public ConnectionPointType type;

		[NonSerialized]public Rect rect;
		[NonSerialized] public SDFNode node;

		public Action<ConnectionPoint_Generic<T>> OnClickConnectionPoint;

		//only Used for newly created connection points
		public void Init(SDFNode node, ConnectionPointType type)
		{
			this.node = node;
			this.type = type;

			id = Guid.NewGuid().ToString();
			Init();
		}

		public virtual void OnAfterDeserialize(SDFNode node)
		{
			this.node = node;
			node.graph.connectionPointMapping.Add(id, this);
			Init();
		}

		public ConnectionPointType GetConnectionType()
		{
			return type;
		}

		public string GetID()
		{
			return id;
		}

		public virtual void Draw(int index, Rect contentRect, SDFEditor editor)
		{
			//rect.y = contentRect.y + (contentRect.height * 0.5f) - rect.height * 0.5f + rect.height * index;

			rect.y = contentRect.y + 5f + rect.height * index;

			switch (type)
			{
				case ConnectionPointType.In:
					rect.x = contentRect.x - rect.width;
					break;

				case ConnectionPointType.Out:
					rect.x = contentRect.x + contentRect.width;
					break;
			}

			if (GUI.Button(rect, ""))
			{
				OnClickConnectionPoint?.Invoke(this);
				if(type == ConnectionPointType.In)
				{
					editor.OnClickInPoint(this);
				}
				else
				{
					editor.OnClickOutPoint(this);
				}
			}
		}

		public SDFNode GetNode()
		{
			return node;
		}

		public Rect GetRect()
		{
			return rect;
		}

		public object GetData()
		{
			if(type == ConnectionPointType.In)
			{
				IConnectionPoint start = node.graph.GetConnectionStartPoint(this);
				if(start != null)
				{
					return start.GetData();
				}
				return GetDefaultValue();
			}

			return node.EvaluateNode(this);
		}

		protected void Init()
		{
			rect = new Rect(0, 0, 10f, 20f);
		}

		protected virtual object GetDefaultValue()
		{
			return null;
		}
	}
}