using System;
using UnityEditor;
using UnityEngine;

namespace SDFEditor
{
	[Serializable]
	public class Connection
	{
		public string inId;
		public string outid;

		[NonSerialized]public ConnectionPoint inPoint;
		[NonSerialized] public ConnectionPoint outPoint;
		public Action<Connection> OnClickRemoveConnection;

		//Used when constructing a new connection
		public Connection(ConnectionPoint inPoint, ConnectionPoint outPoint)
		{
			this.inPoint = inPoint;
			this.outPoint = outPoint;
			inId = inPoint.id;
			outid = outPoint.id;
		}

		public virtual void OnAfterDeserialize(SDFEditorGraph graph)
		{
			graph.connectionPointMapping.TryGetValue(inId, out inPoint);
			graph.connectionPointMapping.TryGetValue(outid, out outPoint);
		}

		public void Draw(SDFEditor editor)
		{
			Handles.DrawBezier(
				inPoint.rect.center,
				outPoint.rect.center,
				inPoint.rect.center + Vector2.left * 50f,
				outPoint.rect.center - Vector2.left * 50f,
				Color.white,
				null,
				2f
			);

			if (Handles.Button((inPoint.rect.center + outPoint.rect.center) * 0.5f,
					Quaternion.identity,4, 8, Handles.RectangleHandleCap))
			{
				OnClickRemoveConnection?.Invoke(this);
				editor.OnClickRemoveConnection(this);
			}
		}
	}
}