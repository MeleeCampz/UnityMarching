using System;
using UnityEngine;

namespace SDFEditor
{
	public enum ConnectionPointType { In, Out }

	[Serializable]
	public class ConnectionPoint
	{
		public string id;

		[NonSerialized]public Rect rect;
		[NonSerialized] public ConnectionPointType type;

		[NonSerialized] public SDFNode node;
		[NonSerialized] public GUIStyle style;

		public Action<ConnectionPoint> OnClickConnectionPoint;

		public ConnectionPoint(SDFNode node, ConnectionPointType type, GUIStyle style,
			Action<ConnectionPoint> OnClickConnectionPoint, string id = null)
		{
			this.node = node;
			this.type = type;
			this.style = style;
			this.OnClickConnectionPoint = OnClickConnectionPoint;
			rect = new Rect(0, 0, 10f, 20f);

			this.id = id ?? Guid.NewGuid().ToString();
		}

		public void Draw(int index)
		{
			rect.y = node.rect.y + (node.rect.height * 0.5f) - rect.height * 0.5f + rect.height * index;

			switch (type)
			{
				case ConnectionPointType.In:
					rect.x = node.rect.x - rect.width + 8f;
					break;

				case ConnectionPointType.Out:
					rect.x = node.rect.x + node.rect.width - 8f;
					break;
			}

			if (GUI.Button(rect, "", style))
			{
				OnClickConnectionPoint?.Invoke(this);
			}
		}
	}
}