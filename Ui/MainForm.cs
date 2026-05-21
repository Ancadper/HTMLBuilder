using HTMLBuilder.Models;
using System.ComponentModel;
using System.Diagnostics;
using HtmlBuilderDocument = HTMLBuilder.Models.HtmlDocument;

namespace HTMLBuilder.Ui;

public sealed class MainForm : Form
{
    private readonly AppSettings settings;
    private HtmlBuilderDocument document = new();
    private string? currentPath;
    private readonly Dictionary<string, TreeNode> treeNodes = [];
    private readonly TreeView structureTree = new();
    private readonly TextBox htmlPreview = new();

    public MainForm(AppSettings settings)
    {
        this.settings = settings;
        Text = "HTMLBuilder";
        AccessibleName = Text;
        Width = 980;
        Height = 660;
        StartPosition = FormStartPosition.CenterScreen;
        Shown += (_, _) => structureTree.Focus();
        Shown += (_, _) => ShowInitialLanguageDialogIfNeeded();

        BuildMenu();
        BuildUi();
        RefreshAll();
    }

    private void BuildMenu()
    {
        var menu = new MenuStrip { Dock = DockStyle.Top };
        MainMenuStrip = menu;

        var fileMenu = new ToolStripMenuItem(Localizer.T("menu.file"));
        fileMenu.DropDownItems.Add(MenuItem(Localizer.T("menu.new"), Keys.Control | Keys.N, OnNew));
        fileMenu.DropDownItems.Add(MenuItem(Localizer.T("menu.open"), Keys.Control | Keys.O, OnOpenText));
        fileMenu.DropDownItems.Add(MenuItem(Localizer.T("menu.save"), Keys.Control | Keys.S, OnSave));
        fileMenu.DropDownItems.Add(MenuItem(Localizer.T("menu.preferences"), Keys.Control | Keys.Oemcomma, OnPreferences));
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(MenuItem(Localizer.T("menu.exit"), Keys.Alt | Keys.F4, (_, _) => Close()));

        var documentMenu = new ToolStripMenuItem(Localizer.T("menu.document"));
        documentMenu.DropDownItems.Add(MenuItem(Localizer.T("menu.properties"), Keys.Control | Keys.P, OnProperties));

        var elementMenu = new ToolStripMenuItem(Localizer.T("menu.element"));
        elementMenu.DropDownItems.Add(MenuItem(Localizer.T("menu.addChild"), Keys.Control | Keys.E, OnAddChild));
        elementMenu.DropDownItems.Add(MenuItem(Localizer.T("menu.editElement"), Keys.F2, OnEditElement));
        elementMenu.DropDownItems.Add(MenuItem(Localizer.T("menu.deleteElement"), Keys.Delete, OnDeleteElement));
        elementMenu.DropDownItems.Add(new ToolStripSeparator());
        elementMenu.DropDownItems.Add(MenuItem(Localizer.T("menu.moveUp"), Keys.Control | Keys.Up, (_, _) => MoveSelected(-1)));
        elementMenu.DropDownItems.Add(MenuItem(Localizer.T("menu.moveDown"), Keys.Control | Keys.Down, (_, _) => MoveSelected(1)));

        var attributeMenu = new ToolStripMenuItem(Localizer.T("menu.attributes"));
        attributeMenu.DropDownItems.Add(MenuItem(Localizer.T("menu.addAttribute"), Keys.Control | Keys.A, OnAddAttribute));
        attributeMenu.DropDownItems.Add(MenuItem(Localizer.T("menu.removeAttribute"), Keys.Control | Keys.R, OnRemoveAttribute));

        var viewMenu = new ToolStripMenuItem(Localizer.T("menu.view"));
        viewMenu.DropDownItems.Add(MenuItem(Localizer.T("menu.refreshPreview"), Keys.F5, (_, _) => RefreshAll()));
        viewMenu.DropDownItems.Add(MenuItem(Localizer.T("menu.openBrowser"), Keys.Control | Keys.F5, OnOpenInBrowser));
        viewMenu.DropDownItems.Add(MenuItem(Localizer.T("menu.copyHtml"), Keys.Control | Keys.Shift | Keys.C, OnCopyHtml));

        var helpMenu = new ToolStripMenuItem(Localizer.T("menu.help"));
        helpMenu.DropDownItems.Add(MenuItem(Localizer.T("menu.about"), Keys.None, OnAbout));

        menu.Items.AddRange([fileMenu, documentMenu, elementMenu, attributeMenu, viewMenu, helpMenu]);
        Controls.Add(menu);
        Controls.SetChildIndex(menu, 0);
    }

