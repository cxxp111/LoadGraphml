using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace LoadGraphml
{
    public class AutomateScriptManager
    {
        private static AutomateScriptManager instance;
        public static AutomateScriptManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new AutomateScriptManager();
                return instance;
            }
        }
        public delegate bool AutomateHandle(AutomateScriptContext automateComponent);
        public Dictionary<string, AutomateHandle> AutomateHandles = new Dictionary<string, AutomateHandle>();

        public AutomateScriptManager()
        {
            this.AutomateHandles.Add("LoadPlayer", new AutomateHandle(LoadPlayer));
            this.AutomateHandles.Add("HideNpcHpBar", new AutomateHandle(HideNpcHpBar));
            this.AutomateHandles.Add("SetNpcsHpLock", new AutomateHandle(SetNpcsHpLock));
            this.AutomateHandles.Add("HasVictimByGuid", new AutomateHandle(HasVictimByGuid));
            this.AutomateHandles.Add("LoadAnimToObject", new AutomateHandle(LoadAnimToObject));
            this.AutomateHandles.Add("SetGameObjectAnimatorLayerWeight", new AutomateHandle(SetGameObjectAnimatorLayerWeight));
            this.AutomateHandles.Add("CheckSceneInteractiveStateWithOpt", new AutomateHandle(CheckSceneInteractiveStateWithOpt));
            this.AutomateHandles.Add("CheckSceneInteractiveState", new AutomateHandle(CheckSceneInteractiveState));
            this.AutomateHandles.Add("ChangeSceneInteractiveState", new AutomateHandle(ChangeSceneInteractiveState));
            this.AutomateHandles.Add("PlayAnimBySceneInteractiveState", new AutomateHandle(PlayAnimBySceneInteractiveState));
            this.AutomateHandles.Add("GetGameObjectCurrentAnimatorStateProgress", new AutomateHandle(GetGameObjectCurrentAnimatorStateProgress));
            this.AutomateHandles.Add("PlayMouldAnim", new AutomateHandle(PlayMouldAnim));
            this.AutomateHandles.Add("SubtractTriggerCount", new AutomateHandle(SubtractTriggerCount));
        }
        public bool LoadPlayer(AutomateScriptContext ctx)
        {
            Console.WriteLine("LoadPlayer");
            return true;
        }
        public bool HideNpcHpBar(AutomateScriptContext ctx)
        {
            string str1 = ctx.GetOneMethodParam().toStr;
            int num = ctx.GetOneMethodParam().toInt;
            ctx.PushGlobalExtendParam("a",3.25f);
            Console.WriteLine("HideNpcHpBar");
            return true;
        }
        public bool SetNpcsHpLock(AutomateScriptContext ctx)
        {
            Console.WriteLine("SetNpcsHpLock");
            return ctx.GetGlobalExtendParam("a").toFloat == 3.25f;
        }
        public bool HasVictimByGuid(AutomateScriptContext ctx)
        {
            Console.WriteLine("HasVictimByGuid");
            string str1 = ctx.GetOneMethodParam().toStr;
            if(str1.Equals("30307"))
            return true;
            else return false;
        }
        public bool LoadAnimToObject(AutomateScriptContext ctx)
        {
            Console.WriteLine("LoadAnimToObject");
            return true;
        }
        public bool SetGameObjectAnimatorLayerWeight(AutomateScriptContext ctx)
        {
            Console.WriteLine("SetGameObjectAnimatorLayerWeight");
            return true;
        }
        public bool CheckSceneInteractiveStateWithOpt(AutomateScriptContext ctx)
        {
            Console.WriteLine("CheckSceneInteractiveStateWithOpt");
            return false;
        }
        public bool CheckSceneInteractiveState(AutomateScriptContext ctx)
        {
            Console.WriteLine("CheckSceneInteractiveState");
            string str1 = ctx.GetOneMethodParam().toStr;
            Console.WriteLine(str1);    
            return true;
        }
        public bool ChangeSceneInteractiveState(AutomateScriptContext ctx)
        {
            Console.WriteLine("ChangeSceneInteractiveState");
            return true;
        }
        public bool PlayAnimBySceneInteractiveState(AutomateScriptContext ctx)
        {
            Console.WriteLine(ctx.GetOneMethodParam());
            Console.WriteLine("PlayAnimBySceneInteractiveState");
            return true;
        }
        public bool GetGameObjectCurrentAnimatorStateProgress(AutomateScriptContext ctx)
        {
            Console.WriteLine("GetGameObjectCurrentAnimatorStateProgress");
            ctx.PushResult(1);
            return false;
        }
        public bool PlayMouldAnim(AutomateScriptContext ctx)
        {
            Console.WriteLine("PlayMouldAnim");
            return true;
        }
        public bool SubtractTriggerCount(AutomateScriptContext ctx)
        {
            Console.WriteLine("SubtractTriggerCount");
            return true;
        }
    }
}
