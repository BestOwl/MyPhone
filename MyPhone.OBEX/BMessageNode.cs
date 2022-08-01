using System;
using System.Collections.Generic;
using System.IO;

namespace GoodTimeStudio.MyPhone.OBEX
{
    public class BMessageNode
    {
        public string NodeName { get; set; }

        public Dictionary<string, BMessageNode> ChildrenNode { get; set; }

        public Dictionary<string, string> Attributes { get; set; }

        public string? Value { get; set; }

        public BMessageNode(string nodeName)
        {
            NodeName = nodeName;
            ChildrenNode = new Dictionary<string, BMessageNode>();
            Attributes = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            return "BEGIN:" + NodeName + Environment.NewLine + Value + Environment.NewLine + "END:" + NodeName;
        }

        public static BMessageNode Parse(string bMessageString)
        {
            StringReader reader = new StringReader(bMessageString);
            string line = reader.ReadLine().Trim();
            if (!line.StartsWith("BEGIN:"))
            {
                throw new BMessageException(1, "bMessage string should starts with BEGIN");
            }

            BMessageNode root = new BMessageNode(line.Substring(6));
            return _parseRecursive(ref reader, root, 1);
        }

        private static BMessageNode _parseRecursive(ref StringReader reader, BMessageNode root, int lineNum, bool ignoreAttr = false)
        {
            string line = reader.ReadLine().Trim();

            while (true)
            {
                if (line.StartsWith("BEGIN:"))
                {
                    switch (line)
                    {
                        case "BEGIN:MSG":
                        case "BEGIN:VCARD":
                            root.ChildrenNode.Add(line.Substring(6),
                            _parseRecursive(ref reader, new BMessageNode(line.Substring(6)),
                            lineNum, true));
                            break;
                        default:
                            root.ChildrenNode.Add(line.Substring(6),
                            _parseRecursive(ref reader, new BMessageNode(line.Substring(6)),
                            lineNum));
                            break;
                    }
                }
                else if (line.StartsWith("END:"))
                {
                    if (line.Substring(4) != root.NodeName)
                    {
                        throw new BMessageException(lineNum, "Enclosing node name does not equal to opening node name.");
                    }
                    return root;
                }
                else
                {
                    if (line.Contains(":") && !ignoreAttr)
                    {
                        string[] kv = line.Split(':');
                        root.Attributes[kv[0]] = kv[1];
                    }
                    else
                    {
                        root.Value += line;
                        root.Value += Environment.NewLine;
                    }
                }

                line = reader.ReadLine().Trim();
                lineNum++;
                if (line == null)
                {
                    throw new BMessageException(lineNum, "Reach end of file.");
                }
            }
        }
    }

    public class BMessageException : Exception
    {
        public BMessageException(int lineNumber) : base($"Error at line {lineNumber}. The provided string is not a valid bMessage.")
        {
        }

        public BMessageException(int lineNumber, string message)
            : base($"Error at line {lineNumber}: {message}. The provided string is not a valid bMessage.") { }

        public BMessageException(int lineNumber, string message, Exception inner)
            : base($"Error at line {lineNumber}: {message}. The provided string is not a valid bMessage.", inner)
        {
        }
    }
}
