using System.Globalization;

using NAnt.Core;

namespace NAntExtensions.TeamCity.Common.Helper
{
	public static class PropertyDictionaryHelper
	{
		public static void AddOrUpdateInt(PropertyDictionary instance, string key, int value)
		{
			Ensure.ArgumentIsNotNull(instance, "instance");
			Ensure.ArgumentIsNotNullOrEmptyString(key, "key");

			int parsedValue;
			if (!int.TryParse(instance[key], out parsedValue))
			{
				parsedValue = 0;
			}

			parsedValue += value;
			instance[key] = parsedValue.ToString(CultureInfo.InvariantCulture);
		}

		public static void SetInt(PropertyDictionary instance, string key, int value)
		{
			Ensure.ArgumentIsNotNull(instance, "instance");
			Ensure.ArgumentIsNotNullOrEmptyString(key, "key");

			string valueToSet = value.ToString(CultureInfo.InvariantCulture);
			if (instance.Contains(key))
			{
				instance[key] = valueToSet;
			}
			else
			{
				instance.Add(key, valueToSet);
			}
		}
	}
}