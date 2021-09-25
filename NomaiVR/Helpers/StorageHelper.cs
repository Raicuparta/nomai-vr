namespace NomaiVR.Helpers
{
	public static class StorageHelper
	{
		public static T Load<T>(string filename) =>
			JsonHelper.LoadJsonObject<T>(NomaiVR.ModFolderPath + filename);

		public static void Save<T>(T obj, string filename) =>
			JsonHelper.SaveJsonObject(NomaiVR.ModFolderPath + filename, obj);
	}
}
