﻿using System;
using UnityEditor;
using UnityEngine;

namespace SDFEditor
{
	[Serializable]
	public struct ConnectionData
	{
		public ConnectionPointData inPoint;
		public ConnectionPointData outPoint;
	}

	public class Connection
	{
		public ConnectionData Data
		{
			get
			{
				return new ConnectionData()
				{
					inPoint = inPoint.Data,
					outPoint = outPoint.Data
				};
			}
		}

		public ConnectionPoint inPoint;
		public ConnectionPoint outPoint;
		public Action<Connection> OnClickRemoveConnection;

		public Connection(ConnectionPoint inPoint, ConnectionPoint outPoint, Action<Connection> OnClickRemoveConnection)
		{
			this.inPoint = inPoint;
			this.outPoint = outPoint;
			this.OnClickRemoveConnection = OnClickRemoveConnection;
		}

		public void Draw()
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
			}
		}
	}
}