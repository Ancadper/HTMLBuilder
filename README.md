# HTMLBuilder

HTMLBuilder is a C# Windows Forms desktop application for creating, reviewing, importing and exporting HTML pages without writing the code manually.

## Project status

The application is ready for functional testing. It includes structural validation, integrated help for tags and attributes, basic HTML import, internal preview and direct opening in the default browser.

## Main features

- Tree-based visual editor for navigating the full document structure.
- HTML generation using common semantic tags and controlled attributes.
- Strict validation of parent-child relationships.
- Dynamic fields by tag, showing only the values that apply to the selected element.
- Support for empty elements without unnecessary text fields.
- Support for HTML boolean attributes based on presence or absence.
- Predefined combo box values for explicit attributes such as `aria-hidden`.
- Contextual help with `F1` for tags and attributes.
- Basic import of existing HTML into the editor's internal structure.
- Read-only preview of the generated HTML.
- Direct opening of the current page in the default browser with `Ctrl + F5`.
- Management of the document language and title.
- Quick copy of the generated HTML to the clipboard.

## Keyboard shortcuts

- `Ctrl + N`: create a new document.
- `Ctrl + O`: open an HTML file.
- `Ctrl + S`: save the current document.
- `Ctrl + P`: open document properties.
- `Ctrl + E`: add a child element to the selected node.
- `F2`: edit the selected element.
- `Del`: delete the selected item.
- `Ctrl + Up Arrow`: move the selected item up.
- `Ctrl + Down Arrow`: move the selected item down.
- `Ctrl + A`: add or modify an attribute of the selected element.
- `Ctrl + R`: remove an attribute from the selected element.
- `F5`: update the preview.
- `Ctrl + F5`: open the current page in the default browser.
- `Ctrl + Shift + C`: copy the generated HTML.
- `F1`: open contextual help for the selected tag or attribute.
- `Alt + F4`: close the application.

## Distribution

Downloads are provided through the GitHub Releases section.

The project may include two delivery formats:

- Self-contained portable build: includes the required .NET runtime.
- Framework-dependent portable build: requires the appropriate .NET runtime to be installed on the user's system.

## Feedback

You can open issues for suggestions, bug reports or general feedback.

## License

This project is licensed under the Arash Attribution License.

You may use, modify, distribute, sublicense, sell, host, deploy, and include this project in
personal, commercial, or proprietary software.

Please preserve the following attribution in a reasonably accessible location:

Based on [HTMLBuilder](https://github.com/Ancadper/HTMLBuilder) by [Arash](https://Ancadper.com).