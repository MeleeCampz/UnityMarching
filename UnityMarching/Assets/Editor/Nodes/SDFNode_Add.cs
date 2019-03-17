using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace SDFEditor
{
	public class SDFNode_Add : SDFNode
	{

		private int _currentVal = 10;

		public override void Setup(Vector2 position, SDFEditorGraph graph)
		{
			base.Setup(position, graph);

			AddConnectionPoint<ConnectionPoint_Int, int>(ConnectionPointType.In);
			AddConnectionPoint<ConnectionPoint_Int, int>(ConnectionPointType.In);

			AddConnectionPoint<ConnectionPoint_Int, int>(ConnectionPointType.Out);
		}

		protected override void OnFinishedInit()
		{
			title = "Add";
			rect = new Rect(rect.x, rect.y, 80, 120);
		}

		public override void Draw(SDFEditor editor)
		{
			base.Draw(editor);

			Vector2 previewSize = new Vector2(50, 20);

			//Rect previewRect = new Rect(contentRect.position + new Vector2(contentRect.width/2, 20f) - previewSize/2f, previewSize);
			//GUI.Box(previewRect, _currentVal.ToString());

			GUILayout.BeginArea(contentRect);

			EditorGUILayout.IntField(_currentVal);

			if (GUILayout.Button("Update"))
			{
				_currentVal = (int)inPoints[0].GetData() + (int)inPoints[1].GetData();
			}

			GUILayout.EndArea();
		}

		public override object EvaluateNode(IConnectionPoint point)
		{
			return base.EvaluateNode(point);
		}
	}
}
