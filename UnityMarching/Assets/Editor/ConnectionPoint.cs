using System;
using UnityEngine;

namespace SDFEditor
{
	public enum ConnectionPointType { In, Out }

	[Serializable]
	public class ConnectionPoint
	{
		public string id;
		public ConnectionPointType type;

		[NonSerialized]public Rect rect;
		[NonSerialized] public SDFNode node;

		public Action<ConnectionPoint> OnClickConnectionPoint;

		public ConnectionPoint(SDFNode node, ConnectionPointType type)
		{
			this.node = node;
			this.type = type;
			
			id = Guid.NewGuid().ToString();
			Init();
		}

		public virtual void OnAfterDeserialize(SDFNode node)
		{
			this.node = node;
			Init();
		}

		public void Draw(int index, Rect contentRect, SDFEditor editor)
		{
			rect.y = contentRect.y + (contentRect.height * 0.5f) - rect.height * 0.5f + rect.height * index;

			switch (type)
			{
				case ConnectionPointType.In:
					rect.x = contentRect.x - rect.width + 8f;
					break;

				case ConnectionPointType.Out:
					rect.x = contentRect.x + contentRect.width - 8f;
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

		private void Init()
		{
			rect = new Rect(0, 0, 10f, 20f);
		}
	}
}