using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

namespace SDFEditor
{
	public class SDFNode_IntField : SDFNode
	{
		public int Value;

		public override void Setup(Vector2 position, SDFEditorGraph graph)
		{
			base.Setup(position, graph);
			AddConnectionPoint<ConnectionPoint_Int, int>(ConnectionPointType.Out);
		}

		protected override void OnFinishedInit()
		{
			title = "Int";
			rect = new Rect(rect.x, rect.y, 80, 50);
		}

		public override void Draw(SDFEditor editor)
		{
			base.Draw(editor);

			Vector2 previewSize = new Vector2(50, 20);

			Rect previewRect = new Rect(contentRect.center - previewSize / 2f, previewSize);
			Value = EditorGUI.IntField(previewRect, Value);
		}

		public override object EvaluateNode(IConnectionPoint point)
		{
			return Value;
		}
	}
}
