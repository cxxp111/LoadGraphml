using System;
using System.Collections.Generic;
using System.Text;

namespace LoadGraphml
{
    public  class AutomateManager
    {
        private AutomateManager instance;
        public AutomateManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new AutomateManager();
                return instance;
            }
        }


    }
}
