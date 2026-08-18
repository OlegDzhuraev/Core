namespace InsaneOne.Core.Goals
{
	/// <summary> Runtime progress snapshot of a single configured goal. </summary>
	public readonly struct GoalProgress
	{
		public readonly GoalDefinition Definition;
		public readonly int Current;

		public string Id => Definition.Id;
		public string Title => Definition.Title;
		public int Required => Definition.RequiredAmount;
		public bool IsCompleted => Current >= Required;

		/// <summary> Progress from 0 to 1. </summary>
		public float Progress01 => Required != 0 ? (float) Current / Required : 0f;

		public GoalProgress(GoalDefinition definition, int current)
		{
			Definition = definition;
			Current = current;
		}
	}
}
