using System;

namespace Snak.Utilities
{
	/// <summary>
	/// Summary description for ResourceUtils.
	/// </summary>
	internal class ResourceUtils
	{
		private ResourceUtils(){}

		internal static System.IO.Stream GetResourceStream(Type type, string name)
		{
			return type.Assembly.GetManifestResourceStream(type, name);
		}
	}
}
