using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDFEditor
{
	public class SDFNode_Output : SDFNode
	{
		protected override void OnFinishedInit()
		{
			rect = new Rect(rect.x, rect.y, 120, 300);

			inPoints.Add(new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint));
			outPoints.Add(new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint));
			outPoints.Add(new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint));
		}
	}
}
