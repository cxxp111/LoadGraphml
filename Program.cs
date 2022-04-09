using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace LoadGraphml
{

    public class Program
    {

        public static void Main(string[] args)
        {
            string pattern = @"(\""[^\""]*\"")";
            string input = @"LoadPlayerfalse652(123.456,false,1,""Player false"",""10.20,1.0,10"",""哈哈哈"",20,false,"""",true,"""",321.32)";
            RegexOptions options = RegexOptions.Multiline;

            foreach (Match m in Regex.Matches(input, pattern, options))
            {
                Console.WriteLine("'{0}' found at index {1}.", m.Value, m.Index);
            }

            List<AutomateState> automateStates = new List<AutomateState>();
            List<AutomateTransition> automateTransitions = new List<AutomateTransition>();

            XmlDocument xml = new XmlDocument();
            //xml.Load("D:\\UnityProject\\Client\\Wuxia6\\Assets\\Resources\\Graphml\\Source\\2072_jiguan_rigui.graphml");
            xml.Load(@"C:\Users\77547\Desktop\test.graphml");
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
            XmlNodeList xmlNodeList = xml.GetElementsByTagName("node");
            for (int i = 0; i < xmlNodeList.Count; i++)
            {
                XmlNode xmlNode = xmlNodeList.Item(i);
                string id = xmlNode.Attributes["id"].Value;
                XmlNode nodeLabel = GetNode(xmlNode, "y:NodeLabel");
                if (nodeLabel == null) continue;
                System.Console.WriteLine(nodeLabel.InnerText);

                AutomateState automateState = new AutomateState();
                automateState.id = id;
                string nodeLabelText = nodeLabel.InnerText;
                List<string> instructionStrs = new List<string>();
                if (nodeLabelText.Contains("\r\n"))
                    instructionStrs.AddRange(nodeLabelText.Split("\r\n"));
                else if (nodeLabelText.Contains("\n"))
                    instructionStrs.AddRange(nodeLabelText.Split("\n"));
                else instructionStrs.Add(nodeLabelText);

                foreach (string instructionStr in instructionStrs)
                {
                    Instruction instruction = new Instruction();
                    instruction.ParseInstruction(instructionStr, MethodType.Normal);
                    automateState.instructions.Add(instruction);
                }
                automateStates.Add(automateState);

            }

            XmlNodeList xmlEdgeList = xml.GetElementsByTagName("edge");
            for (int i = 0; i < xmlEdgeList.Count; i++)
            {
                XmlNode xmlNode = xmlEdgeList.Item(i);
                string id = xmlNode.Attributes["id"].Value;
                string source = xmlNode.Attributes["source"].Value;
                string target = xmlNode.Attributes["target"].Value;
                XmlNode edgeLabel = GetNode(xmlNode, "y:EdgeLabel");

                if (edgeLabel == null) continue;
                System.Console.WriteLine(edgeLabel.InnerText);

                AutomateTransition automateTransition = new AutomateTransition();
                automateTransition.id = id;
                automateTransition.source = source;
                automateTransition.target = target;
                string edgeLabellText = edgeLabel.InnerText;
                List<string> conditions = new List<string>();
                if (edgeLabellText.Contains("\r\n"))
                    conditions.AddRange(edgeLabellText.Split("\r\n"));
                else if (edgeLabellText.Contains("\n"))
                    conditions.AddRange(edgeLabellText.Split("\n"));
                else
                    conditions.Add(edgeLabellText);

                foreach (string conditionStr in conditions)
                {
                    Instruction instruction = new Instruction();
                    if (conditions.IndexOf(conditionStr) == 0)
                    {
                        instruction.ParsePriority(conditionStr, automateTransition.priority);
                    }
                    instruction.ParseInstruction(conditionStr, MethodType.Condition);
                    automateTransition.conditions.Add(instruction);
                }
                automateTransitions.Add(automateTransition);
            }
        }

        public static XmlNode GetNode(XmlNode xmlNode, string nodeName)
        {
            if (xmlNode.Name.Equals(nodeName))
            {
                return xmlNode;
            }
            for (int i = 0; i < xmlNode.ChildNodes.Count; i++)
            {
                XmlNode tmpNode = GetNode(xmlNode.ChildNodes.Item(i), nodeName);
                if (tmpNode != null)
                {

                    return tmpNode;
                }
            }
            return null;
        }

        public class AutomateState
        {
            public string id;
            public List<Instruction> instructions = new List<Instruction>();// 指令集

        }
        public class AutomateTransition
        {
            public string id;
            public string source;// 源
            public string target;// 目标
            public string priority;// 优先级
            public List<Instruction> conditions = new List<Instruction>();// 条件指令集
        }
        public struct Instruction
        {
            public string instruction;// 指令
            public string methodName;
            public MethodType methodType;
            public List<MethodParam> methodParams;
            public ResultOpt resultOpt;

            private List<string> strParams;
            private List<float> numberParams;
            private List<bool> boolParams;
            public void ParseInstruction(string instruction, MethodType methodType)
            {
                this.instruction = instruction.Trim();
                this.methodType = methodType;
                this.methodParams = new List<MethodParam>();

                this.strParams = new List<string>();
                this.numberParams = new List<float>();
                this.boolParams = new List<bool>();

                if (this.methodType == MethodType.Normal)
                {
                    if (string.IsNullOrEmpty(this.instruction)) return;
                    this.ParseMethodAndParams(this.instruction);

                }
                else if (this.methodType == MethodType.Condition)
                {
                    if (string.IsNullOrEmpty(this.instruction)) return;
                    string tempInstruction = this.instruction;

                    if (string.IsNullOrEmpty(tempInstruction)) return;
                    this.ParseMethodAndParams(tempInstruction);
                }
            }
            // 解析条件顺序
            public void ParsePriority(string instruction, out int priority)
            {
                priority = 0;
                Match priorityMatch = Regex.Match(instruction, "^\\[\\d\\]");
                if (priorityMatch.Success)
                {
                    priority = int.Parse(priorityMatch.Value.Substring(1, priorityMatch.Length - 2));
                    //outInstruction = instruction.Substring(priorityMatch.Index + priorityMatch.Length);
                }
            }
            public void ParseMethodAndParams(string instruction)
            {
                string tempInstruction = instruction;
                Match m = Regex.Match(tempInstruction, @"^(?=\d*)(.+)(?=\()|Start|Exit");
                if (!m.Success)
                    throw new Exception(tempInstruction + " 匹配方法名失败");
                this.methodName = m.Value;
                tempInstruction = tempInstruction.Remove(m.Index, m.Value.Length);
                Console.WriteLine("'{0}' found at index {1}.", m.Value, m.Index);

                // 字符串 (\"[^\"]*\")
                // bool ((?<=[(,])(true|false))
                // 浮点数 (?<=[,(])\d+\.*\d*(?<=[,)]*)
                // 先替换字符串

                int i = 0;
                string pattern = @"(\""[^\""]*\"")";
                m = Regex.Match(tempInstruction, pattern);
                while (m.Success)
                {
                    Console.WriteLine("'{0}' found at index {1}.", m.Value, m.Index);
                    tempInstruction = tempInstruction.Remove(m.Index, m.Value.Length);
                    tempInstruction = tempInstruction.Insert(m.Index, "replaceString-" + i++);
                    strParams.Add(m.Value.Trim().Substring(1, m.Value.Trim().Length - 2));
                    m = Regex.Match(tempInstruction, pattern);
                }

                i = 0;
                pattern = @"((?<=[(,])(true|false))";
                m = Regex.Match(tempInstruction, pattern);
                while (m.Success)
                {
                    Console.WriteLine("'{0}' found at index {1}.", m.Value, m.Index);
                    tempInstruction = tempInstruction.Remove(m.Index, m.Value.Length);
                    tempInstruction = tempInstruction.Insert(m.Index, "replaceBool-" + i++);
                    boolParams.Add(m.Value.Trim().Equals("true"));
                    m = Regex.Match(tempInstruction, pattern);
                }

                i = 0;
                pattern = @"(?<=[,(])\d+\.*\d*(?<=[,)]*)";
                m = Regex.Match(tempInstruction, pattern);
                while (m.Success)
                {
                    Console.WriteLine("'{0}' found at index {1}.", m.Value, m.Index);
                    tempInstruction = tempInstruction.Remove(m.Index, m.Value.Length);
                    tempInstruction = tempInstruction.Insert(m.Index, "replaceNumber-" + i++);
                    numberParams.Add(float.Parse(m.Value.Trim()));
                    m = Regex.Match(tempInstruction, pattern);
                }

                pattern = @"(?<=\().*(?=\))";
                m = Regex.Match(tempInstruction, pattern);
                if (m.Success && !string.IsNullOrEmpty(m.Value))
                {
                    tempInstruction = tempInstruction.Remove(m.Index, m.Value.Length);
                    string[] replaceParamStrs = m.Value.Split(",");
                    for (i = 0; i < replaceParamStrs.Length; i++)
                    {
                        string replaceParamStr = replaceParamStrs[i];
                        string[] replaceParamSplit = replaceParamStr.Split("-");
                        string replaceParamType = replaceParamSplit[0];
                        string indexStr = replaceParamSplit[1];
                        int index = int.Parse(indexStr);

                        MethodParam methodParam = new MethodParam();
                        methodParam.index = i;
                        if (replaceParamType.Equals("replaceString"))
                        {
                            methodParam.value = strParams[index];
                            methodParam.paramType = ParamType.String;
                        }
                        else if (replaceParamType.Equals("replaceBool"))
                        {
                            methodParam.value = boolParams[index] ? "true" : "false";
                            methodParam.paramType = ParamType.Bool;
                        }
                        if (replaceParamType.Equals("replaceNumber"))
                        {
                            methodParam.value = numberParams[index].ToString();
                            methodParam.paramType = ParamType.Number;
                        }

                        this.methodParams.Add(methodParam);

                    }
                }

                pattern = @"[<>=]=*";
                m = Regex.Match(tempInstruction, pattern);
                if (m.Success)
                {
                    if (m.Value.Equals("=="))
                    {
                        this.resultOpt = ResultOpt.Eq;
                    }
                    else if (m.Value.Equals("<="))
                    {
                        this.resultOpt = ResultOpt.Le;
                    }
                    else if (m.Value.Equals(">="))
                    {
                        this.resultOpt = ResultOpt.Ge;
                    }
                    else if (m.Value.Equals("<"))
                    {
                        this.resultOpt = ResultOpt.Lt;
                    }
                    else if (m.Value.Equals(">"))
                    {
                        this.resultOpt = ResultOpt.Gt;
                    }
                }


            }
            public void ExportByte(MemoryStream memoryStream)
            {

            }
        }

        public struct MethodParam
        {
            public int index;
            public ParamType paramType;
            public String value;
        }
        public enum MethodType
        {
            Normal = 0,// 正常方法
            Condition = 1,// 判断条件
        }
        public enum ParamType
        {
            Number = 0,
            String = 1,
            Bool = 2,
        }
        public enum ResultOpt
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
