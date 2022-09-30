using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace LoadGraphml
{
    // 自动脚本
    public class AutomateScriptContext
    {
        private AutomateState[] automateStates; // 所有的状态
        private Dictionary<string, AutomateState> dic_AutomateStates = new Dictionary<string, AutomateState>(); // 所有的状态字典
        private AutomateTransation[] automateTransations; // 所有的连线
        private Dictionary<string, List<AutomateTransation>> dic_AutomateTransations = new Dictionary<string, List<AutomateTransation>>(); // 连线字典方便查询

        private AutomateState m_CurrentState; // 当前状态
        private Queue<MethodParam> queue_MethodParams = new Queue<MethodParam>(); // 当前调用指令的参数
        private Dictionary<string, MethodParam> dic_GlobalParams = new Dictionary<string, MethodParam>(); // 当前脚本全局参数
        private MethodParam m_TransationResult; // 指令调用结果
        private bool m_NeedCheckTransations = false; // 是否需要重新判断
        private bool m_CanToNextState = true; // 是否可以到下一状态

        public bool scriptFinished = false;

        public void LoafFromFile(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            MemoryStream memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);
            fileStream.Close();
            memoryStream.Seek(0, SeekOrigin.Begin);
            int stateNum = StreamUtils.ReadInt32(memoryStream);
            automateStates = new AutomateState[stateNum];
            for (int i = 0; i < stateNum; i++)
            {
                AutomateState automateState = new AutomateState();
                automateState.Analysis(memoryStream);
                automateStates[i] = automateState;
                dic_AutomateStates[automateState.id] = automateState;
                // 设置入口
                if (automateState.instructions.Length > 0 && automateState.instructions[0].methodName.Equals("Start"))
                {
                    m_CurrentState = automateState;
                }
            }

            int transationNum = StreamUtils.ReadInt32(memoryStream);
            automateTransations = new AutomateTransation[transationNum];
            for (int i = 0; i < transationNum; i++)
            {
                AutomateTransation automateTransation = new AutomateTransation();
                automateTransation.Analysis(memoryStream);
                automateTransations[i] = automateTransation;
                if (!dic_AutomateTransations.ContainsKey(automateTransation.source)) dic_AutomateTransations[automateTransation.source] = new List<AutomateTransation>();
                dic_AutomateTransations[automateTransation.source].Add(automateTransation);
            }
        }

        public void Update()
        {
            if (this.IsFinalState())
            {
                m_CurrentState = null;
                scriptFinished = true;
            }

            if (m_CurrentState == null || scriptFinished == true) return;
            if (m_NeedCheckTransations)
            {
                // 需要重新检查当前状态到下一状态的条件
                GetNextState();
            }
            else
            {
                // 执行当前状态的指令
                DoCurrentState();
                // 判断 获取下一状态
                if (m_CanToNextState)
                {
                    GetNextState();
                }
            }
        }

        // 执行当前的状态 指令
        public void DoCurrentState()
        {
            if (m_CurrentState == null) return;

            m_CanToNextState = true;

            for (int i = 0; i < m_CurrentState.instructions.Length; i++)
            {
                Instruction instruction = m_CurrentState.instructions[i];
                bool doInstructionResult = DoInstruction(instruction);
                if (doInstructionResult == false) m_CanToNextState = false;
            }
        }

        // 获取下一个状态
        public void GetNextState()
        {
            if (m_CurrentState == null)
                return;
            string sourceId = m_CurrentState.id;
            List<AutomateTransation> targetAutomateTransations = null;
            dic_AutomateTransations.TryGetValue(sourceId, out targetAutomateTransations);
            if (targetAutomateTransations == null) targetAutomateTransations = new List<AutomateTransation>();
            if (targetAutomateTransations.Count >= 2)
                targetAutomateTransations.Sort((v1, v2) => { return v1.priority == v2.priority ? 0 : v1.priority < v2.priority ? 1 : -1; });
            bool toNextState = false;
            AutomateTransation successAutomateTransation = null;
            for (int i = 0; i < targetAutomateTransations.Count; i++)
            {
                // 如果连线有条件要执行条件的指令
                successAutomateTransation = targetAutomateTransations[i];
                if (CheckTransation(targetAutomateTransations[i]))
                {
                    toNextState = true;
                    break;
                }
            }

            if (toNextState)
            {
                m_CurrentState = dic_AutomateStates[successAutomateTransation.target];
                m_NeedCheckTransations = false;
            }
            else
            {
                m_NeedCheckTransations = true;
            }
        }

        // 检查这条连线的指令是否可以通过
        public bool CheckTransation(AutomateTransation automateTransation)
        {
            if (automateTransation.instructions.Length == 0) return true;
            bool checkResult = true;
            for (int i = 0; i < automateTransation.instructions.Length; i++)
            {
                Instruction instruction = automateTransation.instructions[i];
                if (!DoInstruction(instruction, true))
                {
                    checkResult = false;
                    break;
                }
            }

            return checkResult;
        }

        // 执行指令
        public bool DoInstruction(Instruction instruction, bool isTransation = false)
        {
            if (string.IsNullOrEmpty(instruction.methodName) ||
                instruction.methodName.Equals("Start") ||
                instruction.methodName.Equals("Entry") ||
                instruction.methodName.Equals("Exit")) return true;


            AutomateScriptManager.AutomateHandle automateHandle;
            AutomateScriptManager.Instance.AutomateHandles.TryGetValue(instruction.methodName, out automateHandle);
            if (automateHandle == null)
            {
                Console.WriteLine("Error: function name [{0}] not exist!", instruction.methodName);
                return false;
            }

            // 参数变量入队
            this.queue_MethodParams.Clear();
            for (int i = 0; i < instruction.methodParams.Length; i++)
                this.queue_MethodParams.Enqueue(instruction.methodParams[i]);

            bool invokeResult = automateHandle.Invoke(this);

            if (!isTransation) return true; // 如果是状态直接返回true 表示通过
            // 以下都是判断连线条件结果
            if (string.IsNullOrEmpty(instruction.resultCompareValue) && instruction.resultOpt == ResultOpt.None)
                return invokeResult;
            if (this.m_TransationResult == null || string.IsNullOrEmpty(this.m_TransationResult.value))
            {
                Console.WriteLine("Error: function name [{0}] need set a result!", instruction.methodName);
                return false;
            }

            if (instruction.resultCompareValueType != this.m_TransationResult.paramType)
            {
                Console.WriteLine("Error: function name [{0}] result type error!", instruction.methodName);
                return false;
            }

            if (instruction.resultCompareValueType == ParamType.Bool)
                return this.Compare(this.m_TransationResult.toBool, bool.Parse(instruction.resultCompareValue), instruction.resultOpt);
            else if (instruction.resultCompareValueType == ParamType.Number)
            {
                if (instruction.resultCompareValue.Contains("."))
                {
                    return this.Compare(this.m_TransationResult.toFloat, float.Parse(instruction.resultCompareValue), instruction.resultOpt);
                }
                else
                {
                    return this.Compare(this.m_TransationResult.toInt, int.Parse(instruction.resultCompareValue), instruction.resultOpt);
                }
            }
            else if (instruction.resultCompareValueType == ParamType.String)
            {
                return this.Compare(this.m_TransationResult.toStr, instruction.resultCompareValue, instruction.resultOpt);
            }

            return true;
        }

        private bool Compare<T>(T v1, T v2, ResultOpt compareOpt)
        {
            if (typeof(T) == typeof(string))
            {
                string tmpV1 = (string)(object)v1;
                string tmpV2 = (string)(object)v2;
                if (compareOpt == ResultOpt.Eq) return tmpV1.Equals(tmpV2);
                if (compareOpt == ResultOpt.Le) return tmpV1.Equals(tmpV2) && tmpV1.CompareTo(tmpV2) == 0;
                if (compareOpt == ResultOpt.Ge) return tmpV1.Equals(tmpV2) && tmpV1.CompareTo(tmpV2) == 1;
                if (compareOpt == ResultOpt.Lt) return tmpV1.CompareTo(tmpV2) == 0;
                if (compareOpt == ResultOpt.Gt) return tmpV1.CompareTo(tmpV2) == 1;
            }
            else if (typeof(T) == typeof(bool))
            {
                bool tmpV1 = (bool)(object)v1;
                bool tmpV2 = (bool)(object)v2;
                if (compareOpt == ResultOpt.Eq) return tmpV1 == tmpV2;
            }
            else if (typeof(T) == typeof(int))
            {
                int tmpV1 = (int)(object)v1;
                int tmpV2 = (int)(object)v2;
                if (compareOpt == ResultOpt.Eq) return tmpV1 == tmpV2;
                if (compareOpt == ResultOpt.Le) return tmpV1 <= tmpV2;
                if (compareOpt == ResultOpt.Ge) return tmpV1 >= tmpV2;
                if (compareOpt == ResultOpt.Lt) return tmpV1 < tmpV2;
                if (compareOpt == ResultOpt.Gt) return tmpV1 > tmpV2;
            }
            else if (typeof(T) == typeof(float))
            {
                float tmpV1 = (float)(object)v1;
                float tmpV2 = (float)(object)v2;
                if (compareOpt == ResultOpt.Eq) return tmpV1 == tmpV2;
                if (compareOpt == ResultOpt.Le) return tmpV1 <= tmpV2;
                if (compareOpt == ResultOpt.Ge) return tmpV1 >= tmpV2;
                if (compareOpt == ResultOpt.Lt) return tmpV1 < tmpV2;
                if (compareOpt == ResultOpt.Gt) return tmpV1 > tmpV2;
            }

            return false;
        }

        // 获取当前指令的一个变量
        public MethodParam GetOneMethodParam()
        {
            return this.queue_MethodParams.Dequeue();
        }

        // 放一个当前脚本的全局变量
        public void PushGlobalExtendParam<T>(string key, T value)
        {
            MethodParam methodParam = new MethodParam();
            methodParam.index = this.dic_GlobalParams.Count;
            methodParam.value = string.Format("{0}", value);
            methodParam.paramType = this.GetParamType(value);
            this.dic_GlobalParams[key] = methodParam;
        }

        // 获得一个当前脚本的全局变量
        public MethodParam GetGlobalExtendParam(string key)
        {
            MethodParam methodParam = null;
            this.dic_GlobalParams.TryGetValue(key, out methodParam);
            return methodParam;
        }

        // 放指令执行结果
        public void PushResult<T>(T value)
        {
            MethodParam methodParam = new MethodParam();
            methodParam.index = this.dic_GlobalParams.Count;
            methodParam.value = string.Format("{0}", value);
            methodParam.paramType = this.GetParamType(value);
            this.m_TransationResult = methodParam;
        }

        private ParamType GetParamType<T>(T value)
        {
            if (typeof(T) == typeof(int) || typeof(T) == typeof(float) || typeof(T) == typeof(Int32) || typeof(T) == typeof(double) || typeof(T) == typeof(Double))
                return ParamType.Number;
            else if (typeof(T) == typeof(bool) || typeof(T) == typeof(Boolean))
                return ParamType.Bool;
            else return ParamType.String;
        }

        // 判断是否是最后一个状态
        private bool IsFinalState()
        {
            if (m_CurrentState == null) return true;

            string sourceId = m_CurrentState.id;
            List<AutomateTransation> targetAutomateTransations = null;
            dic_AutomateTransations.TryGetValue(sourceId, out targetAutomateTransations);
            // 没有连线就是最后一个了，指令一般都是 Exit，这里就不用判断了
            if (targetAutomateTransations != null)
            {
                return false;
            }

            return true;
        }

        public class AutomateState
        {
            public string id;
            public Instruction[] instructions;

            public void Analysis(MemoryStream memoryStream)
            {
                this.id = StreamUtils.ReadString(memoryStream);
                int instructionNum = StreamUtils.ReadInt32(memoryStream);
                if (instructionNum > 0) this.instructions = new Instruction[instructionNum];
                for (int i = 0; i < instructionNum; i++)
                {
                    this.instructions[i] = new Instruction();
                    this.instructions[i].Analysis(memoryStream, MethodType.Normal);
                }
            }
        }

        public class AutomateTransation
        {
            public string id;
            public string source; // 源
            public string target; // 目标
            public int priority; // 优先级
            public Instruction[] instructions; // 条件指令集

            public void Analysis(MemoryStream memoryStream)
            {
                this.id = StreamUtils.ReadString(memoryStream);
                this.source = StreamUtils.ReadString(memoryStream);
                this.target = StreamUtils.ReadString(memoryStream);
                this.priority = StreamUtils.ReadInt32(memoryStream);
                int instructionNum = StreamUtils.ReadInt32(memoryStream);
                if (instructionNum > 0) this.instructions = new Instruction[instructionNum];
                for (int i = 0; i < instructionNum; i++)
                {
                    this.instructions[i] = new Instruction();
                    this.instructions[i].Analysis(memoryStream, MethodType.Condition);
                }
            }
        }

        public class Instruction
        {
            public string methodName;
            public MethodParam[] methodParams;
            public ResultOpt resultOpt;
            public ParamType resultCompareValueType;
            public string resultCompareValue;

            public void Analysis(MemoryStream memoryStream, MethodType methodType)
            {
                this.methodName = StreamUtils.ReadString(memoryStream);
                int methodParamNum = StreamUtils.ReadInt32(memoryStream);
                this.methodParams = new MethodParam[methodParamNum];
                for (int i = 0; i < methodParamNum; i++)
                {
                    this.methodParams[i] = new MethodParam();
                    this.methodParams[i].index = StreamUtils.ReadInt32(memoryStream);
                    this.methodParams[i].paramType = (ParamType)StreamUtils.ReadInt32(memoryStream);
                    this.methodParams[i].value = StreamUtils.ReadString(memoryStream);
                }

                if (methodType == MethodType.Condition)
                {
                    this.resultCompareValueType = (ParamType)StreamUtils.ReadInt32(memoryStream);
                    this.resultCompareValue = StreamUtils.ReadString(memoryStream);
                    this.resultOpt = (ResultOpt)StreamUtils.ReadInt32(memoryStream);
                }
            }
        }

        public class MethodParam
        {
            public int index;
            public ParamType paramType;
            public string value;
            public string toStr => value;
            public int toInt => int.Parse(value);
            public bool toBool => bool.Parse(value);
            public float toFloat => float.Parse(value);
        }

        public enum MethodType
        {
            Normal = 0, // 正常方法
            Condition = 1, // 判断条件
        }

        public enum ParamType
        {
            Number = 0,
            String = 1,
            Bool = 2,
        }

        public enum ResultOpt
        {
            None = 0, // 对结果没有判断
            Eq = 1, // 结果与目标值比较为 相等
            Lt = 2, // 结果与目标值比较为 小于
            Gt = 3, // 结果与目标值比较为 大于
            Le = 4, // 结果与目标值比较为 小于等于
            Ge = 5, // 结果与目标值比较为 大于等于
        }
    }
}