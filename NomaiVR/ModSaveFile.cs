using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NomaiVR {
    public class ModSaveFile {
        public const string FileName = "save.json";
        public List<string> tutorialSteps = new List<string>();

        public void AddTutorialStep (string step) {
            tutorialSteps.Add(step);
            NomaiVR.Log("saving............" + String.Join(", ", tutorialSteps.ToArray()));
            Save();
        }

        void Save () {
            NomaiVR.Helper.Storage.Save(this, FileName);
        }
    }
}
