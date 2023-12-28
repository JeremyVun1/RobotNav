namespace RobotNav.Extensions
{
    public static class CollectionExtensions
	{
		public static double GetValueOrDefaultAsDouble(this string[] collection, int index, double defaultValue)
		{
			if (collection.Length <= index)
				return defaultValue;
			
			return double.TryParse(collection[index], out double parsedValue)
				? parsedValue
				: defaultValue;
		}

		public static bool GetValueOrDefaultAsBool(this string[] collection, int index, bool defaultValue)
		{
			if (collection.Length <= index)
				return defaultValue;
			
			return bool.TryParse(collection[index], out bool parsedValue)
				? parsedValue
				: defaultValue;
		}
	}
}