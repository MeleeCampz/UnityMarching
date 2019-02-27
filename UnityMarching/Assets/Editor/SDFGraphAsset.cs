using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDFEditor
{
	[CreateAssetMenu(fileName = "SDFGraphAsset", menuName = "SDFGraphAsset", order = 1)]
	[System.Serializable]
	public class SDFGraphAsset : ScriptableObject
	{
		//[HideInInspector]
		public List<SDFNode> nodes;
		//[HideInInspector]
		public List<Connection> connections;

		private void OnEnable()
		{
			
		}
	}
}