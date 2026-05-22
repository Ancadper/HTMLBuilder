using HTMLBuilder.Models;
using BuilderDocument = HTMLBuilder.Models.HtmlDocument;

namespace HTMLBuilder.Ui;

internal static class RuleComplianceVerifier
{
    public static int Run(TextWriter output)
    {
        Localizer.CurrentLanguage = Localizer.English;
        var checks = new List<(string Name, bool Passed)>
        {
            ("new document", Valid(new BuilderDocument())),
            ("li allows a", TagCatalog.CanAddChild("li", "a")),
            ("li allows nested ul", TagCatalog.CanAddChild("li", "ul")),
            ("p allows phrasing a", TagCatalog.CanAddChild("p", "a")),
            ("p blocks block div", !TagCatalog.CanAddChild("p", "div")),
            ("ul only offers li", Sequence(TagCatalog.TagsForParent("ul"), "li")),
            ("table blocks direct td", !TagCatalog.CanAddChild("table", "td")),
            ("tr allows td and th", TagCatalog.CanAddChild("tr", "td") && TagCatalog.CanAddChild("tr", "th")),
            ("select only offers option", Sequence(TagCatalog.TagsForParent("select"), "option")),
            ("a blocks nested a", !TagCatalog.CanAddChild("a", "a")),
            ("a blocks button", !TagCatalog.CanAddChild("a", "button")),
            ("button allows strong", TagCatalog.CanAddChild("button", "strong")),
            ("button blocks a", !TagCatalog.CanAddChild("button", "a")),
            ("button blocks input", !TagCatalog.CanAddChild("button", "input")),
            ("label blocks label", !TagCatalog.CanAddChild("label", "label")),
            ("head only offers head tags", Sequence(TagCatalog.TagsForParent("head"), "title", "meta", "link")),
            ("valid ul li a chain", Valid(WithBody(UlLiAnchor()))),
            ("img requires src and alt", Invalid(WithBody(new ElementNode("img")))),
            ("link requires rel and href", Invalid(LinkDocument(new ElementNode("link")))),
            ("meta requires charset or name plus content", Invalid(LinkDocument(new ElementNode("meta")))),
            ("ul requires li", Invalid(WithBody(new ElementNode("ul")))),
            ("ol requires li", Invalid(WithBody(new ElementNode("ol")))),
            ("select requires option", Invalid(WithBody(new ElementNode("select")))),
            ("tr requires a cell", Invalid(WithBody(TableWith(new ElementNode("tr"))))),
            ("table requires tr", Invalid(WithBody(new ElementNode("table")))),
            ("picture requires exactly one img", Invalid(WithBody(new ElementNode("picture")))),
            ("picture rejects multiple img", Invalid(WithBody(PictureWithImages(2)))),
            ("picture rejects source after img", Invalid(WithBody(PictureWithSourceAfterImage()))),
            ("caption must be first", Invalid(WithBody(TableWithLateCaption()))),
            ("legend must be first", Invalid(WithBody(FieldsetWithLateLegend()))),
            ("summary must be first", Invalid(WithBody(DetailsWithLateSummary()))),
            ("form cannot nest form", !TagCatalog.CanAddChild("form", "form")),
            ("audio requires src or source", Invalid(WithBody(new ElementNode("audio")))),
            ("form method is get or post", Invalid(WithBody(FormWithMethod("put")))),
            ("ol start is numeric", Invalid(WithBody(OrderedList("first", "x")))),
            ("input checked needs checkbox or radio", Invalid(WithBody(Input("text", "checked")))),
            ("input placeholder blocks checkbox", Invalid(WithBody(Input("checkbox", "placeholder")))),
            ("target blank normalizes rel", AnchorNormalization())
        };

        foreach (var check in checks)
        {
            output.WriteLine($"{(check.Passed ? "PASS" : "FAIL")} {check.Name}");
        }

        var failed = checks.Count(check => !check.Passed);
        output.WriteLine($"{checks.Count - failed}/{checks.Count} rule checks passed.");
        return failed == 0 ? 0 : 1;
    }

