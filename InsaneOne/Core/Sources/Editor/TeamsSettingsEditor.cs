#if INSANE_TEAMS_EXTENSION
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneOne.Core.Teams.Development
{
	[CustomEditor(typeof(TeamsSettings))]
	public sealed class TeamsSettingsEditor : Editor
	{
		const string StylesPath = "InsaneOne/ToolsStyles";
		const string GroupStyleName = "group-box";
		const string RulesFieldName = "enemiesTeamsRules";
		const string TeamInfosFieldName = "teamInfos";
		const float RowLabelWidth = 110f;
		const float CellWidth = 26f;

		SerializedProperty rulesProperty;
		VisualElement matrixContainer;

		void OnEnable() => rulesProperty = serializedObject.FindProperty(RulesFieldName);

		public override VisualElement CreateInspectorGUI()
		{
			var root = new VisualElement();

			var stylesheet = Resources.Load<StyleSheet>(StylesPath);
			if (stylesheet)
				root.styleSheets.Add(stylesheet);

			root.Add(new PropertyField(serializedObject.FindProperty(TeamInfosFieldName)));

			matrixContainer = new VisualElement();
			matrixContainer.AddToClassList(GroupStyleName);
			root.Add(matrixContainer);

			root.Bind(serializedObject);
			root.TrackSerializedObjectValue(serializedObject, _ => RefreshMatrix());

			RefreshMatrix();

			return root;
		}

		// Rebuilt from scratch on every change (own toggle clicks included, via TrackSerializedObjectValue) rather than
		// patched in place - team count can grow/shrink from the Team Infos list above at any time, and a full rebuild
		// is the simplest way to keep the grid size and every cell's value correct without tracking stale indices.
		void RefreshMatrix()
		{
			matrixContainer.Clear();

			var teamsSettings = (TeamsSettings) target;
			var teamCount = teamsSettings.GetTeamCount();

			matrixContainer.Add(new Label("Enemy Relations") { style = { unityFontStyleAndWeight = FontStyle.Bold } });

			if (teamCount < 2)
			{
				matrixContainer.Add(new HelpBox("Add at least 2 entries to Team Infos above to configure enemy relations.", HelpBoxMessageType.Info));
				return;
			}

			var header = new VisualElement { style = { flexDirection = FlexDirection.Row } };
			header.Add(new Label { style = { width = RowLabelWidth } });

			for (var col = 0; col < teamCount; col++)
				header.Add(new Label(col.ToString())
				{
					style = { width = CellWidth, unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter) },
				});

			matrixContainer.Add(header);

			for (var row = 0; row < teamCount; row++)
				matrixContainer.Add(BuildRow(teamsSettings, row, teamCount));

			AppendDuplicateWarnings(teamsSettings);
		}

		VisualElement BuildRow(TeamsSettings teamsSettings, int row, int teamCount)
		{
			var rowElement = new VisualElement { style = { flexDirection = FlexDirection.Row } };
			rowElement.Add(new Label(teamsSettings.GetTeamName(row)) { style = { width = RowLabelWidth } });

			for (var col = 0; col < teamCount; col++)
			{
				// Relations are symmetric and a team is never its own enemy - only the upper triangle needs a control.
				if (col <= row)
				{
					rowElement.Add(new VisualElement { style = { width = CellWidth } });
					continue;
				}

				// Toggle reserves internal space for a label even when it has none, so a plain width on it doesn't
				// center the checkbox under the header number - wrap it in a cell that centers its content instead.
				var cell = new VisualElement();
				cell.AddToClassList("teams-matrix-cell");

				var toggle = new Toggle
				{
					value = teamsSettings.IsEnemies(row, col),
					tooltip = "Is enemies?",
				};
				toggle.AddToClassList("teams-matrix-toggle");

				var capturedRow = row;
				var capturedCol = col;
				toggle.RegisterValueChangedCallback(evt => SetEnemy(capturedRow, capturedCol, evt.newValue));

				cell.Add(toggle);
				rowElement.Add(cell);
			}

			return rowElement;
		}

		void SetEnemy(int teamA, int teamB, bool isEnemy)
		{
			serializedObject.Update();

			var existingIndex = FindRuleIndex(teamA, teamB);

			if (isEnemy && existingIndex < 0)
			{
				rulesProperty.InsertArrayElementAtIndex(rulesProperty.arraySize);
				var newRule = rulesProperty.GetArrayElementAtIndex(rulesProperty.arraySize - 1);
				newRule.FindPropertyRelative("teamA").intValue = teamA;
				newRule.FindPropertyRelative("teamB").intValue = teamB;
			}
			else if (!isEnemy && existingIndex >= 0)
			{
				rulesProperty.DeleteArrayElementAtIndex(existingIndex);
			}

			serializedObject.ApplyModifiedProperties(); // registers Undo and triggers the TrackSerializedObjectValue refresh
		}

		int FindRuleIndex(int teamA, int teamB)
		{
			for (var i = 0; i < rulesProperty.arraySize; i++)
			{
				var element = rulesProperty.GetArrayElementAtIndex(i);
				var a = element.FindPropertyRelative("teamA").intValue;
				var b = element.FindPropertyRelative("teamB").intValue;

				if ((a == teamA && b == teamB) || (a == teamB && b == teamA))
					return i;
			}

			return -1;
		}

		// Safety net for rules that didn't come from the matrix above (hand-edited asset, merge conflict, script) -
		// the matrix itself can't create duplicates or self-relations since it only offers one control per team pair.
		void AppendDuplicateWarnings(TeamsSettings teamsSettings)
		{
			var rules = teamsSettings.GetAllRulesInternal();

			for (var index = 0; index < rules.Length; index++)
			{
				var ruleA = rules[index];

				for (var j = index + 1; j < rules.Length; j++)
					if (ruleA.IsSame(rules[j]))
						matrixContainer.Add(new HelpBox($"Duplicated rule for Team {ruleA.teamA} / Team {ruleA.teamB}.", HelpBoxMessageType.Warning));

				if (ruleA.teamA == ruleA.teamB)
					matrixContainer.Add(new HelpBox($"Rule at index {index} refers to the same team twice.", HelpBoxMessageType.Warning));
			}
		}
	}
}
#endif