    private void BuildUi()
    {
        structureTree.Dock = DockStyle.Fill;
        structureTree.HideSelection = false;
        structureTree.AccessibleName = Localizer.T("access.tree");
        structureTree.NodeMouseDoubleClick += (_, _) => OnEditElement(this, EventArgs.Empty);

        htmlPreview.Dock = DockStyle.Fill;
        htmlPreview.Multiline = true;
        htmlPreview.ReadOnly = true;
        htmlPreview.WordWrap = false;
        htmlPreview.ScrollBars = ScrollBars.Both;
        htmlPreview.Font = new Font(FontFamily.GenericMonospace, 10);
        htmlPreview.AccessibleName = Localizer.T("access.preview");

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var treePanel = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
        var previewPanel = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };

        treePanel.Controls.Add(structureTree);
        previewPanel.Controls.Add(htmlPreview);

        layout.Controls.Add(treePanel, 0, 0);
        layout.Controls.Add(previewPanel, 1, 0);
        Controls.Add(layout);
    }

    private void RebuildChrome()
    {
        if (MainMenuStrip is not null)
        {
            Controls.Remove(MainMenuStrip);
            MainMenuStrip.Dispose();
            MainMenuStrip = null;
        }

        BuildMenu();
        structureTree.AccessibleName = Localizer.T("access.tree");
        htmlPreview.AccessibleName = Localizer.T("access.preview");
    }

    private void ShowInitialLanguageDialogIfNeeded()
    {
        if (settings.HasAskedLanguage)
        {
            return;
        }

        using var dialog = new LanguageChoiceDialog();
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            ApplyLanguage(dialog.SelectedLanguage);
        }

        settings.HasAskedLanguage = true;
        settings.Save();
    }

    private void ApplyLanguage(string language)
    {
        settings.Language = Localizer.NormalizeLanguage(language);
        Localizer.CurrentLanguage = settings.Language;
        settings.Save();
        RebuildChrome();
        RefreshAll();
    }

    private void RefreshAll()
    {
        var selectedId = SelectedNode().NodeId;
        structureTree.BeginUpdate();
        structureTree.Nodes.Clear();
        treeNodes.Clear();

        var rootNode = CreateTreeNode(document.Root);
        structureTree.Nodes.Add(rootNode);
        AddChildren(rootNode, document.Root);
        structureTree.ExpandAll();
        structureTree.SelectedNode = treeNodes.GetValueOrDefault(selectedId, rootNode);
        structureTree.EndUpdate();

        htmlPreview.Text = document.ToHtml();
        htmlPreview.SelectionStart = 0;
        htmlPreview.SelectionLength = 0;
        structureTree.Focus();
    }

    private void AddChildren(TreeNode parentTreeNode, ElementNode parentNode)
    {
        foreach (var child in parentNode.Children)
        {
            var childTreeNode = CreateTreeNode(child);
            parentTreeNode.Nodes.Add(childTreeNode);
            AddChildren(childTreeNode, child);
        }
    }

    private TreeNode CreateTreeNode(ElementNode node)
    {
        var treeNode = new TreeNode(node.Label) { Tag = node.NodeId };
        treeNodes[node.NodeId] = treeNode;
        return treeNode;
    }

    private ElementNode SelectedNode()
    {
        if (structureTree.SelectedNode?.Tag is string nodeId)
        {
            return document.FindNode(nodeId) ?? document.Root;
        }

        return document.Root;
    }

    private void OnNew(object? sender, EventArgs eventArgs)
    {
        if (MessageBox.Show(this, Localizer.T("dialog.newConfirm"), Localizer.T("title.newDocument"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        document = new HtmlBuilderDocument();
        currentPath = null;
        RefreshAll();
    }

    private void OnOpenText(object? sender, EventArgs eventArgs)
    {
        using var dialog = new OpenFileDialog
        {
            Title = Localizer.T("dialog.openTitle"),
            Filter = Localizer.T("dialog.openFilter")
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var html = File.ReadAllText(dialog.FileName);
            document = HtmlBuilderDocument.FromHtml(html);
            currentPath = dialog.FileName;
            RefreshAll();
        }
        catch (IOException ex)
        {
            MessageBox.Show(this, ex.Message, Localizer.T("dialog.openError"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnSave(object? sender, EventArgs eventArgs)
    {
        using var dialog = new SaveFileDialog
        {
            Title = Localizer.T("dialog.saveTitle"),
            Filter = Localizer.T("dialog.saveFilter"),
            FileName = currentPath ?? Localizer.T("dialog.defaultFileName")
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            File.WriteAllText(dialog.FileName, document.ToHtml());
            currentPath = dialog.FileName;
        }
        catch (IOException ex)
        {
            MessageBox.Show(this, ex.Message, Localizer.T("dialog.saveError"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnProperties(object? sender, EventArgs eventArgs)
    {
        using var dialog = new DocumentDialog(document.Title, document.Language);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        document.Title = dialog.DocumentTitle;
        document.Language = dialog.Language;
        RefreshAll();
    }

    private void OnPreferences(object? sender, EventArgs eventArgs)
    {
        using var dialog = new PreferencesDialog(settings.Language);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        ApplyLanguage(dialog.SelectedLanguage);
    }

    private void OnAddChild(object? sender, EventArgs eventArgs)
    {
        var parent = SelectedNode();
        var allowedChildren = TagCatalog.TagsForParent(parent.Tag);
        if (allowedChildren.Count == 0)
        {
            MessageBox.Show(this, string.Format(Localizer.T("msg.noChildren"), parent.Tag), Localizer.T("title.invalidStructure"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new ElementDialog(Localizer.T("title.addChild"), parent.Tag);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var child = new ElementNode(dialog.SelectedTag, dialog.ElementText);
        ApplyAttributes(child, dialog.ElementAttributes);
        parent.Children.Add(child);
        RefreshAll();
    }

    private void OnEditElement(object? sender, EventArgs eventArgs)
    {
        var node = SelectedNode();
        if (ReferenceEquals(node, document.Root) || ReferenceEquals(node, document.Head) || ReferenceEquals(node, document.Body))
        {
            MessageBox.Show(this, Localizer.T("msg.structuralNotEditable"), Localizer.T("title.documentStructure"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var parent = document.FindParent(node.NodeId) ?? document.Body;
        using var dialog = new ElementDialog(Localizer.T("title.editElement"), parent.Tag, node.Tag, node.Text, node.Attributes);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        node.Tag = dialog.SelectedTag;
        node.Text = dialog.ElementText;
        ApplyAttributes(node, dialog.ElementAttributes);
        RefreshAll();
    }

    private void OnDeleteElement(object? sender, EventArgs eventArgs)
    {
        var node = SelectedNode();
        if (ReferenceEquals(node, document.Root) || ReferenceEquals(node, document.Head) || ReferenceEquals(node, document.Body))
        {
            MessageBox.Show(this, Localizer.T("msg.structuralNotDelete"), Localizer.T("title.documentStructure"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (MessageBox.Show(this, string.Format(Localizer.T("msg.deleteConfirm"), node.Label), Localizer.T("title.confirmDelete"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            document.RemoveNode(node.NodeId);
            RefreshAll();
        }
    }

    private void MoveSelected(int direction)
    {
        var node = SelectedNode();
        if (document.MoveNode(node.NodeId, direction))
        {
            RefreshAll();
            return;
        }

        MessageBox.Show(this, Localizer.T("msg.cannotMove"), Localizer.T("title.moveElement"), MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void OnAddAttribute(object? sender, EventArgs eventArgs)
    {
        var node = SelectedNode();
        if (TagCatalog.AttributesFor(node.Tag).Count == 0)
        {
            MessageBox.Show(this, string.Format(Localizer.T("msg.noEditableAttributes"), node.Tag), Localizer.T("title.noAttributes"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new AttributeDialog(node.Tag);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        node.Attributes[dialog.AttributeName] = TagCatalog.IsBooleanAttribute(dialog.AttributeName)
            ? string.Empty
            : dialog.AttributeValue;
        RefreshAll();
    }

    private void OnRemoveAttribute(object? sender, EventArgs eventArgs)
    {
        var node = SelectedNode();
        if (node.Attributes.Count == 0)
        {
            MessageBox.Show(this, Localizer.T("msg.noAttributesOnElement"), Localizer.T("title.noAttributes"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new AttributeChoiceDialog(node.Attributes.Keys.Order().ToArray());
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.SelectedAttribute is null)
        {
            return;
        }

        node.Attributes.Remove(dialog.SelectedAttribute);
        RefreshAll();
    }

    private void OnCopyHtml(object? sender, EventArgs eventArgs)
    {
        Clipboard.SetText(document.ToHtml());
        MessageBox.Show(this, Localizer.T("msg.copiedHtml"), Localizer.T("title.copyHtml"), MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void OnOpenInBrowser(object? sender, EventArgs eventArgs)
    {
        try
        {
            var previewDirectory = Path.Combine(AppContext.BaseDirectory, "preview");
            Directory.CreateDirectory(previewDirectory);

            var previewPath = Path.Combine(previewDirectory, Localizer.T("dialog.previewFileName"));
            File.WriteAllText(previewPath, document.ToHtml());

            Process.Start(new ProcessStartInfo
            {
                FileName = previewPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or Win32Exception)
        {
            MessageBox.Show(this, ex.Message, Localizer.T("dialog.browserError"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnAbout(object? sender, EventArgs eventArgs)
    {
        MessageBox.Show(
            this,
            Localizer.T("about.text"),
            Localizer.T("menu.about"),
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private static ToolStripMenuItem MenuItem(string text, Keys shortcut, EventHandler handler)
    {
        var item = new ToolStripMenuItem(text);
        if (shortcut != Keys.None)
        {
            item.ShortcutKeys = shortcut;
        }

        item.Click += handler;
        return item;
    }

    private static void ApplyAttributes(ElementNode node, IReadOnlyDictionary<string, string> attributes)
    {
        node.Attributes.Clear();
        foreach (var pair in attributes)
        {
            node.Attributes[pair.Key] = pair.Value;
        }
    }
}

internal sealed class AttributeChoiceDialog : Form
{
    private readonly ListBox list = new();

    public AttributeChoiceDialog(string[] attributes)
    {
        Text = Localizer.T("button.removeAttribute");
        AccessibleName = Localizer.T("button.removeAttribute");
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ClientSize = new Size(360, 260);

        list.Dock = DockStyle.Fill;
        list.Items.AddRange(attributes);
        list.AccessibleName = Localizer.T("access.selectedAttributes");
        if (list.Items.Count > 0)
        {
            list.SelectedIndex = 0;
        }

        var okButton = new Button { Text = Localizer.T("button.removeAttribute"), DialogResult = DialogResult.OK, AutoSize = true };
        var cancelButton = new Button { Text = Localizer.T("button.cancel"), DialogResult = DialogResult.Cancel, AutoSize = true };
        AcceptButton = okButton;
        CancelButton = cancelButton;

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Padding = new Padding(8),
            Controls = { cancelButton, okButton }
        };

        Controls.Add(list);
        Controls.Add(buttons);
    }

    public string? SelectedAttribute => list.SelectedItem?.ToString();
}
