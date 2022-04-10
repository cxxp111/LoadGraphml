using System;
using System.Collections.Generic;
using System.Text;

namespace LoadGraphml
{
    public class AutomateScriptManager
    {
        private AutomateScriptManager instance;
        public AutomateScriptManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new AutomateScriptManager();
                return instance;
            }
        }
        public delegate bool AutomateHandle(AutomateComponent automateComponent);
        public Dictionary<string, AutomateHandle> AutomateHandles = new Dictionary<string, AutomateHandle>();

        public AutomateScriptManager()
        {
            this.AutomateHandles.Add("LoadPlayer", new AutomateHandle(LoadPlayer));
        }
        public bool LoadPlayer(AutomateComponent component) {
            return true;
        }
        public bool PlayerAttack(AutomateComponent component)
        {
            return true;
        }
        public bool CheckPlayer(AutomateComponent component) {
            return true;
        }

    }
}
