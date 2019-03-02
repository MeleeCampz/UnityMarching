using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDFEditor
{
	public class SDFNode_Example : SDFNode
	{
		public SDFNode_Example()
		{
			inPoints.Add(new ConnectionPoint(this, ConnectionPointType.In));
			inPoints.Add(new ConnectionPoint(this, ConnectionPointType.In));
			inPoints.Add(new ConnectionPoint(this, ConnectionPointType.In));
			outPoints.Add(new ConnectionPoint(this, ConnectionPointType.Out));
			outPoints.Add(new ConnectionPoint(this, ConnectionPointType.Out));
		}


		protected override void OnFinishedInit()
		{
			title = "Example";
			rect = new Rect(rect.x, rect.y, 80, 200);
		}
	}
}
