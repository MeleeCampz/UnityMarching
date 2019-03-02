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
		public SDFEditor editor;
		//[HideInInspector]
		public List<SDFNode> nodes;
		//[HideInInspector]
		public List<Connection> connections;

		private void OnEnable()
		{
			Debug.Log("Graph: Deserialized!");
			foreach (var node in nodes)
			{
				node.OnAfterDeserialize(this);
			}
		}
	}
}