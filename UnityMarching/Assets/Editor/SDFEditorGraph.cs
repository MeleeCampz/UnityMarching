using System;
using System.Collections;
using System.Linq;

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
		public Dictionary<string, IConnectionPoint> connectionPointMapping;

		//[HideInInspector]
		public List<SDFNode> nodes;
		//[HideInInspector]
		public List<Connection> connections;

		private void OnEnable()
		{
			Debug.Log("Graph: Deserialized!");

			connectionPointMapping = new Dictionary<string, IConnectionPoint>();
			//Always deserialize nodes first so connections can find them in the lookup table
			foreach (var node in nodes)
			{
				node.OnAfterDeserialize(this);
			}


			//After nodes are loaded connection shoudl be able to fond nodes via mapping and id
			foreach(var con in connections)
			{
				con.OnAfterDeserialize(this);
			}

		}

		/// <summary>
		/// Get the startPoint for a connection with the given end point if existent
		/// </summary>
		/// <param name="inPoint"></param>
		/// <returns></returns>
		public IConnectionPoint GetConnectionStartPoint(IConnectionPoint endPoint)
		{
			if(endPoint.GetConnectionType() != ConnectionPointType.In)
			{
				Debug.LogError("Unexpeted Connection point type");
			}

			IConnectionPoint outPoint = null;

			Connection con = connections.FirstOrDefault(x => x.outPoint == endPoint);
			if(con != null)
			{
				outPoint = con.inPoint; 
			}

			return outPoint;
		}

		/// <summary>
		/// Get the all endPoints for a connection with the given start point if existent
		/// </summary>
		/// <param name="inPoint"></param>
		/// <returns></returns>
		public List<IConnectionPoint> GetConnectionPoint(IConnectionPoint startPoint)
		{
			if (startPoint.GetConnectionType() != ConnectionPointType.In)
			{
				Debug.LogError("Unexpeted Connection point type");
			}

			List<IConnectionPoint> outPoints = new List<IConnectionPoint>();

			IEnumerable<Connection> conns = connections.Where(x => x.inPoint == startPoint);
			foreach(var connection in conns)
			{
				outPoints.Add(connection.outPoint);
			}

			return outPoints;
		}
	}
}