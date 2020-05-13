using System.Collections.Generic;

namespace NomaiVR
{
    public class ModSaveFile
    {
        public const string FileName = "save.json";
        public HashSet<string> tutorialSteps = new HashSet<string>();

        public void AddTutorialStep(string step)
        {
            tutorialSteps.Add(step);
            Save();
        }

        void Save()
        {
            NomaiVR.Helper.Storage.Save(this, FileName);
        }
    }
}
