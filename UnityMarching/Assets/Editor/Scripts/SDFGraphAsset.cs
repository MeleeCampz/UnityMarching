using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDFEditor
{
	[CreateAssetMenu(fileName = "SDFGraphAsset", menuName = "SDFGraphAsset", order = 1)]
	public class SDFGraphAsset : ScriptableObject
	{
		//[HideInInspector]
		public List<SDFNodeData> sdfNodeData;
		//[HideInInspector]
		public List<ConnectionData> connectionData;
	}
}