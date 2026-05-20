using HTMLBuilder.Models;
using System.ComponentModel;
using System.Diagnostics;
using HtmlBuilderDocument = HTMLBuilder.Models.HtmlDocument;

namespace HTMLBuilder.Ui;

public sealed class MainForm : Form
{
    private HtmlBuilderDocument document = new();
    private string? currentPath;
    private readonly Dictionary<string, TreeNode> treeNodes = [];
    private readonly TreeView structureTree = new();
    private readonly TextBox htmlPreview = new();

    public MainForm()
    {
        Text = "HTMLBuilder";
        AccessibleName = Text;
        Width = 980;
        Height = 660;
        StartPosition = FormStartPosition.CenterScreen;
        Shown += (_, _) => structureTree.Focus();

        BuildMenu();
        BuildUi();
        RefreshAll();
    }

    private void BuildMenu()
    {
        var menu = new MenuStrip { Dock = DockStyle.Top };
        MainMenuStrip = menu;

        var fileMenu = new ToolStripMenuItem("&Archivo");
        fileMenu.DropDownItems.Add(MenuItem("&Nuevo documento", Keys.Control | Keys.N, OnNew));
        fileMenu.DropDownItems.Add(MenuItem("&Abrir HTML...", Keys.Control | Keys.O, OnOpenText));
        fileMenu.DropDownItems.Add(MenuItem("&Guardar documento HTML...", Keys.Control | Keys.S, OnSave));
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(MenuItem("Salir", Keys.Alt | Keys.F4, (_, _) => Close()));

        var documentMenu = new ToolStripMenuItem("&Documento");
        documentMenu.DropDownItems.Add(MenuItem("&Propiedades del documento...", Keys.Control | Keys.P, OnProperties));

        var elementMenu = new ToolStripMenuItem("&Elemento");
        elementMenu.DropDownItems.Add(MenuItem("Agregar elemento &hijo...", Keys.Control | Keys.E, OnAddChild));
        elementMenu.DropDownItems.Add(MenuItem("&Editar elemento...", Keys.F2, OnEditElement));
        elementMenu.DropDownItems.Add(MenuItem("Eliminar elemento", Keys.Delete, OnDeleteElement));
        elementMenu.DropDownItems.Add(new ToolStripSeparator());
        elementMenu.DropDownItems.Add(MenuItem("Mover hacia arriba", Keys.Control | Keys.Up, (_, _) => MoveSelected(-1)));
        elementMenu.DropDownItems.Add(MenuItem("Mover hacia abajo", Keys.Control | Keys.Down, (_, _) => MoveSelected(1)));

        var attributeMenu = new ToolStripMenuItem("&Atributos");
        attributeMenu.DropDownItems.Add(MenuItem("Agregar o modificar atributo...", Keys.Control | Keys.A, OnAddAttribute));
        attributeMenu.DropDownItems.Add(MenuItem("Quitar atributo...", Keys.Control | Keys.R, OnRemoveAttribute));

        var viewMenu = new ToolStripMenuItem("&Vista");
        viewMenu.DropDownItems.Add(MenuItem("Actualizar vista previa", Keys.F5, (_, _) => RefreshAll()));
        viewMenu.DropDownItems.Add(MenuItem("Abrir en navegador", Keys.Control | Keys.F5, OnOpenInBrowser));
        viewMenu.DropDownItems.Add(MenuItem("Copiar HTML", Keys.Control | Keys.Shift | Keys.C, OnCopyHtml));

        var helpMenu = new ToolStripMenuItem("A&yuda");
        helpMenu.DropDownItems.Add(MenuItem("Acerca de la aplicación", Keys.None, OnAbout));

        menu.Items.AddRange([fileMenu, documentMenu, elementMenu, attributeMenu, viewMenu, helpMenu]);
        Controls.Add(menu);
    }

