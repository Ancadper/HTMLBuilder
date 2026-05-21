using System.Text.RegularExpressions;

namespace HTMLBuilder.Ui;

internal sealed partial class ElementDialog : Form
{
    private static readonly Regex TagPattern = new("^[a-z][a-z0-9-]*$", RegexOptions.Compiled);
    private readonly string parentTag;
    private readonly ComboBox tagCombo = new();
    private readonly Label parentHintLabel = CreateLabel(string.Empty);
    private readonly Label fieldTitleLabel = CreateLabel(Localizer.T("label.tagFields"));
    private readonly Label textLabel = CreateLabel(Localizer.T("label.textContent"));
    private readonly TextBox textBox = new();
    private readonly TableLayoutPanel fieldPanel = new();
    private readonly ListBox attributeList = new();
    private readonly Dictionary<string, string> attributes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, TextBox> generatedFieldBoxes = new(StringComparer.OrdinalIgnoreCase);

    public ElementDialog(string title, string parentTag, string tag = "p", string text = "", IReadOnlyDictionary<string, string>? existingAttributes = null)
    {
        this.parentTag = parentTag;
        ConfigureModalForm(title, new Size(560, 590));

        if (existingAttributes is not null)
        {
            foreach (var pair in existingAttributes)
            {
                attributes[pair.Key] = pair.Value;
            }
        }

        var availableTags = TagCatalog.TagsForParent(parentTag);
        tagCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        tagCombo.Items.AddRange(availableTags.ToArray());
        tagCombo.AccessibleName = Localizer.T("access.tagCombo");
        tagCombo.SelectedIndexChanged += (_, _) => RebuildForSelectedTag();
        tagCombo.Enabled = tagCombo.Items.Count > 1;

        if (availableTags.Contains(tag, StringComparer.OrdinalIgnoreCase))
        {
            tagCombo.SelectedItem = availableTags.First(item => string.Equals(item, tag, StringComparison.OrdinalIgnoreCase));
        }
        else if (tagCombo.Items.Count > 0)
        {
            tagCombo.SelectedIndex = 0;
        }

        parentHintLabel.Text = BuildParentHint(parentTag, availableTags);
        parentHintLabel.AccessibleName = Localizer.T("access.parentHint");

        textBox.Multiline = true;
        textBox.ScrollBars = ScrollBars.Both;
        textBox.AcceptsReturn = true;
        textBox.AcceptsTab = false;
        textBox.Text = text;
        textBox.AccessibleName = Localizer.T("access.textContent");

        fieldPanel.Dock = DockStyle.Fill;
        fieldPanel.AutoSize = true;
        fieldPanel.ColumnCount = 1;
        fieldPanel.AccessibleName = Localizer.T("access.tagFields");

        attributeList.Dock = DockStyle.Fill;
        attributeList.AccessibleName = Localizer.T("access.attributeList");

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            RowCount = 9,
            ColumnCount = 1,
            RowStyles =
            {
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.Percent, 100),
                new RowStyle(SizeType.Absolute, 130),
                new RowStyle(SizeType.AutoSize)
            }
        };

        layout.Controls.Add(CreateLabel(Localizer.T("label.htmlTag")), 0, 0);
        layout.Controls.Add(tagCombo, 0, 1);
        layout.Controls.Add(parentHintLabel, 0, 2);
        layout.Controls.Add(fieldTitleLabel, 0, 3);
        layout.Controls.Add(fieldPanel, 0, 4);
        layout.Controls.Add(textLabel, 0, 5);
        layout.Controls.Add(textBox, 0, 6);
        layout.Controls.Add(CreateAttributePanel(), 0, 7);
        layout.Controls.Add(CreateButtonRow(Accept), 0, 8);
        Controls.Add(layout);

        RebuildForSelectedTag();
    }

    public string SelectedTag => tagCombo.SelectedItem?.ToString() ?? "p";
    public string ElementText => TagCatalog.CanHaveText(SelectedTag) ? textBox.Text : string.Empty;
    public IReadOnlyDictionary<string, string> ElementAttributes => attributes;

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F1)
        {
            using var dialog = new TagHelpDialog(SelectedTag);
            dialog.ShowDialog(this);
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void Accept()
    {
        if (!TagPattern.IsMatch(SelectedTag))
        {
            MessageBox.Show(this, Localizer.T("msg.invalidTag"), Localizer.T("title.invalidTag"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (!TagCatalog.CanAddChild(parentTag, SelectedTag))
        {
            MessageBox.Show(this, string.Format(Localizer.T("msg.childNotAllowed"), SelectedTag, parentTag), Localizer.T("title.invalidStructure"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        ApplyGeneratedFields();
        NormalizeBooleanAttributes();

        foreach (var attribute in attributes.Keys.ToArray())
        {
            if (!TagCatalog.IsAttributeAllowed(SelectedTag, attribute))
            {
                MessageBox.Show(this, string.Format(Localizer.T("msg.attributeNotAllowed"), attribute, SelectedTag), Localizer.T("title.attributeNotAllowed"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        foreach (var field in TagCatalog.FieldsFor(SelectedTag).Where(field => field.Required))
        {
            if (!attributes.TryGetValue(field.AttributeName, out var value) || (!field.AllowsEmpty && string.IsNullOrWhiteSpace(value)))
            {
                MessageBox.Show(this, string.Format(Localizer.T("msg.requiredField"), SelectedTag, field.Label), Localizer.T("title.requiredField"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        if (string.Equals(SelectedTag, "meta", StringComparison.OrdinalIgnoreCase) && !HasValidMetaCombination())
        {
            MessageBox.Show(this, Localizer.T("msg.invalidMeta"), Localizer.T("title.incompleteMeta"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (string.Equals(SelectedTag, "a", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(textBox.Text))
        {
            MessageBox.Show(this, Localizer.T("msg.anchorNeedsText"), Localizer.T("title.requiredText"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private void ApplyGeneratedFields()
    {
        var fieldSpecs = TagCatalog.FieldsFor(SelectedTag).ToDictionary(field => field.AttributeName, StringComparer.OrdinalIgnoreCase);
        foreach (var pair in generatedFieldBoxes)
        {
            var value = pair.Value.Text.Trim();
            if (string.IsNullOrEmpty(value) && fieldSpecs.TryGetValue(pair.Key, out var field) && !field.Required)
            {
                attributes.Remove(pair.Key);
            }
            else
            {
                attributes[pair.Key] = value;
            }
        }
    }

    private void NormalizeBooleanAttributes()
    {
        foreach (var attribute in attributes.Keys.ToArray())
        {
            if (TagCatalog.IsBooleanAttribute(attribute))
            {
                attributes[attribute] = string.Empty;
            }
        }
    }

    private bool HasValidMetaCombination()
    {
        var hasCharset = attributes.TryGetValue("charset", out var charset) && !string.IsNullOrWhiteSpace(charset);
        var hasName = attributes.TryGetValue("name", out var name) && !string.IsNullOrWhiteSpace(name);
        var hasContent = attributes.TryGetValue("content", out var content) && !string.IsNullOrWhiteSpace(content);
        return hasCharset || (hasName && hasContent);
    }

    private void RebuildForSelectedTag()
    {
        RemoveAttributesNotAllowedForSelectedTag();
        RebuildGeneratedFields();
        RebuildTextField();
        RefreshAttributeList();
    }

    private void RebuildGeneratedFields()
    {
        fieldPanel.SuspendLayout();
        fieldPanel.Controls.Clear();
        fieldPanel.RowStyles.Clear();
        generatedFieldBoxes.Clear();

        var fields = TagCatalog.FieldsFor(SelectedTag);
        if (fields.Count == 0)
        {
            fieldPanel.RowCount = 1;
            fieldPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            fieldPanel.Controls.Add(CreateLabel(Localizer.T("hint.noSpecialFields")), 0, 0);
            fieldPanel.ResumeLayout();
            return;
        }

        fieldPanel.RowCount = fields.Count * 2;
        var row = 0;
        foreach (var field in fields)
        {
            fieldPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            fieldPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var labelText = field.Required ? string.Format(Localizer.T("label.requiredSuffix"), field.Label) : field.Label;
            var input = new TextBox
            {
                Dock = DockStyle.Fill,
                Text = attributes.GetValueOrDefault(field.AttributeName, string.Empty),
                AccessibleName = $"{labelText}. {field.HelpText}"
            };
            fieldPanel.Controls.Add(CreateLabel(labelText), 0, row++);
            fieldPanel.Controls.Add(input, 0, row++);
            generatedFieldBoxes[field.AttributeName] = input;
        }

        fieldPanel.ResumeLayout();
    }

    private void RebuildTextField()
    {
        var canHaveText = TagCatalog.CanHaveText(SelectedTag);
        textLabel.Visible = canHaveText;
        textBox.Visible = canHaveText;
        textBox.Enabled = canHaveText;
        if (!canHaveText)
        {
            textBox.Clear();
        }
    }

    private Panel CreateAttributePanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        var label = new Label { Text = Localizer.T("label.validAttributes"), Dock = DockStyle.Top, AutoSize = true };
        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft
        };

        var addButton = new Button { Text = Localizer.T("button.addAttribute"), AutoSize = true };
        var removeButton = new Button { Text = Localizer.T("button.removeAttribute"), AutoSize = true };
        addButton.Click += (_, _) => AddOrEditAttribute();
        removeButton.Click += (_, _) => RemoveSelectedAttribute();
        buttons.Controls.Add(removeButton);
        buttons.Controls.Add(addButton);

        panel.Controls.Add(attributeList);
        panel.Controls.Add(buttons);
        panel.Controls.Add(label);
        return panel;
    }

    private void AddOrEditAttribute()
    {
        if (TagCatalog.AttributesFor(SelectedTag).Count == 0)
        {
            MessageBox.Show(this, string.Format(Localizer.T("msg.noEditableAttributes"), SelectedTag), Localizer.T("title.noAttributes"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new AttributeDialog(SelectedTag);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        attributes[dialog.AttributeName] = TagCatalog.IsBooleanAttribute(dialog.AttributeName)
            ? string.Empty
            : dialog.AttributeValue;

        if (generatedFieldBoxes.TryGetValue(dialog.AttributeName, out var generatedBox))
        {
            generatedBox.Text = dialog.AttributeValue;
        }

        RefreshAttributeList();
    }

    private void RemoveSelectedAttribute()
    {
        if (attributeList.SelectedItem is not AttributeListItem selected)
        {
            MessageBox.Show(this, Localizer.T("msg.noSelectedAttribute"), Localizer.T("button.removeAttribute"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        attributes.Remove(selected.Name);
        if (generatedFieldBoxes.TryGetValue(selected.Name, out var generatedBox))
        {
            generatedBox.Clear();
        }

        RefreshAttributeList();
    }

    private void RefreshAttributeList()
    {
        attributeList.Items.Clear();
        foreach (var pair in attributes.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            attributeList.Items.Add(new AttributeListItem(pair.Key, pair.Value));
        }

        if (attributeList.Items.Count > 0)
        {
            attributeList.SelectedIndex = 0;
        }
    }

    private void RemoveAttributesNotAllowedForSelectedTag()
    {
        foreach (var attribute in attributes.Keys.ToArray())
        {
            if (!TagCatalog.IsAttributeAllowed(SelectedTag, attribute))
            {
                attributes.Remove(attribute);
            }
        }
    }

    private void ConfigureModalForm(string title, Size clientSize)
    {
        Text = title;
        AccessibleName = title;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ClientSize = clientSize;
    }

    private FlowLayoutPanel CreateButtonRow(Action acceptAction)
    {
        var okButton = new Button { Text = Localizer.T("button.save"), DialogResult = DialogResult.None, AutoSize = true };
        var cancelButton = new Button { Text = Localizer.T("button.cancel"), DialogResult = DialogResult.Cancel, AutoSize = true };
        okButton.Click += (_, _) => acceptAction();
        AcceptButton = okButton;
        CancelButton = cancelButton;

        return new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Controls = { cancelButton, okButton }
        };
    }

    private static Label CreateLabel(string text) => new() { Text = text, AutoSize = true, Dock = DockStyle.Fill };

    private static string BuildParentHint(string parentTag, IReadOnlyList<string> availableTags)
    {
        if (availableTags.Count == 0)
        {
            return string.Format(Localizer.T("hint.parent.none"), parentTag);
        }

        if (availableTags.Count == 1)
        {
            return string.Format(Localizer.T("hint.parent.one"), parentTag, availableTags[0]);
        }

        return string.Format(Localizer.T("hint.parent.many"), parentTag, availableTags.Count);
    }

    private sealed record AttributeListItem(string Name, string Value)
    {
        public override string ToString() => string.IsNullOrEmpty(Value) ? Name : $"{Name}=\"{Value}\"";
    }
}

internal sealed class AttributeDialog : Form
{
    private static readonly Regex AttributePattern = new("^[a-zA-Z_:][a-zA-Z0-9_:\\-.]*$", RegexOptions.Compiled);
    private readonly string tag;
    private readonly ComboBox nameCombo = new();
    private readonly Label valueLabel = CreateLabel(Localizer.T("label.value"));
    private readonly Label booleanHelpLabel = CreateLabel(Localizer.T("hint.booleanAttribute"));
    private readonly TextBox valueBox = new();
    private readonly ComboBox presetValueCombo = new();

    public AttributeDialog(string tag, string name = "", string value = "")
    {
        this.tag = tag;
        ConfigureModalForm(Localizer.T("title.editAttribute"), new Size(420, 190));

        var availableAttributes = TagCatalog.AttributesFor(tag);
        if (availableAttributes.Count == 0)
        {
            throw new InvalidOperationException(string.Format(Localizer.T("msg.tagNoAttributes"), tag));
        }

        nameCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        nameCombo.Items.AddRange(availableAttributes.ToArray());
        nameCombo.AccessibleName = Localizer.T("access.attributeName");
        nameCombo.SelectedIndexChanged += (_, _) => RebuildValueField();

        if (!string.IsNullOrWhiteSpace(name) && availableAttributes.Contains(name, StringComparer.OrdinalIgnoreCase))
        {
            nameCombo.SelectedItem = availableAttributes.First(item => string.Equals(item, name, StringComparison.OrdinalIgnoreCase));
        }
        else if (nameCombo.Items.Count > 0)
        {
            nameCombo.SelectedIndex = 0;
        }

        valueBox.Text = value;
        valueBox.AccessibleName = Localizer.T("access.attributeValue");
        presetValueCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        presetValueCombo.AccessibleName = Localizer.T("access.presetValue");

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            ColumnCount = 1,
            RowCount = 7,
            RowStyles =
            {
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize)
            }
        };

        layout.Controls.Add(CreateLabel(Localizer.T("label.attributeName")), 0, 0);
        layout.Controls.Add(nameCombo, 0, 1);
        layout.Controls.Add(valueLabel, 0, 2);
        layout.Controls.Add(valueBox, 0, 3);
        layout.Controls.Add(presetValueCombo, 0, 4);
        layout.Controls.Add(booleanHelpLabel, 0, 5);
        layout.Controls.Add(CreateButtonRow(Accept), 0, 6);
        Controls.Add(layout);

        RebuildValueField();
    }

    public string AttributeName => nameCombo.Text.Trim();
    public string AttributeValue =>
        TagCatalog.IsBooleanAttribute(AttributeName) ? string.Empty
        : TagCatalog.PresetValuesFor(AttributeName).Count > 0 ? presetValueCombo.Text.Trim()
        : valueBox.Text.Trim();

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F1)
        {
            using var dialog = new AttributeHelpDialog(AttributeName);
            dialog.ShowDialog(this);
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void Accept()
    {
        if (!AttributePattern.IsMatch(AttributeName))
        {
            MessageBox.Show(this, Localizer.T("msg.invalidAttributeName"), Localizer.T("title.invalidAttribute"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (!TagCatalog.IsAttributeAllowed(tag, AttributeName))
        {
            MessageBox.Show(this, string.Format(Localizer.T("msg.attributeNotAllowed"), AttributeName, tag), Localizer.T("title.attributeNotAllowed"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (!TagCatalog.IsBooleanAttribute(AttributeName)
            && !TagCatalog.AttributeAllowsEmpty(AttributeName)
            && string.IsNullOrWhiteSpace(AttributeValue))
        {
            MessageBox.Show(this, string.Format(Localizer.T("msg.attributeNeedsValue"), AttributeName), Localizer.T("title.requiredValue"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private void RebuildValueField()
    {
        var isBoolean = TagCatalog.IsBooleanAttribute(AttributeName);
        var presetValues = TagCatalog.PresetValuesFor(AttributeName);
        var usesPresetValues = presetValues.Count > 0 && !isBoolean;

        presetValueCombo.Items.Clear();
        if (usesPresetValues)
        {
            presetValueCombo.Items.AddRange(presetValues.ToArray());
            if (presetValues.Contains(valueBox.Text, StringComparer.OrdinalIgnoreCase))
            {
                presetValueCombo.SelectedItem = presetValues.First(item => string.Equals(item, valueBox.Text, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                presetValueCombo.SelectedIndex = 0;
            }
        }

        valueLabel.Visible = !isBoolean;
        valueBox.Visible = !isBoolean && !usesPresetValues;
        valueBox.Enabled = !isBoolean && !usesPresetValues;
        presetValueCombo.Visible = usesPresetValues;
        presetValueCombo.Enabled = usesPresetValues;
        booleanHelpLabel.Visible = isBoolean;
        if (isBoolean)
        {
            valueBox.Clear();
        }
    }

    private void ConfigureModalForm(string title, Size clientSize)
    {
        Text = title;
        AccessibleName = title;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ClientSize = clientSize;
    }

    private FlowLayoutPanel CreateButtonRow(Action acceptAction)
    {
        var okButton = new Button { Text = Localizer.T("button.save"), DialogResult = DialogResult.None, AutoSize = true };
        var cancelButton = new Button { Text = Localizer.T("button.cancel"), DialogResult = DialogResult.Cancel, AutoSize = true };
        okButton.Click += (_, _) => acceptAction();
        AcceptButton = okButton;
        CancelButton = cancelButton;

        return new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Controls = { cancelButton, okButton }
        };
    }

    private static Label CreateLabel(string text) => new() { Text = text, AutoSize = true, Dock = DockStyle.Fill };
}

internal sealed class DocumentDialog : Form
{
    private readonly TextBox titleBox = new();
    private readonly TextBox languageBox = new();

    public DocumentDialog(string title, string language)
    {
        Text = Localizer.T("title.documentProperties");
        AccessibleName = Localizer.T("title.documentProperties");
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ClientSize = new Size(420, 160);

        titleBox.Text = title;
        titleBox.AccessibleName = Localizer.T("label.documentTitle");
        languageBox.Text = language;
        languageBox.AccessibleName = Localizer.T("label.language");

        var okButton = new Button { Text = Localizer.T("button.save"), DialogResult = DialogResult.OK, AutoSize = true };
        var cancelButton = new Button { Text = Localizer.T("button.cancel"), DialogResult = DialogResult.Cancel, AutoSize = true };
        AcceptButton = okButton;
        CancelButton = cancelButton;

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Controls = { cancelButton, okButton }
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            ColumnCount = 1,
            RowCount = 5,
            RowStyles =
            {
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize)
            }
        };

        layout.Controls.Add(new Label { Text = Localizer.T("label.documentTitle"), AutoSize = true }, 0, 0);
        layout.Controls.Add(titleBox, 0, 1);
        layout.Controls.Add(new Label { Text = Localizer.T("label.language"), AutoSize = true }, 0, 2);
        layout.Controls.Add(languageBox, 0, 3);
        layout.Controls.Add(buttons, 0, 4);
        Controls.Add(layout);
    }

    public string DocumentTitle => string.IsNullOrWhiteSpace(titleBox.Text) ? Localizer.T("default.untitled") : titleBox.Text.Trim();
    public string Language => string.IsNullOrWhiteSpace(languageBox.Text) ? Localizer.CurrentLanguage : languageBox.Text.Trim();
}

internal sealed class TagHelpDialog : Form
{
    public TagHelpDialog(string tag)
    {
        var help = TagCatalog.GetHelp(tag);
        Text = string.Format(Localizer.T("help.tag.title"), tag);
        AccessibleName = Text;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ClientSize = new Size(560, 360);

        var textBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            WordWrap = false,
            ScrollBars = ScrollBars.Both,
            Dock = DockStyle.Fill,
            AccessibleName = Localizer.T("help.tag.access"),
            Text = $"{string.Format(Localizer.T("help.tag.label"), tag)}{Environment.NewLine}{Environment.NewLine}" +
                   $"{Localizer.T("help.what")}{Environment.NewLine}{help.Description}{Environment.NewLine}{Environment.NewLine}" +
                   $"{Localizer.T("help.accessibility")}{Environment.NewLine}{help.Accessibility}{Environment.NewLine}{Environment.NewLine}" +
                   $"{Localizer.T("help.advice")}{Environment.NewLine}{help.Advice}"
        };

        var okButton = new Button { Text = Localizer.T("button.close"), DialogResult = DialogResult.OK, AutoSize = true };
        AcceptButton = okButton;

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Padding = new Padding(8),
            Controls = { okButton }
        };

        Controls.Add(textBox);
        Controls.Add(buttons);
    }
}

internal sealed class AttributeHelpDialog : Form
{
    public AttributeHelpDialog(string attribute)
    {
        var help = TagCatalog.GetAttributeHelp(attribute);
        Text = string.Format(Localizer.T("help.attribute.title"), attribute);
        AccessibleName = Text;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ClientSize = new Size(560, 360);

        var textBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            WordWrap = false,
            ScrollBars = ScrollBars.Both,
            Dock = DockStyle.Fill,
            AccessibleName = Localizer.T("help.attribute.access"),
            Text = $"{string.Format(Localizer.T("help.attribute.label"), attribute)}{Environment.NewLine}{Environment.NewLine}" +
                   $"{Localizer.T("help.what")}{Environment.NewLine}{help.Description}{Environment.NewLine}{Environment.NewLine}" +
                   $"{Localizer.T("help.values")}{Environment.NewLine}{help.Values}{Environment.NewLine}{Environment.NewLine}" +
                   $"{Localizer.T("help.advice")}{Environment.NewLine}{help.Advice}"
        };

        var okButton = new Button { Text = Localizer.T("button.close"), DialogResult = DialogResult.OK, AutoSize = true };
        AcceptButton = okButton;

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Padding = new Padding(8),
            Controls = { okButton }
        };

        Controls.Add(textBox);
        Controls.Add(buttons);
    }
}

internal sealed class PreferencesDialog : Form
{
    private readonly ComboBox languageCombo = new();

    public PreferencesDialog(string selectedLanguage)
    {
        Text = Localizer.T("preferences.title");
        AccessibleName = Text;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ClientSize = new Size(360, 155);

        languageCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        languageCombo.Items.AddRange(
        [
            new LanguageChoice(Localizer.T("preferences.english"), Localizer.English),
            new LanguageChoice(Localizer.T("preferences.spanish"), Localizer.Spanish)
        ]);
        languageCombo.SelectedItem = languageCombo.Items
            .OfType<LanguageChoice>()
            .First(choice => choice.Code == Localizer.NormalizeLanguage(selectedLanguage));

        var okButton = new Button { Text = Localizer.T("button.save"), DialogResult = DialogResult.OK, AutoSize = true };
        var cancelButton = new Button { Text = Localizer.T("button.cancel"), DialogResult = DialogResult.Cancel, AutoSize = true };
        AcceptButton = okButton;
        CancelButton = cancelButton;

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Controls = { cancelButton, okButton }
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            ColumnCount = 1,
            RowCount = 4,
            RowStyles =
            {
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.Percent, 100),
                new RowStyle(SizeType.AutoSize)
            }
        };

        layout.Controls.Add(new Label { Text = Localizer.T("preferences.language"), AutoSize = true }, 0, 0);
        layout.Controls.Add(languageCombo, 0, 1);
        layout.Controls.Add(new Label { Text = Localizer.T("preferences.restartless"), AutoSize = true }, 0, 2);
        layout.Controls.Add(buttons, 0, 3);
        Controls.Add(layout);
    }

    public string SelectedLanguage => ((LanguageChoice)languageCombo.SelectedItem!).Code;
}

internal sealed class LanguageChoiceDialog : Form
{
    private readonly ComboBox languageCombo = new();

    public LanguageChoiceDialog()
    {
        Text = Localizer.T("language.title");
        AccessibleName = Text;
        StartPosition = FormStartPosition.CenterScreen;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ClientSize = new Size(360, 150);

        languageCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        languageCombo.Items.AddRange(
        [
            new LanguageChoice(Localizer.T("language.english"), Localizer.English),
            new LanguageChoice(Localizer.T("language.spanish"), Localizer.Spanish)
        ]);
        languageCombo.SelectedIndex = 0;

        var okButton = new Button { Text = Localizer.T("button.save"), DialogResult = DialogResult.OK, AutoSize = true };
        AcceptButton = okButton;

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Controls = { okButton }
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            ColumnCount = 1,
            RowCount = 4,
            RowStyles =
            {
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.Percent, 100),
                new RowStyle(SizeType.AutoSize)
            }
        };

        layout.Controls.Add(new Label { Text = Localizer.T("language.message"), AutoSize = true }, 0, 0);
        layout.Controls.Add(languageCombo, 0, 1);
        layout.Controls.Add(new Panel(), 0, 2);
        layout.Controls.Add(buttons, 0, 3);
        Controls.Add(layout);
    }

    public string SelectedLanguage => ((LanguageChoice)languageCombo.SelectedItem!).Code;
}

internal sealed record LanguageChoice(string Name, string Code)
{
    public override string ToString() => Name;
}
