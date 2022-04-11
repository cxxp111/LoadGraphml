using System;
using System.Collections.Generic;
using System.Text;

namespace LoadGraphml
{
    public class AutomateManager
    {
        private static AutomateManager instance;
        public static AutomateManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new AutomateManager();
                return instance;
            }
        }

        public List<AutomateScriptContext> runingAutomateScripts = new List<AutomateScriptContext>();
        private List<AutomateScriptContext> finishedAutomateScripts = new List<AutomateScriptContext>();
        private List<AutomateScriptContext> needRemoveScripts = new List<AutomateScriptContext>();
        public void LoadAutomateScript(string filePath)
        {
            AutomateScriptContext automateScript = new AutomateScriptContext();
            automateScript.LoafFromFile(filePath);
            runingAutomateScripts.Add(automateScript);
        }
        public void StartAutomateScript(string fileName)
        {

        }

        public void Update()
        {
            for (int i = 0; i < runingAutomateScripts.Count; i++)
            {
                runingAutomateScripts[i].Update();
                if (runingAutomateScripts[i].scriptFinished)
                {
                    needRemoveScripts.Add(runingAutomateScripts[i]);
                }
            }
            for (int i = 0; i < needRemoveScripts.Count; i++)
            {
                runingAutomateScripts.Remove(needRemoveScripts[i]);
            }
            needRemoveScripts.Clear();

        }
    }
}
