using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDFEditor
{
	public class SDFNode_Output : SDFNode
	{
		public SDFNode_Output()
		{
			outPoints.Add(new ConnectionPoint(this, ConnectionPointType.In));
			outPoints.Add(new ConnectionPoint(this, ConnectionPointType.In));
			outPoints.Add(new ConnectionPoint(this, ConnectionPointType.In));
			outPoints.Add(new ConnectionPoint(this, ConnectionPointType.In));
		}

		protected override void OnFinishedInit()
		{
			title = "Output";
			rect = new Rect(rect.x, rect.y, 80, 200);
		}
	}
}