    private static bool Valid(BuilderDocument document) => TagCatalog.ValidateDocument(document).Count == 0;
    private static bool Invalid(BuilderDocument document) => TagCatalog.ValidateDocument(document).Count > 0;

    private static bool Sequence(IReadOnlyList<string> actual, params string[] expected) =>
        actual.SequenceEqual(expected, StringComparer.OrdinalIgnoreCase);

    private static BuilderDocument WithBody(params ElementNode[] children)
    {
        var document = new BuilderDocument();
        document.Body.Children.AddRange(children);
        return document;
    }

    private static BuilderDocument LinkDocument(ElementNode headChild)
    {
        var document = new BuilderDocument();
        document.Head.Children.Add(headChild);
        return document;
    }

    private static ElementNode UlLiAnchor()
    {
        var anchor = new ElementNode("a", "Products");
        anchor.Attributes["href"] = "/products";
        var item = new ElementNode("li");
        item.Children.Add(anchor);
        var list = new ElementNode("ul");
        list.Children.Add(item);
        return list;
    }

    private static ElementNode TableWith(params ElementNode[] children)
    {
        var table = new ElementNode("table");
        table.Children.AddRange(children);
        return table;
    }

    private static ElementNode Caption() => new("caption", "Caption");

    private static ElementNode Cell() => new("td", "Cell");

    private static ElementNode TableWithLateCaption()
    {
        var row = new ElementNode("tr");
        row.Children.Add(Cell());
        return TableWith(row, Caption());
    }

    private static ElementNode PictureWithImages(int count)
    {
        var picture = new ElementNode("picture");
        for (var index = 0; index < count; index++)
        {
            picture.Children.Add(Image());
        }

        return picture;
    }

    private static ElementNode PictureWithSourceAfterImage()
    {
        var picture = new ElementNode("picture");
        picture.Children.Add(Image());
        picture.Children.Add(Source());
        return picture;
    }

    private static ElementNode Image()
    {
        var image = new ElementNode("img");
        image.Attributes["src"] = "image.jpg";
        image.Attributes["alt"] = "Image";
        return image;
    }

    private static ElementNode Source()
    {
        var source = new ElementNode("source");
        source.Attributes["src"] = "image.webp";
        return source;
    }

    private static ElementNode FieldsetWith(params ElementNode[] children)
    {
        var fieldset = new ElementNode("fieldset");
        fieldset.Children.AddRange(children);
        return fieldset;
    }

    private static ElementNode FieldsetWithLateLegend() =>
        FieldsetWith(new ElementNode("p", "Data"), new ElementNode("legend", "Legend"));

    private static ElementNode DetailsWith(params ElementNode[] children)
    {
        var details = new ElementNode("details");
        details.Children.AddRange(children);
        return details;
    }

    private static ElementNode DetailsWithLateSummary() =>
        DetailsWith(new ElementNode("p", "Data"), new ElementNode("summary", "Summary"));

    private static ElementNode FormWithMethod(string method)
    {
        var form = new ElementNode("form");
        form.Attributes["method"] = method;
        return form;
    }

    private static ElementNode OrderedList(string text, string start)
    {
        var list = new ElementNode("ol");
        list.Attributes["start"] = start;
        list.Children.Add(new ElementNode("li", text));
        return list;
    }

    private static ElementNode Input(string type, string attribute)
    {
        var input = new ElementNode("input");
        input.Attributes["type"] = type;
        input.Attributes[attribute] = attribute == "placeholder" ? "Example" : string.Empty;
        return input;
    }

    private static bool AnchorNormalization()
    {
        var anchor = new ElementNode("a", "External");
        anchor.Attributes["href"] = "https://example.com";
        anchor.Attributes["target"] = "_blank";
        TagCatalog.NormalizeAttributes(anchor);
        return anchor.Attributes.TryGetValue("rel", out var rel)
            && rel.Contains("noopener", StringComparison.OrdinalIgnoreCase)
            && rel.Contains("noreferrer", StringComparison.OrdinalIgnoreCase);
    }
}
