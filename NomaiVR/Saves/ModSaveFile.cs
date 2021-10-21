using NomaiVR.Helpers;
using System.Collections.Generic;

namespace NomaiVR
{
    public class ModSaveFile
    {
        public HashSet<string> tutorialSteps = new HashSet<string>();
        private const string _fileName = "save.json";

        public void AddTutorialStep(string step)
        {
            tutorialSteps.Add(step);
            Save();
        }

        private void Save()
        {
            StorageHelper.Save(this, _fileName);
        }

        public static ModSaveFile LoadSaveFile()
        {
            var save = StorageHelper.Load<ModSaveFile>(_fileName);
            return save ?? new ModSaveFile();
        }
    }
}
