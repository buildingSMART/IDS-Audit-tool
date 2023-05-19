using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Diagnostics.CodeAnalysis;
using IdsLib.Messages;

namespace IdsLib.IdsSchema;

[DebuggerDisplay("{type} (Line {StartLineNumber}, Pos {StartLinePosition})")]
internal class BaseContext
{
    protected internal readonly string type;

    private BaseContext? parent;
    protected virtual internal BaseContext? Parent
    {
        get => parent;
        private set
        {
            parent = value;
            parent?.AddChild(this);
        }
    }

    protected internal readonly List<BaseContext> Children = new();

    private void AddChild(BaseContext child)
    {
        Children.Add(child);
    }

    internal int StartLineNumber { get; set; } = 0;
    internal int StartLinePosition { get; set; } = 0;

    public BaseContext(XmlReader reader, BaseContext? parent)
    {
        Parent = parent;
        type = reader.LocalName;
        if (reader is IXmlLineInfo li)
        {
            StartLineNumber = li.LineNumber;
            StartLinePosition = li.LinePosition;
        }
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

    protected static bool TryGetUpperNodes(BaseContext start, ref List<BaseContext> nodes, params string[] typeNames)
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

    protected static bool TryGetUpperNode<T>(ILogger? logger, BaseContext context, string[] typeNames, [NotNullWhen(true)] out T? node, out Audit.Status status)
    {
        if (!TryGetUpperNodes(context, typeNames, out var nodes))
        {
			IdsToolMessages.ReportLocatedUnexpectedScenario(logger, $"Missing {typeof(T).Name} ", context);
            node = default;
            status = Audit.Status.IdsStructureError;
            return false;
        }
        if (nodes[0] is not T spec)
        {
            node = default;
			IdsToolMessages.ReportLocatedUnexpectedScenario(logger, $"Invalid {typeof(T).Name} ", context);
            status = Audit.Status.IdsStructureError;
            return false;
        }
        node = spec;
        status = Audit.Status.Ok;
        return true;
    }


    protected static bool TryGetUpperNodes(BaseContext start, string[] typeNames, out List<BaseContext> nodes)
    {
        var span = new ReadOnlySpan<string>(typeNames);
        nodes = new List<BaseContext>();
        return TryGetUpperNodes(start, ref nodes, span);
    }

    protected static bool TryGetUpperNodes(BaseContext start, ref List<BaseContext> nodes, ReadOnlySpan<string> typeNames)
    {
        if (start.Parent is null)
            return false;
        if (start.Parent.type == typeNames[0])
        {
            // found
            nodes.Add(start.Parent);
            if (typeNames.Length > 1) // more to search
#if NETSTANDARD2_0
                return TryGetUpperNodes(start.Parent, ref nodes, typeNames.Slice(1));
#else
                return TryGetUpperNodes(start.Parent, ref nodes, typeNames[1..]);
#endif
            return true; // all found
        }
        // not found, search on the parent, instead
        return TryGetUpperNodes(start.Parent, ref nodes, typeNames);
    }

    protected IEnumerable<BaseContext> GetChildNodes(string name)
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
