# HTMLBuilder

C# desktop application with Windows Forms to create, review and export HTML pages without writing code manually.


## Version

`1.0.0`

## Project status

The application is ready for functional testing phase. It includes structural validations, integrated aids by tag and attribute, basic HTML import, internal preview and direct opening in browser.

## Main functions

- Tree-based visual editor to navigate the complete document structure.
- HTML generation from frequent semantic tags and controlled attributes.
- Strict validation of parent-child relationships.
- Dynamic fields by tag: only the values that actually apply are displayed.
- Empty labels without text box.
- HTML boolean attributes by presence or absence.
- Predefined values in combo box for explicit attributes like 'aria-hidden'.
- Contextual help 
with 'F1` for tags and attributes.
- Basic import of existing HTML into the internal structure of the editor.
- Read-only preview with the generated HTML.
- Direct opening of the current page in the default browser with 'Ctrl + F5'.
- Management of language and title of the document.
- Quick copy of the HTML to the clipboard.

## Keyboard shortcuts

- 'Ctrl + N': create a new document.
- 'Ctrl +O`: open an HTML file.
- 'Ctrl + S': save the current document.
- 'Ctrl + P': open document properties.
- 'Ctrl + E`: add a child element to the selected node.
- 'F2`: edit the selected element.
- 'Del`: delete the selected item.
- 'Ctrl + Up Arrow': move the selected item up.
- 'Ctrl + Down Arrow`: move the selected item down.
- 'Ctrl + A`: add or modify 
an attribute of the selected element.
- 'Ctrl + R': remove an attribute from the selected element.
- 'F5`: update the preview.
- 'Ctrl + F5`: open the current page in the default browser.
- 'Ctrl + Shift + C`: copy the generated HTML.
` 'F1`: open the contextual help of the label or attribute selector.
- 'Alt + F4': close the application.

## Distribution

The project includes two delivery formats:

- 'publish\portable`: self-contained portable version.
- 'publish\portable-framework`: portable version depending on the installed runtime.

## Feedback

You can open issues for suggestions or bug reports.

## License

This code is available for review and feedback only. It is not allowed to copy, modify, redistribute or use without written permission of the author.