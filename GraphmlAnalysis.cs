using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LoadGraphml
{
    public class GraphmlAnalysis
    {
        static AutomateState[] automateStates;
        static AutomateTransation[] automateTransations;
        public static void Analysis(string filePath)
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
            }
            int transationNum = StreamUtils.ReadInt32(memoryStream);
            automateTransations = new AutomateTransation[transationNum];
            for (int i = 0; i < transationNum; i++)
            {
                AutomateTransation automateTransation = new AutomateTransation();
                automateTransation.Analysis(memoryStream);
                automateTransations[i] = automateTransation;
            }

        }

        class AutomateState
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
                    this.instructions[i].Analysis(memoryStream,MethodType.Normal);
                }
            }
        }
        class AutomateTransation {
            public string id;
            public string source;// 源
            public string target;// 目标
            public int priority;// 优先级
            public Instruction[] instructions;// 条件指令集
            public void Analysis(MemoryStream memoryStream) {
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
        struct Instruction
        {
            public string methodName;
            public MethodParam [] methodParams;
            public ResultOpt resultOpt;
            public ParamType ResultCompareValueType;
            public string ResultCompareValue;
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
                    this.ResultCompareValueType = (ParamType)StreamUtils.ReadInt32(memoryStream);
                    this.ResultCompareValue = StreamUtils.ReadString(memoryStream);
                    this.resultOpt = (ResultOpt)StreamUtils.ReadInt32(memoryStream);
                }
            }
        }
        struct MethodParam
        {
            public int index;
            public ParamType paramType;
            public String value;
        }
        enum MethodType
        {
            Normal = 0,// 正常方法
            Condition = 1,// 判断条件
        }
        enum ParamType
        {
            Number = 0,
            String = 1,
            Bool = 2,
        }
        enum ResultOpt
        {
            None = 0,// 对结果没有判断
            Eq = 1,// 结果与目标值比较为 相等
            Lt = 2,// 结果与目标值比较为 小于
            Gt = 3,// 结果与目标值比较为 大于
            Le = 4,// 结果与目标值比较为 小于等于
            Ge = 5,// 结果与目标值比较为 大于等于
        }
    }
}
