namespace HTMLBuilder.Models;

public sealed class ElementNode
{
    public ElementNode(string tag, string text = "")
    {
        Tag = tag;
        Text = text;
    }

    public string NodeId { get; } = Guid.NewGuid().ToString("N");
    public string Tag { get; set; }
    public string Text { get; set; }
    public Dictionary<string, string> Attributes { get; } = new(StringComparer.Ordinal);
    public List<ElementNode> Children { get; } = [];

    public string Label
    {
        get
        {
            var label = $"<{Tag}>";
            if (Attributes.TryGetValue("id", out var elementId) && !string.IsNullOrWhiteSpace(elementId))
            {
                label += $" id={elementId}";
            }

            if (string.Equals(Tag, "meta", StringComparison.OrdinalIgnoreCase))
            {
                if (Attributes.TryGetValue("charset", out var charset) && !string.IsNullOrWhiteSpace(charset))
                {
                    label += $": charset={charset}";
                }
                else if (Attributes.TryGetValue("content", out var content) && !string.IsNullOrWhiteSpace(content))
                {
                    if (Attributes.TryGetValue("name", out var name) && !string.IsNullOrWhiteSpace(name))
                    {
                        label += $": {name}={content}";
                    }
                    else
                    {
                        label += $": {content}";
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(Text))
            {
                var preview = string.Join(" ", Text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
                if (preview.Length > 40)
                {
                    preview = preview[..37] + "...";
                }

                label += $": {preview}";
            }

            return label;
        }
    }

    public override string ToString() => Label;

    public static string Escape(string value) =>
        value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
}
