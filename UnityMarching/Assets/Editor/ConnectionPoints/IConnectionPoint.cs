using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDFEditor
{
	public enum ConnectionPointType { In, Out }

	public interface IConnectionPoint
	{
		ConnectionPointType GetConnectionType();
		string GetID();
		void Draw(int index, Rect contentRect, SDFEditor editor);
		SDFNode GetNode();
		object GetData();
		Rect GetRect();
		void OnAfterDeserialize(SDFNode node);
	}
}
