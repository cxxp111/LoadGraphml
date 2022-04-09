using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace LoadGraphml
{

    public class Program
    {
        public static void Main(string[] args)
        {
            XmlDocument xml = new XmlDocument();
            //xml.Load("D:\\UnityProject\\Client\\Wuxia6\\Assets\\Resources\\Graphml\\Source\\2072_jiguan_rigui.graphml");
            xml.Load("C:\\Users\\zfj\\Desktop\\test.graphml");
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

                }

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
                string edgeLabellText = edgeLabel.InnerText;
                List<string> conditions = new List<string>();
                if (edgeLabellText.Contains("\r\n"))
                    conditions.AddRange(edgeLabellText.Split("\r\n"));
                else if (edgeLabellText.Contains("\n"))
                    conditions.AddRange(edgeLabellText.Split("\n"));
                else
                    conditions.Add(edgeLabellText);

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

            public void ParseInstruction(string instruction, MethodType methodType)
            {
                this.instruction = instruction;
                this.methodType = methodType;
            }
            public void ExportByte(MemoryStream memoryStream) { 
            
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
            Int32 = 0,
            String = 1
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
