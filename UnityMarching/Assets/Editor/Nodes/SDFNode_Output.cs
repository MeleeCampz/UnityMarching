using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDFEditor
{
	public class SDFNode_Output : SDFNode
	{
		public override void Setup(Vector2 position, SDFEditorGraph graph)
		{
			base.Setup(position, graph);
			AddConnectionPoint<ConnectionPoint_Int, int>(ConnectionPointType.In);
		}

		protected override void OnFinishedInit()
		{
			title = "Output Node";
			rect = new Rect(rect.x, rect.y, 80, 200);
		}
	}
}
