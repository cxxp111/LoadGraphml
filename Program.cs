using System;
using System.Collections.Generic;
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
                if (nodeLabelText.Contains("\r\n"))
                    automateState.instructions.AddRange(nodeLabelText.Split("\r\n"));
                else if (nodeLabelText.Contains("\n"))
                    automateState.instructions.AddRange(nodeLabelText.Split("\n"));
                else
                    automateState.instructions.Add(nodeLabelText);

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
                if (edgeLabellText.Contains("\r\n"))
                    automateTransition.conditions.AddRange(edgeLabellText.Split("\r\n"));
                else if (edgeLabellText.Contains("\n"))
                    automateTransition.conditions.AddRange(edgeLabellText.Split("\n"));
                else
                    automateTransition.conditions.Add(edgeLabellText);

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

        
    }

    
    public class AutomateState
    {
        public string id;
        public List<string> instructions = new List<string>();// 指令集
    }
    public class AutomateTransition
    {
        public string id;
        public string source;// 源
        public string target;// 目标
        public string priority;// 优先级
        public List<string> conditions = new List<string>();// 条件指令集
    }
}
