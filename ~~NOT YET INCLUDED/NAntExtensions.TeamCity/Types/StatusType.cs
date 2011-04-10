namespace NAntExtensions.TeamCity.Types
{
	/// <summary>
	/// The build status to report to TeamCity.
	/// </summary>
	public enum StatusType
	{
		/// <summary>
		/// The build failed.
		/// </summary>
		Failure,
		/// <summary>
		/// The build is successful.
		/// </summary>
		Success
	}
}