    private void BuildUi()
    {
        structureTree.Dock = DockStyle.Fill;
        structureTree.HideSelection = false;
        structureTree.AccessibleName = "Árbol de estructura del documento";
        structureTree.NodeMouseDoubleClick += (_, _) => OnEditElement(this, EventArgs.Empty);

        htmlPreview.Dock = DockStyle.Fill;
        htmlPreview.Multiline = true;
        htmlPreview.ReadOnly = true;
        htmlPreview.WordWrap = false;
        htmlPreview.ScrollBars = ScrollBars.Both;
        htmlPreview.Font = new Font(FontFamily.GenericMonospace, 10);
        htmlPreview.AccessibleName = "Vista previa del HTML generado";

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
        if (MessageBox.Show(this, "¿Crear un documento nuevo y descartar el actual?", "Nuevo documento", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
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
            Title = "Abrir HTML",
            Filter = "HTML (*.html;*.htm)|*.html;*.htm|Todos los archivos (*.*)|*.*"
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
            MessageBox.Show(this, ex.Message, "No se pudo abrir el archivo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnSave(object? sender, EventArgs eventArgs)
    {
        using var dialog = new SaveFileDialog
        {
            Title = "Guardar HTML",
            Filter = "HTML (*.html)|*.html",
            FileName = currentPath ?? "pagina.html"
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
            MessageBox.Show(this, ex.Message, "No se pudo guardar el archivo", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

    private void OnAddChild(object? sender, EventArgs eventArgs)
    {
        var parent = SelectedNode();
        var allowedChildren = TagCatalog.TagsForParent(parent.Tag);
        if (allowedChildren.Count == 0)
        {
            MessageBox.Show(this, $"La etiqueta {parent.Tag} no puede contener elementos hijos.", "Estructura no válida", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new ElementDialog("Agregar elemento hijo", parent.Tag);
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
            MessageBox.Show(this, "Los nodos estructurales html, head y body no se editan directamente. Use Documento > Propiedades para el idioma y el título.", "Estructura del documento", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var parent = document.FindParent(node.NodeId) ?? document.Body;
        using var dialog = new ElementDialog("Editar elemento", parent.Tag, node.Tag, node.Text, node.Attributes);
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
            MessageBox.Show(this, "No se pueden eliminar los nodos estructurales html, head o body.", "Estructura del documento", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (MessageBox.Show(this, $"¿Eliminar {node.Label}?", "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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

        MessageBox.Show(this, "No se puede mover más en esa dirección.", "Mover elemento", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void OnAddAttribute(object? sender, EventArgs eventArgs)
    {
        var node = SelectedNode();
        if (TagCatalog.AttributesFor(node.Tag).Count == 0)
        {
            MessageBox.Show(this, $"La etiqueta {node.Tag} no tiene atributos editables en este editor.", "Sin atributos", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            MessageBox.Show(this, "El elemento seleccionado no tiene atributos.", "Sin atributos", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        MessageBox.Show(this, "El HTML se copió al portapapeles.", "Copiar HTML", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void OnOpenInBrowser(object? sender, EventArgs eventArgs)
    {
        try
        {
            var previewDirectory = Path.Combine(AppContext.BaseDirectory, "preview");
            Directory.CreateDirectory(previewDirectory);

            var previewPath = Path.Combine(previewDirectory, "documento-actual.html");
            File.WriteAllText(previewPath, document.ToHtml());

            Process.Start(new ProcessStartInfo
            {
                FileName = previewPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or Win32Exception)
        {
            MessageBox.Show(this, ex.Message, "No se pudo abrir la vista previa en el navegador", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnAbout(object? sender, EventArgs eventArgs)
    {
        MessageBox.Show(
            this,
            "HTMLBuilder\r\n\r\nAplicación de escritorio orientada a crear páginas HTML estructuradas, accesibles y fáciles de mantener.",
            "Acerca de la aplicación",
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
        Text = "Quitar atributo";
        AccessibleName = "Quitar atributo";
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ClientSize = new Size(360, 260);

        list.Dock = DockStyle.Fill;
        list.Items.AddRange(attributes);
        list.AccessibleName = "Atributos del elemento seleccionado";
        if (list.Items.Count > 0)
        {
            list.SelectedIndex = 0;
        }

        var okButton = new Button { Text = "Quitar atributo", DialogResult = DialogResult.OK, AutoSize = true };
        var cancelButton = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, AutoSize = true };
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
