using System.Collections.Generic;
using NomaiVR.Helpers;

namespace NomaiVR.Saves
{
    public class ModSaveFile
    {
        public HashSet<string> TutorialSteps = new HashSet<string>();
        private const string fileName = "save.json";

        public void AddTutorialStep(string step)
        {
            TutorialSteps.Add(step);
            Save();
        }

        private void Save()
        {
            StorageHelper.Save(this, fileName);
        }

        public static ModSaveFile LoadSaveFile()
        {
            var save = StorageHelper.Load<ModSaveFile>(fileName);
            return save ?? new ModSaveFile();
        }
    }
}
