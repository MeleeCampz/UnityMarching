using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SDFEditor
{
	[CreateAssetMenu(fileName = "SDFGraphAsset", menuName = "SDFGraphAsset", order = 1)]
	[System.Serializable]
	public class SDFEditorGraph : ScriptableObject
	{
		[NonSerialized]
		//Only used to setup the connections after deserialization
		public Dictionary<string, ConnectionPoint> connectionPointMapping;

		//[HideInInspector]
		public List<SDFNode> nodes;
		//[HideInInspector]
		public List<Connection> connections;

		private void OnEnable()
		{
			Debug.Log("Graph: Deserialized!");

			connectionPointMapping = new Dictionary<string, ConnectionPoint>();
			foreach (var node in nodes)
			{
				node.OnAfterDeserialize(this);
			}
			foreach(var con in connections)
			{
				con.OnAfterDeserialize(this);
			}

		}
	}
}