using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Diagnostics.CodeAnalysis;
using IdsLib.Messages;
using System.Text;
using IdsLib.IdsSchema.IdsNodes;

namespace IdsLib.IdsSchema;

[DebuggerDisplay("{type} (Line {StartLineNumber}, Pos {StartLinePosition})")]
internal class IdsXmlNode
{
    protected internal readonly string type;

    private IdsXmlNode? parent;
    protected virtual internal IdsXmlNode? Parent
    {
        get => parent;
        private set
        {
            parent = value;
            parent?.AddChild(this);
        }
    }

    protected internal readonly List<IdsXmlNode> Children = new();

    private void AddChild(IdsXmlNode child)
    {
        Children.Add(child);
    }

    internal int StartLineNumber { get; set; } = 0;
    internal int StartLinePosition { get; set; } = 0;
    internal int PositionalIndex { get; set; } = 1; // 1 based index

    internal string GetPositionalIdentifier()
    {
        StringBuilder sb = new StringBuilder();
        GetPositionalIdentifier(sb);
        return sb.ToString();
    }

	internal void GetPositionalIdentifier(StringBuilder sb)
	{
		if (Parent != null)
		{
			Parent.GetPositionalIdentifier(sb);
		}
		sb.Append($"/{type}{PositionalIndex}");
	}

    internal NodeIdentification GetNodeIdentification()
    {
        return new NodeIdentification()
        {
            PositionalIdentifier = GetPositionalIdentifier(),
            StartLineNumber = StartLineNumber,
            StartLinePosition = StartLinePosition,
            NodeType = type,
        };
	}

	public IdsXmlNode(XmlReader reader, IdsXmlNode? parent)
    {
        Parent = parent;
        type = reader.LocalName;
        if (reader is IXmlLineInfo li)
        {
            StartLineNumber = li.LineNumber;
            StartLinePosition = li.LinePosition;
        }
        if (Parent != null)
            PositionalIndex = Parent.Children.Where(x=>x.type == type).Count(); // this should reflect the number of children already encountered, which corresponds to this item's progressive number (1-based).
    }

    /// <summary>
    /// The Audit method of the base context always succeeds
    /// </summary>
    /// <param name="logger">unused in the base class</param>
    /// <returns><see cref="Audit.Status.Ok"/> in all circumstances, only overridden implementation determine failure behaviours</returns>
    internal protected virtual Audit.Status PerformAudit(ILogger? logger)
    {
        return Audit.Status.Ok;
    }

    internal protected virtual void SetContent(string contentString)
    {
        // nothing to do for the base entity
    }

    protected static bool TryGetUpperNodes(IdsXmlNode start, ref List<IdsXmlNode> nodes, params string[] typeNames)
    {
        if (start.Parent is null)
            return false;
        if (start.Parent.type == typeNames[0])
        {
            // found
            nodes.Add(start.Parent);
            if (typeNames.Length > 1) // more to find
                return TryGetUpperNodes(start.Parent, ref nodes, typeNames.Skip(1).ToArray());
            return true; // all found
        }
        // not found, search on the parent, instead
        return TryGetUpperNodes(start.Parent, ref nodes, typeNames);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="logger"></param>
    /// <param name="startingNode"></param>
    /// <param name="typeNames">array o</param>
    /// <param name="node"></param>
    /// <param name="status">provide status error if return is unsecceessful</param>
    protected static bool TryGetUpperNode<T>(ILogger? logger, IdsXmlNode startingNode, string[] typeNames, [NotNullWhen(true)] out T? node, out Audit.Status status)
    {
        if (!TryGetUpperNodes(startingNode, typeNames, out var nodes))
        {
			IdsErrorMessages.Report501UnexpectedScenario(logger, $"Missing {typeof(T).Name} ", startingNode);
            node = default;
            status = Audit.Status.IdsStructureError;
            return false;
        }
        if (nodes[0] is not T spec)
        {
            node = default;
			IdsErrorMessages.Report501UnexpectedScenario(logger, $"Invalid {typeof(T).Name} ", startingNode);
            status = Audit.Status.IdsStructureError;
            return false;
        }
        node = spec;
        status = Audit.Status.Ok;
        return true;
    }

    protected static bool TryGetUpperNodes(IdsXmlNode startingNode, string[] typeNames, out List<IdsXmlNode> nodes)
    {
        var span = new ReadOnlySpan<string>(typeNames);
        nodes = new List<IdsXmlNode>();
        return TryGetUpperNodes(startingNode, ref nodes, span);
    }

    protected static bool TryGetUpperNodes(IdsXmlNode startingNode, ref List<IdsXmlNode> nodes, ReadOnlySpan<string> typeNames)
    {
        if (startingNode.Parent is null)
            return false;
        if (startingNode.Parent.type == typeNames[0])
        {
            // found
            nodes.Add(startingNode.Parent);
            if (typeNames.Length > 1) // more to search
#if NETSTANDARD2_0
                return TryGetUpperNodes(startingNode.Parent, ref nodes, typeNames.Slice(1));
#else
                return TryGetUpperNodes(startingNode.Parent, ref nodes, typeNames[1..]);
#endif
            return true; // all found
        }
        // not found, search on the parent, instead
        return TryGetUpperNodes(startingNode.Parent, ref nodes, typeNames);
    }

    protected IEnumerable<IdsXmlNode> GetChildNodes(string name)
    {
        return Children.Where(x => x.type == name);
    }

    protected T? GetChildNode<T>(string name)
    {
        return Children.Where(x => x.type == name).OfType<T>().FirstOrDefault();
    }

    internal IStringListMatcher? GetListMatcher()
    {
        return Children.OfType<IStringListMatcher>().FirstOrDefault();
    }
}
