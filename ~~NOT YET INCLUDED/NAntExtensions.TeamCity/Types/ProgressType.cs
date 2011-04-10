namespace NAntExtensions.TeamCity.Types
{
	/// <summary>
	/// The progress type to report to TeamCity.
	/// </summary>
	public enum ProgressType
	{
		/// <summary>
		/// A single progress message.
		/// </summary>
		/// <remarks>
		/// This progress message will be shown until another progress message occurs or until next target starts (in case of NAnt
		/// builds).
		/// </remarks>
		Message,
		/// <summary>
		/// The progress message when a part of a build starts. The same message should be used for both <see cref="Start"/> and 
		/// <see cref="End"/>. This allows nesting of progress blocks. Note that the TeamCity NAnt build runner will
		/// automatically set the status message when a NAnt target starts.
		/// </summary>
		Start,
		/// <summary>
		/// The progress message when a part of a build ends. The same message should be used for both <see cref="Start"/> and 
		/// <see cref="End"/>. This allows nesting of progress blocks. Note that the TeamCity NAnt build runner will
		/// automatically set the status message when a NAnt target starts.
		/// </summary>
		End
	}
}