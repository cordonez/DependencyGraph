namespace Cordonez.DependencyGraph.Editor
{
	using System;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;

	internal static class DependencyGraphAssetDrawer
	{
		//Constants to draw
		private static readonly Color m_assetBackgroundColor = new Color(0, 0, 0, 0.05f);
		private static readonly Color m_selectedBackgroundColor = new Color(1, 0.92f, 0.016f, 0.05f);
		private const float KNOB_RADIUS = 15;
		private const float KNOB_OUTLINE = 1;
		private static readonly Vector2 m_knobSize = new Vector2(KNOB_RADIUS, 0);
		private static readonly Vector2 m_tangent = Vector2.right * 100;

		//Internal lists to draw the knobs
		private static readonly List<Vector2> m_referenceKnobs = new List<Vector2>();
		private static readonly List<Vector2> m_dependencyKnobs = new List<Vector2>();
		private static Vector2 m_mainLeftKnob;
		private static Vector2 m_mainRightKnob;

		private enum Knob
		{
			Left,
			Right
		}

		private enum AssetType
		{
			Reference,
			Selected,
			Dependency
		}

		private static Vector2 m_scrollViewPosition;

		internal static void DrawGraphForActiveObject()
		{
			if (DependencyGraphManager.IsDirty)
			{
				return;
			}

			ClearData();
			UnityEngine.Object activeObject = Selection.activeObject;
			if (activeObject == null)
			{
				GUILayout.FlexibleSpace();
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("No asset selected.", MessageType.Info);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.FlexibleSpace();
				return;
			}

			DependencyGraphAsset graphAsset = DependencyGraphManager.GetSelectedAsset(AssetDatabase.GetAssetPath(activeObject));
			if (graphAsset == null)
			{
				GUILayout.FlexibleSpace();
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.HelpBox("Asset not analyzed.", MessageType.Info);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.FlexibleSpace();
				return;
			}

			m_scrollViewPosition = GUILayout.BeginScrollView(m_scrollViewPosition);
			GUILayout.BeginHorizontal();
			{
				GUILayout.FlexibleSpace();
				GUILayout.BeginVertical();
				{
					//Draw references
					foreach (string reference in graphAsset.ReferencesPaths)
					{
						DrawAsset(DependencyGraphManager.GetSelectedAsset(reference), AssetType.Reference);
					}
				}
				GUILayout.EndVertical();
				GUILayout.Label("", GUILayout.MinWidth(KNOB_RADIUS * 8));
				GUILayout.FlexibleSpace();
				GUILayout.BeginVertical();
				{
					//Draw current asset
					DrawAsset(graphAsset, AssetType.Selected);
				}
				GUILayout.EndVertical();
				GUILayout.Label("", GUILayout.MinWidth(KNOB_RADIUS * 8));
				GUILayout.FlexibleSpace();
				GUILayout.BeginVertical();
				{
					//Draw dependencies
					foreach (string dependency in graphAsset.DependenciesPaths)
					{
						DrawAsset(DependencyGraphManager.GetSelectedAsset(dependency), AssetType.Dependency);
					}
				}
				GUILayout.EndVertical();
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();
			GUILayout.EndScrollView();
		}

		private static void ClearData()
		{
			m_dependencyKnobs.Clear();
			m_referenceKnobs.Clear();
			m_mainLeftKnob = Vector2.zero;
			m_mainRightKnob = Vector2.zero;
		}

		private static void DrawAsset(DependencyGraphAsset _graphAsset, AssetType _assetType)
		{
			if (_graphAsset == null)
			{
				return;
			}

			GUILayout.FlexibleSpace();
			GUILayout.Label("");
			DrawRectangleBackground(_assetType);
			DrawAssetInformation(_graphAsset);
			DrawKnobs(_graphAsset, _assetType);
			DrawConnections();
			GUILayout.Label("");
			GUILayout.FlexibleSpace();
		}

		private static void DrawRectangleBackground(AssetType _assetType)
		{
			Rect lastRect = GUILayoutUtility.GetLastRect();
			lastRect.y += EditorGUIUtility.singleLineHeight - 10;
			lastRect.height += EditorGUIUtility.singleLineHeight * 2 + 10;
			Handles.DrawSolidRectangleWithOutline(lastRect, _assetType == AssetType.Selected ? m_selectedBackgroundColor : m_assetBackgroundColor, Color.black);
		}

		private static void DrawAssetInformation(DependencyGraphAsset _graphAsset)
		{
			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("", GUILayout.MinWidth(KNOB_RADIUS));
				GUILayout.BeginVertical();
				{
					UnityEngine.Object myAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(_graphAsset.Path);
					if (GUILayout.Button("Select"))
					{
						Selection.activeObject = myAsset;
						EditorUtility.FocusProjectWindow();
					}

					EditorGUILayout.ObjectField(myAsset, typeof(UnityEngine.Object), false);
				}
				GUILayout.EndVertical();
				GUILayout.Label("", GUILayout.MinWidth(KNOB_RADIUS));
			}
			GUILayout.EndHorizontal();
		}

		private static void DrawKnobs(DependencyGraphAsset _graphAsset, AssetType _assetType)
		{
			Rect lastRect = GUILayoutUtility.GetLastRect();
			Vector2 knobCenter = new Vector2(lastRect.x, lastRect.y);
			knobCenter.y += lastRect.height / 2;
			//Left knob
			if (_assetType == AssetType.Selected || _assetType == AssetType.Dependency)
			{
				DrawKnob(knobCenter, _graphAsset.ReferencesPaths.Count);
				SaveKnobToDrawConnections(knobCenter, _assetType, Knob.Left);
			}

			//Right knob
			if (_assetType == AssetType.Reference || _assetType == AssetType.Selected)
			{
				knobCenter.x += lastRect.width;
				DrawKnob(knobCenter, _graphAsset.DependenciesPaths.Count);
				SaveKnobToDrawConnections(knobCenter, _assetType, Knob.Right);
			}
		}

		private static void SaveKnobToDrawConnections(Vector2 _knobPosition, AssetType _assetType, Knob _knob)
		{
			switch (_assetType)
			{
				case AssetType.Reference:
					m_referenceKnobs.Add(_knobPosition);
					break;
				case AssetType.Selected:
					if (_knob == Knob.Left)
					{
						m_mainLeftKnob = _knobPosition;
					}
					else
					{
						m_mainRightKnob = _knobPosition;
					}

					break;
				case AssetType.Dependency:
					m_dependencyKnobs.Add(_knobPosition);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(_assetType), _assetType, null);
			}
		}

		private static void DrawKnob(Vector2 _knobCenter, int _number)
		{
			GUIStyle guiStyleFont = new GUIStyle {alignment = TextAnchor.MiddleCenter};
			Rect textRect = new Rect(_knobCenter.x - KNOB_RADIUS, _knobCenter.y - KNOB_RADIUS, KNOB_RADIUS * 2, KNOB_RADIUS * 2);
			Color originalColor = Handles.color;

			//Outline
			Handles.color = Color.black;
			Handles.DrawSolidDisc(_knobCenter, Vector3.forward, KNOB_RADIUS + KNOB_OUTLINE);
			Handles.color = originalColor;

			//Knob
			Handles.color = Color.gray;
			Handles.DrawSolidDisc(_knobCenter, Vector3.forward, KNOB_RADIUS);
			Handles.color = originalColor;

			//Text
			originalColor = GUI.color;
			GUI.color = Color.black;
			GUI.Label(textRect, _number.ToString(), guiStyleFont);
			GUI.color = originalColor;
		}

		private static void DrawConnections()
		{
			if (m_mainLeftKnob == Vector2.zero || m_mainRightKnob == Vector2.zero)
			{
				return;
			}

			foreach (Vector2 referenceKnob in m_referenceKnobs)
			{
				Handles.DrawBezier(
					referenceKnob + m_knobSize, m_mainLeftKnob - m_knobSize,
					referenceKnob + m_tangent, m_mainLeftKnob - m_tangent,
					Color.black, null, 2);
			}

			foreach (Vector2 dependencyKnob in m_dependencyKnobs)
			{
				Handles.DrawBezier(
					m_mainRightKnob + m_knobSize, dependencyKnob - m_knobSize,
					m_mainRightKnob + m_tangent, dependencyKnob - m_tangent,
					Color.black, null, 2);
			}
		}
	}
}