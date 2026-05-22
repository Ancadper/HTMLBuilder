using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HTMLBuilder.Ui;

namespace HTMLBuilder.Models;

public sealed class HtmlDocument
{
    private const string DefaultLanguage = "en";
    private static readonly Regex TokenRegex = new("<!--.*?-->|<!DOCTYPE.*?>|</?[^>]+>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex AttributeRegex = new("([a-zA-Z_:][a-zA-Z0-9_:\\-.]*)\\s*(?:=\\s*(?:\"([^\"]*)\"|'([^']*)'|([^\\s\"'=<`>]+)))?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new("\\s+", RegexOptions.Compiled);

    public HtmlDocument()
    {
        Root = new("html");
        Root.Attributes["lang"] = Localizer.CurrentLanguage;

        Head = new("head");
        Head.Children.Add(new ElementNode("meta") { Attributes = { ["charset"] = "utf-8" } });
        Head.Children.Add(new ElementNode("meta") { Attributes = { ["name"] = "viewport", ["content"] = "width=device-width, initial-scale=1" } });
        Head.Children.Add(new ElementNode("title", DefaultTitle()));

        Body = new("body");

        Root.Children.Add(Head);
        Root.Children.Add(Body);
    }

    public ElementNode Root { get; }
    public ElementNode Head { get; }
    public ElementNode Body { get; }

    public string Title
    {
        get => TitleNode().Text;
        set => TitleNode().Text = string.IsNullOrWhiteSpace(value) ? DefaultTitle() : value.Trim();
    }

    public string Language
    {
        get => Root.Attributes.GetValueOrDefault("lang", DefaultLanguage);
        set => Root.Attributes["lang"] = string.IsNullOrWhiteSpace(value) ? DefaultLanguage : value.Trim();
    }

    public static HtmlDocument FromHtml(string html)
    {
        var document = new HtmlDocument();
        document.Head.Children.Clear();
        document.Body.Children.Clear();

        document.Import(html ?? string.Empty);

        if (document.Head.Children.All(child => !string.Equals(child.Tag, "title", StringComparison.OrdinalIgnoreCase)))
        {
            document.Title = Localizer.T("default.imported");
        }

        return document;
    }

    public IEnumerable<ElementNode> Nodes()
    {
        yield return Root;
        foreach (var child in ChildNodes(Root))
        {
            yield return child;
        }
    }

    public ElementNode? FindNode(string nodeId) =>
        Nodes().FirstOrDefault(node => node.NodeId == nodeId);

    public ElementNode? FindParent(string childId) => FindParent(Root, childId);

    public bool RemoveNode(string nodeId)
    {
        var parent = FindParent(nodeId);
        return parent is not null && parent.Children.RemoveAll(child => child.NodeId == nodeId) > 0;
    }

    public bool MoveNode(string nodeId, int direction)
    {
        var parent = FindParent(nodeId);
        if (parent is null)
        {
            return false;
        }

        var index = parent.Children.FindIndex(child => child.NodeId == nodeId);
        var newIndex = index + direction;
        if (index < 0 || newIndex < 0 || newIndex >= parent.Children.Count)
        {
            return false;
        }

        (parent.Children[index], parent.Children[newIndex]) = (parent.Children[newIndex], parent.Children[index]);
        return true;
    }

    public string ToHtml()
    {
        TagCatalog.NormalizeTree(Root);
        var builder = new StringBuilder();
        builder.AppendLine("<!doctype html>");
        builder.AppendLine(RenderNode(Root, 0));
        return builder.ToString();
    }

    private void Import(string html)
    {
        var stack = new Stack<ElementNode>();
        stack.Push(Root);

        var position = 0;
        foreach (Match match in TokenRegex.Matches(html))
        {
            AppendText(html[position..match.Index], stack.Peek());
            ProcessToken(match.Value, stack);
            position = match.Index + match.Length;
        }

        AppendText(html[position..], stack.Peek());
    }

    private void ProcessToken(string token, Stack<ElementNode> stack)
    {
        if (token.StartsWith("<!--", StringComparison.Ordinal) || token.StartsWith("<!", StringComparison.Ordinal))
        {
            return;
        }

        if (token.StartsWith("</", StringComparison.Ordinal))
        {
            var closingTag = ExtractTagName(token);
            if (!string.IsNullOrWhiteSpace(closingTag))
            {
                CloseTag(stack, closingTag);
            }

            return;
        }

        var selfClosing = token.EndsWith("/>", StringComparison.Ordinal);
        var tagName = ExtractTagName(token);
        if (string.IsNullOrWhiteSpace(tagName))
        {
            return;
        }

        var attributes = ExtractAttributes(token);
        if (TryApplyStructuralToken(tagName, attributes, stack))
        {
            return;
        }

        if (stack.Peek() == Root)
        {
            stack.Push(Body);
        }

        var parent = stack.Peek();
        var node = new ElementNode(tagName);
        ApplyAttributes(node.Attributes, attributes);
        parent.Children.Add(node);

        if (!selfClosing && !TagCatalog.IsVoidTag(tagName))
        {
            stack.Push(node);
        }
    }

    private bool TryApplyStructuralToken(string tagName, IReadOnlyDictionary<string, string> attributes, Stack<ElementNode> stack)
    {
        if (string.Equals(tagName, "html", StringComparison.OrdinalIgnoreCase))
        {
            ApplyAttributes(Root.Attributes, attributes);
            return true;
        }

        if (string.Equals(tagName, "head", StringComparison.OrdinalIgnoreCase))
        {
            ApplyAttributes(Head.Attributes, attributes);
            stack.Push(Head);
            return true;
        }

        if (string.Equals(tagName, "body", StringComparison.OrdinalIgnoreCase))
        {
            ApplyAttributes(Body.Attributes, attributes);
            stack.Push(Body);
            return true;
        }

        return false;
    }

    private static void CloseTag(Stack<ElementNode> stack, string closingTag)
    {
        if (string.Equals(closingTag, "html", StringComparison.OrdinalIgnoreCase))
        {
            while (stack.Count > 1)
            {
                stack.Pop();
            }

            return;
        }

        if (string.Equals(closingTag, "head", StringComparison.OrdinalIgnoreCase))
        {
            PopUntil(stack, "head");
            return;
        }

        if (string.Equals(closingTag, "body", StringComparison.OrdinalIgnoreCase))
        {
            PopUntil(stack, "body");
            return;
        }

        PopUntil(stack, closingTag);
    }

    private static void PopUntil(Stack<ElementNode> stack, string tagName)
    {
        while (stack.Count > 1)
        {
            var current = stack.Pop();
            if (string.Equals(current.Tag, tagName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }
    }

    private static void AppendText(string rawText, ElementNode current)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return;
        }

        var decoded = WebUtility.HtmlDecode(rawText);
        var normalized = string.Equals(current.Tag, "pre", StringComparison.OrdinalIgnoreCase)
            ? decoded
            : WhitespaceRegex.Replace(decoded, " ").Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(current.Text))
        {
            current.Text += " ";
        }

        current.Text += normalized;
    }

    private static string ExtractTagName(string token)
    {
        var inner = token.Trim('<', '>', '/').Trim();
        if (inner.StartsWith("!", StringComparison.Ordinal))
        {
            return string.Empty;
        }

        var spaceIndex = inner.IndexOfAny([' ', '\t', '\r', '\n']);
        return spaceIndex >= 0 ? inner[..spaceIndex] : inner;
    }

    private static Dictionary<string, string> ExtractAttributes(string token)
    {
        var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var inner = token.Trim('<', '>', '/').Trim();
        var spaceIndex = inner.IndexOfAny([' ', '\t', '\r', '\n']);
        if (spaceIndex < 0)
        {
            return attributes;
        }

        var attributePart = inner[(spaceIndex + 1)..];
        foreach (Match match in AttributeRegex.Matches(attributePart))
        {
            var name = match.Groups[1].Value;
            var value = match.Groups[2].Success ? match.Groups[2].Value
                : match.Groups[3].Success ? match.Groups[3].Value
                : match.Groups[4].Success ? match.Groups[4].Value
                : string.Empty;

            if (!string.IsNullOrWhiteSpace(name))
            {
                attributes[name] = WebUtility.HtmlDecode(value);
            }
        }

        return attributes;
    }

    private static void ApplyAttributes(Dictionary<string, string> target, IReadOnlyDictionary<string, string> source)
    {
        target.Clear();
        foreach (var pair in source)
        {
            target[pair.Key] = pair.Value;
        }
    }

    private ElementNode TitleNode()
    {
        var existing = Head.Children.FirstOrDefault(child => string.Equals(child.Tag, "title", StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            return existing;
        }

        var title = new ElementNode("title", DefaultTitle());
        Head.Children.Add(title);
        return title;
    }

    private static string DefaultTitle() => Localizer.T("default.untitled");

    private static IEnumerable<ElementNode> ChildNodes(ElementNode node)
    {
        foreach (var child in node.Children)
        {
            yield return child;
            foreach (var descendant in ChildNodes(child))
            {
                yield return descendant;
            }
        }
    }

    private static ElementNode? FindParent(ElementNode parent, string childId)
    {
        foreach (var child in parent.Children)
        {
            if (child.NodeId == childId)
            {
                return parent;
            }

            var result = FindParent(child, childId);
            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    private static string RenderNode(ElementNode node, int indentLevel)
    {
        var indent = new string(' ', indentLevel);
        var startTag = $"{indent}<{node.Tag}{RenderAttributes(node.Attributes)}>";

        if (TagCatalog.IsVoidTag(node.Tag))
        {
            return startTag;
        }

        var hasText = !string.IsNullOrEmpty(node.Text);
        var renderedText = hasText ? ElementNode.Escape(node.Text) : string.Empty;
        var renderedChildren = node.Children.Select(child => RenderNode(child, indentLevel + 2)).ToList();

        if (!hasText && renderedChildren.Count == 0)
        {
            return $"{startTag}</{node.Tag}>";
        }

        if (renderedChildren.Count == 0)
        {
            return $"{startTag}{renderedText}</{node.Tag}>";
        }

        if (hasText)
        {
            return $"{startTag}{renderedText}{Environment.NewLine}{string.Join(Environment.NewLine, renderedChildren)}{Environment.NewLine}{indent}</{node.Tag}>";
        }

        return $"{startTag}{Environment.NewLine}{string.Join(Environment.NewLine, renderedChildren)}{Environment.NewLine}{indent}</{node.Tag}>";
    }

    private static string RenderAttributes(Dictionary<string, string> attributes)
    {
        if (attributes.Count == 0)
        {
            return string.Empty;
        }

        var rendered = attributes.Select(pair =>
            string.IsNullOrEmpty(pair.Value)
                ? ElementNode.Escape(pair.Key)
                : $"{ElementNode.Escape(pair.Key)}=\"{ElementNode.Escape(pair.Value)}\"");

        return " " + string.Join(" ", rendered);
    }
}
