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

		[NonSerialized]public IConnectionPoint inPoint;
		[NonSerialized] public IConnectionPoint outPoint;
		public Action<Connection> OnClickRemoveConnection;

		//Used when constructing a new connection
		public Connection(IConnectionPoint inPoint, IConnectionPoint outPoint)
		{
			this.inPoint = inPoint;
			this.outPoint = outPoint;
			inId = inPoint.GetID();
			outid = outPoint.GetID();
		}

		public virtual void OnAfterDeserialize(SDFEditorGraph graph)
		{
			graph.connectionPointMapping.TryGetValue(inId, out inPoint);
			graph.connectionPointMapping.TryGetValue(outid, out outPoint);
		}

		public void Draw(SDFEditor editor)
		{
			Rect outRect = outPoint.GetRect();
			Rect inRect = inPoint.GetRect();

			Handles.DrawBezier(
				outRect.center,
				inRect.center,
				outRect.center + Vector2.left * 50f,
				inRect.center - Vector2.left * 50f,
				Color.white,
				null,
				2f
			);

			if (Handles.Button((inRect.center + outRect.center) * 0.5f,
					Quaternion.identity,4, 8, Handles.RectangleHandleCap))
			{
				OnClickRemoveConnection?.Invoke(this);
				editor.OnClickRemoveConnection(this);
			}
		}
	}
}