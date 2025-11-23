using UnityEditor;

namespace InsaneOne.Core.Development
{
	[CustomEditor(typeof(TeamsSettings))]
	public sealed class TeamsSettingsEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			var teamsSettings = target as TeamsSettings;

			var rules = teamsSettings!.GetAllRulesInternal();

			for (var index = 0; index < rules.Length; index++)
			{
				var ruleA = rules[index];

				for (var j = index + 1; j < rules.Length; j++)
				{
					var ruleB = rules[j];

					if (ruleA.IsSame(ruleB))
						EditorGUILayout.HelpBox(
							$"There is duplicated rule for: Team A == {ruleA.teamA}, Team B == {ruleA.teamB}. Recommended to remove it",
							MessageType.Warning);
				}

				if (ruleA.teamA == ruleA.teamB)
					EditorGUILayout.HelpBox($"There is same teams in rule with index {index}.", MessageType.Warning);
			}

			DrawDefaultInspector();
		}
	}
}