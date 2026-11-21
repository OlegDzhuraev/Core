namespace InsaneOne.Core.Goals
{
	/// <summary> Runtime progress snapshot of a single configured goal. </summary>
	public readonly struct GoalProgress
	{
		public readonly GoalDefinition Definition;
		public readonly int Current;

		public string Id => Definition != null ? Definition.Id : string.Empty;
		public string Title => Definition != null ? Definition.Title : string.Empty;
		public int Required => Definition != null ? Definition.RequiredAmount : 0;
		public bool IsCompleted => Definition != null && Current >= Required;

		/// <summary> Progress from 0 to 1. </summary>
		public float Progress01 => Required != 0 ? (float) Current / Required : 0f;

		public GoalProgress(GoalDefinition definition, int current)
		{
			Definition = definition;
			Current = current;
		}
	}
}
