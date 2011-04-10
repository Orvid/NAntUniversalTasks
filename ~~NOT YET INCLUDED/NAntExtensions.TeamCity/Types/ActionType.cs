namespace NAntExtensions.TeamCity.Types
{
	/// <summary>
	/// Defines how status messages are handled by TeamCity.
	/// </summary>
	public enum ActionType
	{
		/// <summary>
		/// Appends the status message to the current message.
		/// </summary>
		Append,
		/// <summary>
		/// Prepends the status message to the current message.
		/// </summary>
		Prepend,
		/// <summary>
		/// Replaces the current message with the status message.
		/// </summary>
		Replace
	}
}