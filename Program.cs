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
            xml.Load("D:\\UnityProject\\Client\\Wuxia6\\Assets\\Resources\\Graphml\\Source\\2072_jiguan_rigui.graphml");
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
            XmlNodeList xmlNodeList = xml.GetElementsByTagName("node");
            for (int i =0;i< xmlNodeList.Count;i++) {
               XmlNode xmlNode = xmlNodeList.Item(i);
                string id = xmlNode.Attributes["id"].Value;
                XmlNode nodeLabel = GetNode(xmlNode, "y:NodeLabel");
                if (nodeLabel!=null) {
                    System.Console.WriteLine(nodeLabel.InnerText);
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
                if (edgeLabel != null)
                {
                    System.Console.WriteLine(edgeLabel.InnerText);
                }
            }
        }

        public static XmlNode GetNode(XmlNode xmlNode,string nodeName)
        {
            if (xmlNode.Name.Equals(nodeName)) {
                return xmlNode;
            }
            for (int i =0;i < xmlNode.ChildNodes.Count;i++) {
                XmlNode tmpNode = GetNode(xmlNode.ChildNodes.Item(i), nodeName);
                if (tmpNode!=null) {
                    return tmpNode;
                }
            }
            return null;
        }
    }
    
    public class AutomateState {
        public string id;
        public List<string> instructions;// 指令集
        public List<AutomateState> parentStates;// 父节点
        public List<AutomateState> childStates;// 子节点
        public List<AutomateTransition> automateTransitions;//连线信息
    }
    public class AutomateTransition {
        public string id;
        public string source;// 源
        public string target;// 目标
        public string priority;// 优先级
    }
}
