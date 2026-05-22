using HTMLBuilder.Models;

namespace HTMLBuilder.Ui;

internal enum TagContentKind
{
    Empty,
    Text,
    Children,
    TextAndChildren
}

internal sealed record TagHelp(string Description, string Accessibility, string Advice);

internal sealed record TagField(string AttributeName, string Label, bool Required, string HelpText, bool AllowsEmpty = false);

internal sealed record AttributeHelp(string Description, string Values, string Advice);

internal sealed record HtmlValidationIssue(string Message);

internal sealed record TagRule(
    string Name,
    TagContentKind ContentKind,
    IReadOnlyList<TagField> Fields,
    IReadOnlySet<string> Attributes,
    IReadOnlySet<string> AllowedParents,
    bool AllowGlobalAttributes = true);

internal sealed record AttributeRule(string Name, bool IsBoolean = false, bool AllowsEmpty = false);

internal static class TagCatalog
{
    private static readonly string[] TagOrder =
    [
        "html", "head", "body", "title", "meta", "link",
        "header", "nav", "main", "section", "article", "aside", "footer",
        "h1", "h2", "h3", "h4", "h5", "h6",
        "p", "a", "strong", "em", "small", "mark", "blockquote", "q", "cite", "code", "pre", "br", "hr",
        "ul", "ol", "li",
        "figure", "figcaption", "img", "picture", "source", "audio", "video", "track", "iframe",
        "table", "caption", "thead", "tbody", "tfoot", "tr", "th", "td",
        "form", "fieldset", "legend", "label", "input", "textarea", "select", "option", "button",
        "details", "summary", "time", "address", "div", "span"
    ];

    private static readonly string[] GlobalAttributes =
    [
        "id", "class", "title", "lang", "dir", "hidden", "tabindex",
        "role", "aria-label", "aria-labelledby", "aria-describedby", "aria-hidden"
    ];

    private static readonly Dictionary<string, AttributeRule> AttributeRules = new(StringComparer.OrdinalIgnoreCase)
    {
        ["id"] = new("id"),
        ["class"] = new("class"),
        ["title"] = new("title"),
        ["lang"] = new("lang"),
        ["dir"] = new("dir"),
        ["hidden"] = new("hidden", IsBoolean: true, AllowsEmpty: true),
        ["tabindex"] = new("tabindex"),
        ["role"] = new("role"),
        ["aria-label"] = new("aria-label"),
        ["aria-labelledby"] = new("aria-labelledby"),
        ["aria-describedby"] = new("aria-describedby"),
        ["aria-hidden"] = new("aria-hidden"),
        ["href"] = new("href"),
        ["target"] = new("target"),
        ["rel"] = new("rel"),
        ["download"] = new("download", AllowsEmpty: true),
        ["src"] = new("src"),
        ["alt"] = new("alt", AllowsEmpty: true),
        ["width"] = new("width"),
        ["height"] = new("height"),
        ["loading"] = new("loading"),
        ["controls"] = new("controls", IsBoolean: true, AllowsEmpty: true),
        ["poster"] = new("poster"),
        ["preload"] = new("preload"),
        ["type"] = new("type"),
        ["name"] = new("name"),
        ["value"] = new("value", AllowsEmpty: true),
        ["placeholder"] = new("placeholder"),
        ["required"] = new("required", IsBoolean: true, AllowsEmpty: true),
        ["readonly"] = new("readonly", IsBoolean: true, AllowsEmpty: true),
        ["disabled"] = new("disabled", IsBoolean: true, AllowsEmpty: true),
        ["checked"] = new("checked", IsBoolean: true, AllowsEmpty: true),
        ["selected"] = new("selected", IsBoolean: true, AllowsEmpty: true),
        ["multiple"] = new("multiple", IsBoolean: true, AllowsEmpty: true),
        ["min"] = new("min"),
        ["max"] = new("max"),
        ["step"] = new("step"),
        ["rows"] = new("rows"),
        ["cols"] = new("cols"),
        ["action"] = new("action"),
        ["method"] = new("method"),
        ["for"] = new("for"),
        ["colspan"] = new("colspan"),
        ["rowspan"] = new("rowspan"),
        ["scope"] = new("scope"),
        ["headers"] = new("headers"),
        ["datetime"] = new("datetime"),
        ["cite"] = new("cite"),
        ["start"] = new("start"),
        ["open"] = new("open", IsBoolean: true, AllowsEmpty: true),
        ["kind"] = new("kind"),
        ["srclang"] = new("srclang"),
        ["label"] = new("label"),
        ["charset"] = new("charset"),
        ["content"] = new("content")
    };

    private static readonly Dictionary<string, TagRule> Rules = new(StringComparer.OrdinalIgnoreCase)
    {
        ["html"] = Rule("html", TagContentKind.Children, [], ["lang"], [], allowGlobalAttributes: false),
        ["head"] = Rule("head", TagContentKind.Children, [], [], ["html"], allowGlobalAttributes: false),
        ["body"] = Rule("body", TagContentKind.Children, [], [], ["html"]),
        ["title"] = Rule("title", TagContentKind.Text, [], [], ["head"], allowGlobalAttributes: false),
        ["meta"] = Rule("meta", TagContentKind.Empty,
            [
                new("charset", "Juego de caracteres", false, "Valor habitual: utf-8."),
                new("name", "Nombre", false, "Ejemplos: viewport, description o author."),
                new("content", "Contenido", false, "Valor asociado al nombre del metadato.")
            ],
            ["charset", "name", "content"], ["head"], allowGlobalAttributes: false),
        ["link"] = Rule("link", TagContentKind.Empty,
            [
                new("rel", "Relación", true, "Ejemplos: stylesheet, preload o icon."),
                new("href", "Archivo o URL", true, "Destino del recurso enlazado.")
            ],
            ["rel", "href", "type"], ["head"], allowGlobalAttributes: false),
        ["header"] = Flow("header", ["aria-label"]),
        ["nav"] = Flow("nav", ["aria-label"]),
        ["main"] = Flow("main", ["aria-label"], ["body"]),
        ["section"] = Flow("section", ["aria-label", "aria-labelledby"]),
        ["article"] = Flow("article", ["aria-label", "aria-labelledby"]),
        ["aside"] = Flow("aside", ["aria-label", "aria-labelledby"]),
        ["footer"] = Flow("footer", ["aria-label"]),
        ["h1"] = Phrasing("h1"),
        ["h2"] = Phrasing("h2"),
        ["h3"] = Phrasing("h3"),
        ["h4"] = Phrasing("h4"),
        ["h5"] = Phrasing("h5"),
        ["h6"] = Phrasing("h6"),
        ["p"] = Phrasing("p"),
        ["a"] = Rule("a", TagContentKind.TextAndChildren,
            [new("href", "Enlace o URL", true, "Destino del enlace. Ejemplo: https://ejemplo.com o contacto.html.")],
            ["href", "target", "rel", "download"]),
        ["strong"] = Phrasing("strong"),
        ["em"] = Phrasing("em"),
        ["small"] = Phrasing("small"),
        ["mark"] = Phrasing("mark"),
        ["blockquote"] = Rule("blockquote", TagContentKind.TextAndChildren,
            [new("cite", "Fuente de la cita", false, "URL de la fuente original de la cita.")],
            ["cite"]),
        ["q"] = Rule("q", TagContentKind.Text,
            [new("cite", "Fuente de la cita", false, "URL de la fuente original de la cita.")],
            ["cite"]),
        ["cite"] = Phrasing("cite"),
        ["code"] = Phrasing("code"),
        ["pre"] = Phrasing("pre"),
        ["br"] = Rule("br", TagContentKind.Empty, [], []),
        ["hr"] = Rule("hr", TagContentKind.Empty, [], []),
        ["ul"] = Rule("ul", TagContentKind.Children, [], []),
        ["ol"] = Rule("ol", TagContentKind.Children,
            [new("start", "Número inicial", false, "Primer número de la lista si no empieza en 1.")],
            ["start"]),
        ["li"] = Rule("li", TagContentKind.TextAndChildren, [], [], ["ul", "ol"]),
        ["figure"] = Rule("figure", TagContentKind.Children, [], []),
        ["figcaption"] = Rule("figcaption", TagContentKind.TextAndChildren, [], [], ["figure"]),
        ["img"] = Rule("img", TagContentKind.Empty,
            [
                new("src", "Archivo o URL de la imagen", true, "Ruta de la imagen."),
                new("alt", "Texto alternativo", true, "Describe la información útil de la imagen. Puede quedar vacío si es decorativa.", AllowsEmpty: true)
            ],
            ["src", "alt", "width", "height", "loading"]),
        ["picture"] = Rule("picture", TagContentKind.Children, [], []),
        ["source"] = Rule("source", TagContentKind.Empty,
            [
                new("src", "Archivo multimedia", true, "Ruta del recurso."),
                new("type", "Tipo MIME", false, "Ejemplo: video/mp4, audio/mpeg o image/webp.")
            ],
            ["src", "type"], ["picture", "audio", "video"]),
        ["audio"] = Rule("audio", TagContentKind.Children,
            [new("src", "Archivo de audio", false, "Puede usar src o añadir elementos source como hijos.")],
            ["src", "controls", "preload"]),
        ["video"] = Rule("video", TagContentKind.Children,
            [
                new("src", "Archivo de video", false, "Puede usar src o añadir elementos source como hijos."),
                new("poster", "Imagen de portada", false, "Imagen mostrada antes de reproducir.")
            ],
            ["src", "controls", "poster", "preload", "width", "height"]),
        ["track"] = Rule("track", TagContentKind.Empty,
            [
                new("src", "Archivo de subtítulos", true, "Archivo WebVTT."),
                new("kind", "Tipo de pista", false, "Valores: subtitles, captions, descriptions, chapters o metadata."),
                new("srclang", "Idioma", false, "Código de idioma de la pista."),
                new("label", "Etiqueta visible", false, "Nombre que verá la persona usuaria.")
            ],
            ["src", "kind", "srclang", "label"], ["audio", "video"]),
        ["iframe"] = Rule("iframe", TagContentKind.Empty,
            [
                new("src", "URL incrustada", true, "Dirección del contenido embebido."),
                new("title", "Título accesible", true, "Describe el contenido del marco para lectores de pantalla.")
            ],
            ["src", "title", "width", "height", "loading"]),
        ["table"] = Rule("table", TagContentKind.Children, [], []),
        ["caption"] = Rule("caption", TagContentKind.Text, [], [], ["table"]),
        ["thead"] = Rule("thead", TagContentKind.Children, [], [], ["table"]),
        ["tbody"] = Rule("tbody", TagContentKind.Children, [], [], ["table"]),
        ["tfoot"] = Rule("tfoot", TagContentKind.Children, [], [], ["table"]),
        ["tr"] = Rule("tr", TagContentKind.Children, [], [], ["table", "thead", "tbody", "tfoot"]),
        ["th"] = Rule("th", TagContentKind.TextAndChildren,
            [new("scope", "Ámbito del encabezado", false, "Valores: col, row, colgroup o rowgroup.")],
            ["scope", "colspan", "rowspan", "headers"], ["tr"]),
        ["td"] = Rule("td", TagContentKind.TextAndChildren,
            [new("headers", "Encabezados asociados", false, "IDs de celdas th que describen esta celda.")],
            ["headers", "colspan", "rowspan"], ["tr"]),
        ["form"] = Rule("form", TagContentKind.Children,
            [
                new("action", "Destino del formulario", false, "URL que recibirá los datos del formulario."),
                new("method", "Método", false, "Valores: get o post.")
            ],
            ["action", "method"]),
        ["fieldset"] = Rule("fieldset", TagContentKind.Children, [], []),
        ["legend"] = Rule("legend", TagContentKind.Text, [], [], ["fieldset"]),
        ["label"] = Rule("label", TagContentKind.TextAndChildren,
            [new("for", "ID del control asociado", false, "Debe coincidir con el id del input, select o textarea.")],
            ["for"]),
        ["input"] = Rule("input", TagContentKind.Empty,
            [
                new("type", "Tipo de campo", true, "Ejemplos: text, email, checkbox, radio o submit."),
                new("name", "Nombre del campo", false, "Nombre enviado con el formulario."),
                new("aria-label", "Nombre accesible", false, "Úselo si no hay una etiqueta label asociada.")
            ],
            ["type", "name", "value", "placeholder", "required", "readonly", "disabled", "checked", "multiple", "min", "max", "step"]),
        ["textarea"] = Rule("textarea", TagContentKind.Text,
            [
                new("name", "Nombre del campo", false, "Nombre enviado con el formulario."),
                new("aria-label", "Nombre accesible", false, "Úselo si no hay una etiqueta label asociada.")
            ],
            ["name", "placeholder", "required", "readonly", "disabled", "rows", "cols"]),
        ["select"] = Rule("select", TagContentKind.Children, [], ["name", "required", "disabled", "multiple"]),
        ["option"] = Rule("option", TagContentKind.Text,
            [new("value", "Valor", false, "Valor enviado si se selecciona esta opción.", AllowsEmpty: true)],
            ["value", "selected", "disabled", "label"], ["select"]),
        ["button"] = Rule("button", TagContentKind.TextAndChildren,
            [new("type", "Tipo de botón", true, "Valores recomendados: button, submit o reset.")],
            ["type", "name", "value", "disabled", "aria-label"]),
        ["details"] = Rule("details", TagContentKind.Children, [], ["open"]),
        ["summary"] = Rule("summary", TagContentKind.TextAndChildren, [], [], ["details"]),
        ["time"] = Rule("time", TagContentKind.Text,
            [new("datetime", "Fecha u hora legible por máquina", false, "Versión ISO de la fecha u hora visible.")],
            ["datetime"]),
        ["address"] = Phrasing("address"),
        ["div"] = Flow("div"),
        ["span"] = Phrasing("span")
    };

    public static readonly string[] CommonTags = TagOrder;

    public static readonly string[] CommonAttributes =
        AttributeRules.Keys.Order(StringComparer.OrdinalIgnoreCase).ToArray();

    private static readonly HashSet<string> StructuralTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "html", "head", "body"
    };

    private static readonly HashSet<string> FlowTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "header", "nav", "main", "section", "article", "aside", "footer", "address", "div",
        "p", "h1", "h2", "h3", "h4", "h5", "h6", "blockquote", "ul", "ol", "li",
        "figure", "figcaption", "picture", "img", "audio", "video", "iframe", "table",
        "form", "fieldset", "details", "hr", "pre", "span", "a", "strong", "em", "small",
        "mark", "cite", "code", "q", "br", "label", "input", "textarea", "select",
        "button", "time"
    };

    private static readonly HashSet<string> PhrasingTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "span", "a", "strong", "em", "small", "mark", "cite", "code", "q", "br",
        "img", "label", "input", "textarea", "select", "button", "time"
    };

    private static readonly HashSet<string> PhrasingParents = new(StringComparer.OrdinalIgnoreCase)
    {
        "p", "h1", "h2", "h3", "h4", "h5", "h6", "span", "strong", "em", "small",
        "mark", "cite", "code", "pre", "figcaption", "th", "summary"
    };

    private static readonly HashSet<string> AnchorChildren = new(StringComparer.OrdinalIgnoreCase)
    {
        "span", "strong", "em", "small", "mark", "cite", "code", "q", "br", "img", "time"
    };

    private static readonly HashSet<string> ButtonChildren = new(StringComparer.OrdinalIgnoreCase)
    {
        "span", "strong", "em", "small", "mark", "cite", "code", "q", "br", "img", "time"
    };

    private static readonly HashSet<string> AddressChildren = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "br", "span", "strong", "em", "small", "mark", "cite", "code", "q", "img", "time"
    };

    private static readonly Dictionary<string, TagHelp> TagHelp = new(StringComparer.OrdinalIgnoreCase)
    {
        ["html"] = Help("Elemento raíz del documento.", "Suele llevar lang para indicar el idioma principal.", "Normalmente contiene head y body como hijos directos."),
        ["head"] = Help("Contenedor de metadatos del documento.", "Suele incluir title, meta y link.", "Úselo para configurar la página, no para contenido visible."),
        ["body"] = Help("Contenedor del contenido visible.", "Puede reunir encabezados, secciones, formularios, tablas y otros bloques.", "Todo lo que la persona usuaria ve normalmente vive dentro de body."),
        ["title"] = Help("Título del documento.", "Va dentro de head y ayuda a navegadores y lectores de pantalla.", "Manténgalo breve, claro y descriptivo."),
        ["meta"] = Help("Metadato del documento.", "Puede usar charset o la pareja name y content.", "Es útil para viewport, descripción, autor y codificación."),
        ["link"] = Help("Vincula recursos externos.", "Suele usar rel y href.", "Se usa habitualmente para hojas de estilo, iconos o precargas."),
        ["header"] = Help("Cabecera de página o sección.", "Puede usar aria-label si hay varias cabeceras.", "Agrupa título, logotipo, búsqueda o navegación inicial."),
        ["nav"] = Help("Bloque de navegación.", "Use aria-label si hay varias zonas de navegación.", "Nombre la navegación: principal, secundaria, pie, etc."),
        ["main"] = Help("Contenido principal del documento.", "Debe existir solo un main visible por página.", "Úselo para el contenido central, no para menús ni pie."),
        ["section"] = Help("Sección temática.", "Conviene acompañarla de encabezado visible o aria-label.", "Úsela cuando el grupo tenga un tema claro."),
        ["article"] = Help("Contenido independiente o reutilizable.", "Puede usar aria-labelledby si tiene título interno.", "Es útil para noticias, entradas o tarjetas completas."),
        ["aside"] = Help("Contenido complementario.", "Acepta aria-label si conviene explicar su propósito.", "Úselo para notas, ayudas o enlaces relacionados."),
        ["footer"] = Help("Pie de página o sección.", "Puede usar aria-label si hay varios pies.", "Úselo para información legal, contacto o enlaces finales."),
        ["h1"] = Help("Encabezado principal.", "Define el título o sección de mayor jerarquía.", "Use h1 para la idea principal del documento o bloque."),
        ["h2"] = Help("Encabezado de segundo nivel.", "Organiza secciones bajo un h1.", "Mantenga una jerarquía lógica con los demás encabezados."),
        ["h3"] = Help("Encabezado de tercer nivel.", "Subdivide una sección encabezada por h2.", "Úselo cuando una subsección necesite un título propio."),
        ["h4"] = Help("Encabezado de cuarto nivel.", "Extiende la jerarquía de títulos.", "Evite saltos bruscos en el nivel de encabezado."),
        ["h5"] = Help("Encabezado de quinto nivel.", "Representa un subnivel muy específico.", "Úselo solo cuando la estructura realmente lo necesite."),
        ["h6"] = Help("Encabezado de sexto nivel.", "Es el último nivel de jerarquía estándar.", "Reserve este nivel para estructuras muy detalladas."),
        ["p"] = Help("Párrafo de texto.", "Puede contener texto y elementos semánticos en línea.", "Úselo para bloques de texto corrido."),
        ["a"] = Help("Enlace hipertextual.", "Necesita href. El texto visible debe describir el destino.", "Evite textos genéricos como 'clic aquí'."),
        ["strong"] = Help("Énfasis fuerte.", "Sirve para resaltar importancia semántica.", "Úselo cuando el contenido tenga relevancia especial."),
        ["em"] = Help("Énfasis de voz o intención.", "Marca una palabra o fragmento con intención expresiva.", "Prefiera este elemento al formato visual puro."),
        ["small"] = Help("Texto secundario o nota.", "Sirve para aclaraciones, avisos o letra menor.", "Úselo para contenido de menor jerarquía."),
        ["mark"] = Help("Texto resaltado por relevancia.", "Marca una parte importante dentro de un texto.", "Úselo para coincidencias, resultados o ideas clave."),
        ["blockquote"] = Help("Cita en bloque.", "Representa contenido citado de otra fuente.", "Acompáñelo con cite si conoce el origen."),
        ["q"] = Help("Cita breve en línea.", "Se usa para citas cortas dentro de un párrafo.", "Reserve blockquote para citas extensas."),
        ["cite"] = Help("Referencia a una obra o fuente.", "Suele acompañar a citas, libros o documentos.", "Úselo para atribuir correctamente una fuente."),
        ["code"] = Help("Fragmento de código o texto técnico.", "Marca contenido que debe leerse literalmente.", "Evite mezclarlo con párrafos largos si no es necesario."),
        ["pre"] = Help("Texto preformateado.", "Conserva espacios y saltos de línea.", "Úselo cuando el formato original importe."),
        ["br"] = Help("Salto de línea.", "No lleva contenido ni etiqueta de cierre.", "Úselo solo cuando el salto tenga significado real."),
        ["hr"] = Help("Separador temático.", "No lleva contenido ni etiqueta de cierre.", "Úselo para separar ideas o apartados."),
        ["ul"] = Help("Lista no ordenada.", "Contiene únicamente elementos li.", "Úsela para listas sin orden jerárquico."),
        ["ol"] = Help("Lista ordenada.", "Contiene únicamente elementos li.", "Úsela cuando el orden de los elementos importe."),
        ["li"] = Help("Elemento de lista.", "Debe vivir dentro de ul u ol.", "Use un li por cada punto de la lista."),
        ["figure"] = Help("Figura o recurso autocontenido.", "Suele acompañarse de figcaption.", "Úsela para imágenes, fragmentos o recursos ilustrativos."),
        ["figcaption"] = Help("Pie o descripción de una figura.", "Va dentro de figure.", "Resume o contextualiza el contenido de la figura."),
        ["img"] = Help("Inserta una imagen.", "Necesita src y alt. Si es decorativa, alt puede quedar vacío.", "Describa la información útil, no solo la apariencia."),
        ["picture"] = Help("Imagen adaptable.", "Agrupa source e img para servir variantes según el contexto.", "Úselo cuando necesite formatos o resoluciones alternativas."),
        ["source"] = Help("Fuente alternativa de un recurso multimedia.", "Se usa dentro de picture, audio o video.", "Acompáñelo con el tipo adecuado cuando convenga."),
        ["audio"] = Help("Reproductor de audio.", "Puede usar src o varios source.", "Añada controls si la persona usuaria debe manejar la reproducción."),
        ["video"] = Help("Reproductor de video.", "Puede usar src, source y poster.", "Use poster para mostrar una imagen previa útil."),
        ["track"] = Help("Pista de texto para multimedia.", "Sirve para subtítulos, descripciones o capítulos.", "Combine kind, srclang y label según el caso."),
        ["iframe"] = Help("Inserta contenido externo.", "Necesita src y title.", "El title debe explicar qué se muestra dentro del marco."),
        ["table"] = Help("Tabla de datos.", "Conviene usar caption, th, scope y headers cuando corresponda.", "No la use solo para maquetación."),
        ["caption"] = Help("Título o descripción breve de una tabla.", "Va dentro de table y explica su propósito.", "Manténgalo breve y claro."),
        ["thead"] = Help("Cabecera de tabla.", "Agrupa las filas de encabezado.", "Úselo para mejorar la lectura de tablas complejas."),
        ["tbody"] = Help("Cuerpo de tabla.", "Agrupa las filas principales de datos.", "Sirve para separar los datos del encabezado y el pie."),
        ["tfoot"] = Help("Pie de tabla.", "Agrupa filas de resumen o totales.", "Úselo para remates o sumarios de datos."),
        ["tr"] = Help("Fila de tabla.", "Contiene celdas th o td.", "Úselo para estructurar cada fila de datos."),
        ["th"] = Help("Celda de encabezado de tabla.", "Use scope para indicar si encabeza fila o columna.", "Ayuda mucho a lectores de pantalla."),
        ["td"] = Help("Celda de datos de tabla.", "Puede usar headers en tablas complejas.", "Úsela dentro de tr."),
        ["form"] = Help("Formulario.", "Suele usar action y method.", "Agrupa controles relacionados con un envío de datos."),
        ["fieldset"] = Help("Grupo de controles de formulario.", "Suele acompañarse de legend.", "Úselo para organizar formularios complejos."),
        ["legend"] = Help("Título del grupo de controles.", "Va dentro de fieldset.", "Debe describir con claridad el conjunto de campos."),
        ["label"] = Help("Etiqueta visible de un control.", "Use for para asociarla al id del control.", "Es preferible a aria-label cuando hay texto visible."),
        ["input"] = Help("Control de formulario.", "Debe tener nombre accesible mediante label, aria-label o aria-labelledby.", "Asocie siempre un label cuando sea posible."),
        ["textarea"] = Help("Área de texto multilínea.", "Puede usar name, rows, cols y atributos de accesibilidad.", "No sustituye a un label."),
        ["select"] = Help("Lista desplegable.", "Contiene option y puede ser múltiple.", "Acompáñelo con label para mejorar la accesibilidad."),
        ["option"] = Help("Opción de una lista.", "Va dentro de select.", "Su value debe reflejar el dato enviado."),
        ["button"] = Help("Botón de acción.", "Debe tener texto visible o un nombre accesible.", "Use a para navegación y button para acciones."),
        ["details"] = Help("Panel desplegable.", "Puede usar open para mostrarse expandido por defecto.", "Sirve para contenido que no necesita estar siempre visible."),
        ["summary"] = Help("Encabezado del panel desplegable.", "Va dentro de details.", "Debe ser breve y describir claramente el contenido."),
        ["time"] = Help("Fecha u hora reconocible por máquina.", "Puede usar datetime con formato legible para software.", "Úselo cuando el texto represente una fecha u hora concreta."),
        ["address"] = Help("Información de contacto.", "Se usa para datos de autor, organización o contacto relacionado.", "Úselo para información de contacto semántica."),
        ["div"] = Help("Agrupa contenido sin significado semántico propio.", "Puede usar role o aria-label solo si no hay una etiqueta semántica mejor.", "Úselo como último recurso genérico."),
        ["span"] = Help("Agrupa texto en línea sin significado semántico propio.", "Normalmente no necesita ARIA.", "Úselo para fragmentos pequeños de texto.")
    };

    private static readonly Dictionary<string, AttributeHelp> AttributeHelp = new(StringComparer.OrdinalIgnoreCase)
    {
        ["id"] = Attribute("Identificador único del elemento.", "Texto sin espacios. Ejemplo: titulo-principal.", "Útil para enlazar etiquetas, encabezados o estilos."),
        ["class"] = Attribute("Clase o grupo del elemento.", "Uno o varios nombres separados por espacios.", "Útil para aplicar estilos CSS."),
        ["title"] = Attribute("Texto adicional de ayuda.", "Texto libre.", "No lo use como única información importante; muchas personas no lo perciben."),
        ["lang"] = Attribute("Idioma del contenido.", "Códigos como es, en, fr o es-MX.", "Úselo cuando una parte esté en otro idioma."),
        ["dir"] = Attribute("Dirección del texto.", "ltr, rtl o auto.", "auto suele ser buena opción si no conoce la dirección."),
        ["hidden"] = Attribute("Oculta el elemento.", "Atributo booleano: no necesita valor.", "El contenido oculto tampoco estará disponible normalmente para lectores de pantalla."),
        ["tabindex"] = Attribute("Controla el orden de foco.", "0, -1 o un número positivo.", "Evite números positivos; use 0 o -1."),
        ["role"] = Attribute("Rol ARIA del elemento.", "button, navigation, region, alert, dialog, tab, tabpanel y otros.", "Use HTML semántico antes de recurrir a role."),
        ["aria-label"] = Attribute("Nombre accesible escrito a mano.", "Texto libre.", "Úselo si no hay texto visible que nombre el control."),
        ["aria-labelledby"] = Attribute("Nombre accesible tomado de otro elemento.", "Uno o varios id separados por espacios.", "Suele ser mejor que aria-label si ya existe un título visible."),
        ["aria-describedby"] = Attribute("Descripción accesible adicional.", "Uno o varios id separados por espacios.", "Útil para instrucciones o mensajes de ayuda."),
        ["aria-hidden"] = Attribute("Oculta el elemento a lectores de pantalla.", "true o false.", "No lo use en contenido importante o enfocable."),
        ["href"] = Attribute("Destino de un enlace.", "URL, ruta relativa, #id, mailto: o tel:.", "El texto del enlace debe explicar el destino."),
        ["target"] = Attribute("Dónde se abre el enlace.", "_self, _blank, _parent o _top.", "Si usa _blank, conviene rel=\"noopener\"."),
        ["rel"] = Attribute("Relación del enlace con el destino.", "noopener, noreferrer, nofollow, external, author, prev o next.", "Con target=\"_blank\" use noopener."),
        ["download"] = Attribute("Indica descarga en lugar de navegación.", "Vacío o nombre de archivo.", "Úselo solo si el enlace descarga un recurso."),
        ["src"] = Attribute("Ruta de un recurso.", "URL o ruta relativa.", "Se usa en img, audio, video, iframe, source y track."),
        ["alt"] = Attribute("Texto alternativo de una imagen.", "Texto libre o vacío si es decorativa.", "Describa la información, no escriba solo 'imagen de'."),
        ["width"] = Attribute("Ancho del recurso.", "Número en píxeles.", "Útil para imágenes o marcos para evitar saltos visuales."),
        ["height"] = Attribute("Alto del recurso.", "Número en píxeles.", "Útil para imágenes o marcos para evitar saltos visuales."),
        ["loading"] = Attribute("Carga diferida de imágenes o iframes.", "lazy o eager.", "lazy ayuda en recursos fuera de la primera vista."),
        ["controls"] = Attribute("Muestra controles nativos multimedia.", "Atributo booleano: no necesita valor.", "Recomendado en audio y video."),
        ["poster"] = Attribute("Imagen previa de un video.", "URL o ruta relativa.", "Ayuda antes de reproducir el video."),
        ["preload"] = Attribute("Sugerencia de precarga multimedia.", "none, metadata o auto.", "metadata suele ser una opción prudente."),
        ["type"] = Attribute("Tipo del control o recurso.", "button, submit, reset, text, email, checkbox, radio, video/mp4 y otros.", "Depende de la etiqueta."),
        ["name"] = Attribute("Nombre enviado por formularios.", "Texto simple, preferiblemente sin espacios.", "Útil en input, select y textarea."),
        ["content"] = Attribute("Contenido asociado a un metadato.", "Texto libre.", "Suele acompañar a name en elementos meta."),
        ["value"] = Attribute("Valor enviado o asociado.", "Texto libre.", "En botones y opciones define el valor enviado."),
        ["placeholder"] = Attribute("Ejemplo dentro de un campo.", "Texto libre.", "No sustituye a label."),
        ["required"] = Attribute("Campo obligatorio.", "Atributo booleano: no necesita valor.", "Debe acompañarse de instrucciones claras."),
        ["readonly"] = Attribute("Campo de solo lectura.", "Atributo booleano: no necesita valor.", "La persona usuaria puede leerlo pero no cambiarlo."),
        ["disabled"] = Attribute("Control desactivado.", "Atributo booleano: no necesita valor.", "Los controles desactivados pueden ser difíciles de percibir."),
        ["checked"] = Attribute("Marca un checkbox o radio.", "Atributo booleano: no necesita valor.", "Solo para input de tipo checkbox o radio."),
        ["selected"] = Attribute("Selecciona una opción.", "Atributo booleano: no necesita valor.", "Solo para option."),
        ["multiple"] = Attribute("Permite múltiples valores.", "Atributo booleano: no necesita valor.", "Se usa en select o input de archivo."),
        ["min"] = Attribute("Valor mínimo.", "Número o fecha según el tipo.", "Útil en campos numéricos o de fecha."),
        ["max"] = Attribute("Valor máximo.", "Número o fecha según el tipo.", "Útil en campos numéricos o de fecha."),
        ["step"] = Attribute("Incremento permitido.", "Número o any.", "Útil en number, range y tiempo."),
        ["rows"] = Attribute("Filas visibles.", "Número entero.", "Usado en textarea."),
        ["cols"] = Attribute("Columnas visibles.", "Número entero.", "Usado en textarea."),
        ["action"] = Attribute("Destino del formulario.", "URL o ruta relativa.", "Usado en form."),
        ["method"] = Attribute("Método de envío del formulario.", "get o post.", "get para búsquedas; post para enviar datos."),
        ["for"] = Attribute("Asocia label con un control.", "Id del control.", "Debe coincidir con el id de input, select o textarea."),
        ["colspan"] = Attribute("Número de columnas que ocupa una celda.", "Número entero.", "Usado en th o td."),
        ["rowspan"] = Attribute("Número de filas que ocupa una celda.", "Número entero.", "Usado en th o td."),
        ["scope"] = Attribute("Ámbito de un encabezado de tabla.", "col, row, colgroup o rowgroup.", "Usado en th."),
        ["headers"] = Attribute("Encabezados asociados a una celda.", "Id separados por espacios.", "Útil en tablas complejas."),
        ["datetime"] = Attribute("Fecha u hora legible por máquina.", "Formato ISO. Ejemplo: 2026-05-10.", "Usado en time."),
        ["cite"] = Attribute("Fuente de una cita.", "URL.", "Usado en blockquote o q."),
        ["start"] = Attribute("Número inicial de una lista ordenada.", "Número entero.", "Usado en ol."),
        ["charset"] = Attribute("Juego de caracteres del documento.", "Valor habitual: utf-8.", "Suele usarse en meta dentro de head."),
        ["open"] = Attribute("Abre details por defecto.", "Atributo booleano: no necesita valor.", "Usado en details."),
        ["kind"] = Attribute("Tipo de pista de texto.", "subtitles, captions, descriptions, chapters o metadata.", "Usado en track."),
        ["srclang"] = Attribute("Idioma de una pista de texto.", "Código como es o en.", "Usado en track."),
        ["label"] = Attribute("Etiqueta visible de una opción o pista.", "Texto libre.", "Usado en option o track.")
    };

    private static readonly Dictionary<string, string[]> PresetAttributeValues = new(StringComparer.OrdinalIgnoreCase)
    {
        ["aria-hidden"] = ["true", "false"]
    };

    private static readonly Dictionary<string, TagHelp> EnglishTagHelp = new(StringComparer.OrdinalIgnoreCase)
    {
        ["html"] = Help("Root element of the document.", "Usually carries lang to identify the main language.", "It normally contains head and body as direct children."),
        ["head"] = Help("Container for document metadata.", "Usually includes title, meta, and link.", "Use it to configure the page, not for visible content."),
        ["body"] = Help("Container for visible page content.", "Can contain headings, sections, forms, tables, and other blocks.", "Most content the user sees belongs inside body."),
        ["title"] = Help("Document title.", "Used by browsers and assistive technology.", "Keep it short, clear, and descriptive."),
        ["meta"] = Help("Document metadata.", "Use charset, or name together with content.", "Useful for viewport, description, author, and encoding data."),
        ["link"] = Help("Links external resources.", "Usually uses rel and href.", "Commonly used for stylesheets, icons, and preloads."),
        ["a"] = Help("Hyperlink.", "Requires href; visible text should explain the destination.", "Avoid generic text such as 'click here'."),
        ["img"] = Help("Embeds an image.", "Requires src and alt; alt can be empty for decorative images.", "Describe the useful information, not just the appearance."),
        ["form"] = Help("Form for collecting data.", "Controls need accessible names through labels or ARIA.", "Group related inputs and make submission intent clear."),
        ["input"] = Help("Form control.", "Should have an accessible name from label, aria-label, or aria-labelledby.", "Associate a label whenever possible."),
        ["button"] = Help("Action button.", "Needs visible text or an accessible name.", "Use links for navigation and buttons for actions."),
        ["table"] = Help("Data table.", "Use caption, th, scope, and headers when appropriate.", "Do not use tables only for layout."),
        ["main"] = Help("Main document content.", "There should be only one visible main per page.", "Use it for central content, not navigation or footer material."),
        ["nav"] = Help("Navigation block.", "Use aria-label when several navigation areas exist.", "Name the navigation area, such as primary, secondary, or footer."),
        ["section"] = Help("Thematic section.", "Works best with a visible heading or aria-label.", "Use it when the group has a clear topic."),
        ["article"] = Help("Independent or reusable content.", "Can use aria-labelledby when it has an internal title.", "Useful for news items, posts, or complete cards."),
        ["div"] = Help("Generic block container.", "Can use role or aria-label when no semantic tag fits.", "Use it as a last-resort grouping element."),
        ["span"] = Help("Generic inline text container.", "Usually does not need ARIA.", "Use it for small inline text fragments.")
    };

    private static readonly Dictionary<string, AttributeHelp> EnglishAttributeHelp = new(StringComparer.OrdinalIgnoreCase)
    {
        ["id"] = Attribute("Unique element identifier.", "Text without spaces, for example main-title.", "Useful for labels, headings, anchors, and styles."),
        ["class"] = Attribute("Class or group for the element.", "One or more names separated by spaces.", "Useful for applying CSS styles."),
        ["title"] = Attribute("Additional advisory text.", "Free text.", "Do not use it as the only important information."),
        ["lang"] = Attribute("Language of the content.", "Codes such as en, es, fr, or es-MX.", "Use it when a section is in another language."),
        ["href"] = Attribute("Destination of a link.", "URL, relative path, #id, mailto:, or tel:.", "The link text should explain the destination."),
        ["src"] = Attribute("Path to a resource.", "URL or relative path.", "Used by img, audio, video, iframe, source, and track."),
        ["alt"] = Attribute("Alternative text for an image.", "Free text or empty when decorative.", "Describe the information; do not only write 'image of'."),
        ["aria-label"] = Attribute("Manually written accessible name.", "Free text.", "Use it when no visible text names the control."),
        ["aria-labelledby"] = Attribute("Accessible name taken from another element.", "One or more ids separated by spaces.", "Often better than aria-label when a visible heading exists."),
        ["aria-describedby"] = Attribute("Additional accessible description.", "One or more ids separated by spaces.", "Useful for instructions and help messages."),
        ["role"] = Attribute("ARIA role for the element.", "button, navigation, region, alert, dialog, tab, tabpanel, and others.", "Prefer semantic HTML before adding a role."),
        ["type"] = Attribute("Control or resource type.", "button, submit, reset, text, email, checkbox, video/mp4, and others.", "Its meaning depends on the tag."),
        ["name"] = Attribute("Name submitted by forms or metadata.", "Simple text, preferably without spaces.", "Useful in form controls and meta elements."),
        ["value"] = Attribute("Submitted or associated value.", "Free text.", "For buttons and options, it defines the submitted value."),
        ["required"] = Attribute("Marks a form control as required.", "Boolean attribute: no value needed.", "Pair it with clear instructions."),
        ["disabled"] = Attribute("Disables a control.", "Boolean attribute: no value needed.", "Disabled controls can be harder to perceive."),
        ["hidden"] = Attribute("Hides an element.", "Boolean attribute: no value needed.", "Hidden content is normally unavailable to screen readers too."),
        ["charset"] = Attribute("Document character set.", "Common value: utf-8.", "Usually used on meta inside head."),
        ["content"] = Attribute("Content associated with metadata.", "Free text.", "Usually paired with name in meta elements.")
    };

    public static IReadOnlyList<TagField> FieldsFor(string tag) =>
        GetRule(tag).Fields
            .Select(field => field with
            {
                Label = Localizer.FieldLabel(field.Label),
                HelpText = Localizer.FieldHelp(field.HelpText)
            })
            .ToArray();

    public static TagContentKind ContentKindFor(string tag) =>
        GetRule(tag).ContentKind;

    public static bool IsVoidTag(string tag) =>
        ContentKindFor(tag) == TagContentKind.Empty;

    public static bool CanHaveText(string tag)
    {
        var contentKind = ContentKindFor(tag);
        return contentKind is TagContentKind.Text or TagContentKind.TextAndChildren;
    }

    public static bool CanHaveChildren(string tag)
    {
        var contentKind = ContentKindFor(tag);
        return contentKind is TagContentKind.Children or TagContentKind.TextAndChildren;
    }

    public static bool IsStructuralTag(string tag) =>
        StructuralTags.Contains(tag);

    public static IReadOnlyList<string> TagsForParent(string parentTag)
    {
        if (!CanHaveChildren(parentTag))
        {
            return [];
        }

        return TagOrder
            .Where(tag => !IsStructuralTag(tag) && CanAddChild(parentTag, tag))
            .ToArray();
    }

    public static bool CanAddChild(string parentTag, string childTag)
    {
        if (!CanHaveChildren(parentTag))
        {
            return false;
        }

        var childRule = GetRule(childTag);
        if (childRule.AllowedParents.Count > 0)
        {
            return childRule.AllowedParents.Contains(parentTag);
        }

        if (string.Equals(parentTag, "head", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(childTag, "title", StringComparison.OrdinalIgnoreCase)
                || string.Equals(childTag, "meta", StringComparison.OrdinalIgnoreCase)
                || string.Equals(childTag, "link", StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(parentTag, "body", StringComparison.OrdinalIgnoreCase))
        {
            return FlowTags.Contains(childTag);
        }

        if (string.Equals(parentTag, "ul", StringComparison.OrdinalIgnoreCase)
            || string.Equals(parentTag, "ol", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(childTag, "li", StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(parentTag, "select", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(childTag, "option", StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(parentTag, "table", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(childTag, "caption", StringComparison.OrdinalIgnoreCase)
                || string.Equals(childTag, "thead", StringComparison.OrdinalIgnoreCase)
                || string.Equals(childTag, "tbody", StringComparison.OrdinalIgnoreCase)
                || string.Equals(childTag, "tfoot", StringComparison.OrdinalIgnoreCase)
                || string.Equals(childTag, "tr", StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(parentTag, "picture", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(childTag, "source", StringComparison.OrdinalIgnoreCase)
                || string.Equals(childTag, "img", StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(parentTag, "audio", StringComparison.OrdinalIgnoreCase)
            || string.Equals(parentTag, "video", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(childTag, "source", StringComparison.OrdinalIgnoreCase)
                || string.Equals(childTag, "track", StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(parentTag, "thead", StringComparison.OrdinalIgnoreCase)
            || string.Equals(parentTag, "tbody", StringComparison.OrdinalIgnoreCase)
            || string.Equals(parentTag, "tfoot", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(childTag, "tr", StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(parentTag, "tr", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(childTag, "th", StringComparison.OrdinalIgnoreCase)
                || string.Equals(childTag, "td", StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(parentTag, "figure", StringComparison.OrdinalIgnoreCase))
        {
            return FlowTags.Contains(childTag)
                || string.Equals(childTag, "figcaption", StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(parentTag, "details", StringComparison.OrdinalIgnoreCase))
        {
            return FlowTags.Contains(childTag)
                || string.Equals(childTag, "summary", StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(parentTag, "fieldset", StringComparison.OrdinalIgnoreCase))
        {
            return FlowTags.Contains(childTag)
                || string.Equals(childTag, "legend", StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(parentTag, "form", StringComparison.OrdinalIgnoreCase))
        {
            return FlowTags.Contains(childTag)
                && !string.Equals(childTag, "form", StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(parentTag, "label", StringComparison.OrdinalIgnoreCase))
        {
            return PhrasingTags.Contains(childTag)
                && !string.Equals(childTag, "label", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(childTag, "button", StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(parentTag, "a", StringComparison.OrdinalIgnoreCase))
        {
            return AnchorChildren.Contains(childTag);
        }

        if (string.Equals(parentTag, "button", StringComparison.OrdinalIgnoreCase))
        {
            return ButtonChildren.Contains(childTag);
        }

        if (string.Equals(parentTag, "address", StringComparison.OrdinalIgnoreCase))
        {
            return AddressChildren.Contains(childTag);
        }

        if (PhrasingParents.Contains(parentTag))
        {
            return PhrasingTags.Contains(childTag);
        }

        return FlowTags.Contains(childTag)
            && !string.Equals(childTag, "main", StringComparison.OrdinalIgnoreCase);
    }

    public static IReadOnlyList<string> AttributesFor(string tag)
    {
        var rule = GetRule(tag);
        var attributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (rule.AllowGlobalAttributes)
        {
            attributes.UnionWith(GlobalAttributes);
        }

        attributes.UnionWith(rule.Attributes);
        attributes.UnionWith(rule.Fields.Select(field => field.AttributeName));

        return attributes
            .Where(AttributeRules.ContainsKey)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static bool IsAttributeAllowed(string tag, string attribute) =>
        AttributesFor(tag).Contains(attribute, StringComparer.OrdinalIgnoreCase);

    public static bool CanAppendChild(ElementNode parent, string childTag, out string error)
    {
        if (!CanAddChild(parent.Tag, childTag))
        {
            error = Localizer.IsSpanish
                ? $"La etiqueta {childTag} no se puede agregar dentro de {parent.Tag}."
                : $"The {childTag} tag cannot be added inside {parent.Tag}.";
            return false;
        }

        if (string.Equals(parent.Tag, "table", StringComparison.OrdinalIgnoreCase)
            && string.Equals(childTag, "caption", StringComparison.OrdinalIgnoreCase)
            && parent.Children.Count > 0)
        {
            error = Rule("caption must be the first child of table.", "caption debe ser el primer hijo de table.");
            return false;
        }

        if (string.Equals(parent.Tag, "fieldset", StringComparison.OrdinalIgnoreCase)
            && string.Equals(childTag, "legend", StringComparison.OrdinalIgnoreCase)
            && parent.Children.Count > 0)
        {
            error = Rule("legend must be the first child of fieldset.", "legend debe ser el primer hijo de fieldset.");
            return false;
        }

        if (string.Equals(parent.Tag, "details", StringComparison.OrdinalIgnoreCase)
            && string.Equals(childTag, "summary", StringComparison.OrdinalIgnoreCase)
            && parent.Children.Count > 0)
        {
            error = Rule("summary must be the first child of details.", "summary debe ser el primer hijo de details.");
            return false;
        }

        if (string.Equals(parent.Tag, "picture", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(childTag, "img", StringComparison.OrdinalIgnoreCase)
                && parent.Children.Any(child => string.Equals(child.Tag, "img", StringComparison.OrdinalIgnoreCase)))
            {
                error = Rule("picture must contain exactly one img.", "picture debe contener exactamente un img.");
                return false;
            }

            if (string.Equals(childTag, "source", StringComparison.OrdinalIgnoreCase)
                && parent.Children.Any(child => string.Equals(child.Tag, "img", StringComparison.OrdinalIgnoreCase)))
            {
                error = Rule("source elements must appear before img inside picture.", "Los elementos source deben ir antes de img dentro de picture.");
                return false;
            }
        }

        error = string.Empty;
        return true;
    }

    public static IReadOnlyList<HtmlValidationIssue> ValidateDocument(HTMLBuilder.Models.HtmlDocument document)
    {
        var issues = new List<HtmlValidationIssue>();
        if (!string.Equals(document.Root.Tag, "html", StringComparison.OrdinalIgnoreCase))
        {
            issues.Add(Issue("The document root must be html.", "El nodo raíz del documento debe ser html."));
            return issues;
        }

        if (document.Root.Children.Count != 2
            || !ReferenceEquals(document.Root.Children[0], document.Head)
            || !ReferenceEquals(document.Root.Children[1], document.Body))
        {
            issues.Add(Issue("html must contain head followed by body.", "html debe contener head seguido de body."));
        }

        ValidateNode(document.Root, [], issues);
        return issues;
    }

    public static void NormalizeAttributes(ElementNode node)
    {
        if (!string.Equals(node.Tag, "a", StringComparison.OrdinalIgnoreCase)
            || !node.Attributes.TryGetValue("target", out var target)
            || !string.Equals(target, "_blank", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var relValues = node.Attributes.GetValueOrDefault("rel", string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        relValues.Add("noopener");
        relValues.Add("noreferrer");
        node.Attributes["rel"] = string.Join(" ", relValues);
    }

    public static void NormalizeTree(ElementNode node)
    {
        NormalizeAttributes(node);
        foreach (var child in node.Children)
        {
            NormalizeTree(child);
        }
    }

    public static bool IsBooleanAttribute(string attribute) =>
        AttributeRules.TryGetValue(attribute, out var rule) && rule.IsBoolean;

    public static bool AttributeAllowsEmpty(string attribute) =>
        AttributeRules.TryGetValue(attribute, out var rule) && rule.AllowsEmpty;

    public static IReadOnlyList<string> PresetValuesFor(string attribute) =>
        PresetAttributeValues.TryGetValue(attribute, out var values) ? values : [];

    public static TagHelp GetHelp(string tag)
    {
        if (!Localizer.IsSpanish)
        {
            return EnglishTagHelp.TryGetValue(tag, out var englishHelp)
                ? englishHelp
                : Help(
                    "HTML tag available in the editor.",
                    "It may accept global attributes such as id, class, lang, title, and selected aria-* attributes.",
                    "Choose the tag whose meaning best matches the content.");
        }

        return TagHelp.TryGetValue(tag, out var help)
            ? help
            : Help(
                "Etiqueta HTML disponible en el editor.",
                "Puede aceptar atributos globales como id, class, lang, title y algunos aria-* según el caso.",
                "Elija siempre la etiqueta con el significado más cercano al contenido.");
    }

    public static AttributeHelp GetAttributeHelp(string attribute)
    {
        if (!Localizer.IsSpanish)
        {
            return EnglishAttributeHelp.TryGetValue(attribute, out var englishHelp)
                ? englishHelp
                : Attribute(
                    "Attribute available in the editor.",
                    "Check the specific tag documentation if you need exact values.",
                    "Use simple, semantic attributes whenever possible.");
        }

        return AttributeHelp.TryGetValue(attribute, out var help)
            ? help
            : Attribute(
                "Atributo disponible en el editor.",
                "Consulte la documentación de la etiqueta concreta si necesita valores específicos.",
                "Use atributos simples y semánticos siempre que sea posible.");
    }

    public static bool RequiresVisibleContent(string tag) =>
        RequiredVisibleContent.Contains(tag);

    public static bool IsRequiredAttribute(string tag, string attribute) =>
        FieldsFor(tag).Any(field => field.Required && string.Equals(field.AttributeName, attribute, StringComparison.OrdinalIgnoreCase));

    private static readonly HashSet<string> RequiredVisibleContent = new(StringComparer.OrdinalIgnoreCase)
    {
        "title", "h1", "h2", "h3", "h4", "h5", "h6", "p", "a", "strong", "em",
        "small", "mark", "blockquote", "q", "cite", "code", "pre", "time", "li",
        "figcaption", "caption", "th", "legend", "label", "option", "button", "summary"
    };

    private static void ValidateNode(ElementNode node, IReadOnlyList<string> ancestors, List<HtmlValidationIssue> issues)
    {
        var rule = GetRule(node.Tag);
        var canHaveText = rule.ContentKind is TagContentKind.Text or TagContentKind.TextAndChildren;
        var canHaveChildren = rule.ContentKind is TagContentKind.Children or TagContentKind.TextAndChildren;

        if (!canHaveText && !string.IsNullOrWhiteSpace(node.Text))
        {
            issues.Add(Issue(
                $"{node.Tag} cannot contain direct text.",
                $"{node.Tag} no puede contener texto directo."));
        }

        if (!canHaveChildren && node.Children.Count > 0)
        {
            issues.Add(Issue(
                $"{node.Tag} cannot contain child elements.",
                $"{node.Tag} no puede contener elementos hijos."));
        }

        if (RequiresVisibleContent(node.Tag) && !HasVisibleContent(node))
        {
            issues.Add(Issue(
                $"{node.Tag} must contain visible text or visible content.",
                $"{node.Tag} debe tener texto o contenido visible."));
        }

        foreach (var attribute in node.Attributes.Keys)
        {
            if (!IsAttributeAllowed(node.Tag, attribute))
            {
                issues.Add(Issue(
                    $"{attribute} is not allowed on {node.Tag}.",
                    $"{attribute} no está permitido en {node.Tag}."));
            }
        }

        ValidateRequiredAttributes(node, issues);
        ValidateSpecialAttributes(node, issues);
        ValidateSpecialChildren(node, issues);

        foreach (var child in node.Children)
        {
            if (!CanAddChild(node.Tag, child.Tag))
            {
                issues.Add(Issue(
                    $"{child.Tag} is not a valid child of {node.Tag}.",
                    $"{child.Tag} no es un hijo válido de {node.Tag}."));
            }

            if (string.Equals(child.Tag, "html", StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(Issue("html cannot be nested.", "html no puede estar dentro de otra etiqueta."));
            }

            if (string.Equals(child.Tag, "a", StringComparison.OrdinalIgnoreCase) && ancestors.Contains("a", StringComparer.OrdinalIgnoreCase)
                || string.Equals(node.Tag, "a", StringComparison.OrdinalIgnoreCase) && string.Equals(child.Tag, "a", StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(Issue("a cannot be nested inside a.", "a no puede estar dentro de a."));
            }

            if (string.Equals(child.Tag, "form", StringComparison.OrdinalIgnoreCase) && (ancestors.Contains("form", StringComparer.OrdinalIgnoreCase) || string.Equals(node.Tag, "form", StringComparison.OrdinalIgnoreCase)))
            {
                issues.Add(Issue("form cannot be nested inside form.", "form no puede estar dentro de form."));
            }

            if (string.Equals(child.Tag, "button", StringComparison.OrdinalIgnoreCase) && (ancestors.Contains("button", StringComparer.OrdinalIgnoreCase) || string.Equals(node.Tag, "button", StringComparison.OrdinalIgnoreCase)))
            {
                issues.Add(Issue("button cannot be nested inside button.", "button no puede estar dentro de button."));
            }

            if ((string.Equals(child.Tag, "a", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(child.Tag, "input", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(child.Tag, "select", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(child.Tag, "textarea", StringComparison.OrdinalIgnoreCase))
                && (ancestors.Contains("button", StringComparer.OrdinalIgnoreCase) || string.Equals(node.Tag, "button", StringComparison.OrdinalIgnoreCase)))
            {
                issues.Add(Issue(
                    $"{child.Tag} cannot be inside button.",
                    $"{child.Tag} no puede estar dentro de button."));
            }

            if (string.Equals(child.Tag, "label", StringComparison.OrdinalIgnoreCase)
                && (ancestors.Contains("label", StringComparer.OrdinalIgnoreCase) || string.Equals(node.Tag, "label", StringComparison.OrdinalIgnoreCase)))
            {
                issues.Add(Issue("label cannot be nested inside label.", "label no puede estar dentro de label."));
            }

            ValidateNode(child, ancestors.Append(node.Tag).ToArray(), issues);
        }
    }

    private static void ValidateRequiredAttributes(ElementNode node, List<HtmlValidationIssue> issues)
    {
        foreach (var field in FieldsFor(node.Tag).Where(field => field.Required))
        {
            if (!node.Attributes.TryGetValue(field.AttributeName, out var value)
                || (!field.AllowsEmpty && string.IsNullOrWhiteSpace(value)))
            {
                issues.Add(Issue(
                    $"{node.Tag} requires the {field.AttributeName} attribute.",
                    $"{node.Tag} necesita el atributo {field.AttributeName}."));
            }
        }

        if (string.Equals(node.Tag, "meta", StringComparison.OrdinalIgnoreCase))
        {
            var hasCharset = HasValue(node, "charset");
            var hasName = HasValue(node, "name");
            var hasContent = HasValue(node, "content");
            if (!hasCharset && !(hasName && hasContent))
            {
                issues.Add(Issue(
                    "meta must have charset or name together with content.",
                    "meta debe tener charset o name junto con content."));
            }
        }
    }

    private static void ValidateSpecialAttributes(ElementNode node, List<HtmlValidationIssue> issues)
    {
        if (string.Equals(node.Tag, "ol", StringComparison.OrdinalIgnoreCase)
            && node.Attributes.TryGetValue("start", out var start)
            && !string.IsNullOrWhiteSpace(start)
            && !int.TryParse(start, out _))
        {
            issues.Add(Issue("ol start must be numeric.", "start de ol debe ser numérico."));
        }

        if (string.Equals(node.Tag, "form", StringComparison.OrdinalIgnoreCase)
            && node.Attributes.TryGetValue("method", out var method)
            && !string.IsNullOrWhiteSpace(method)
            && !string.Equals(method, "get", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(method, "post", StringComparison.OrdinalIgnoreCase))
        {
            issues.Add(Issue("form method must be get or post.", "method de form debe ser get o post."));
        }

        if (string.Equals(node.Tag, "input", StringComparison.OrdinalIgnoreCase))
        {
            var type = node.Attributes.GetValueOrDefault("type", string.Empty);
            if (node.Attributes.ContainsKey("checked")
                && !string.Equals(type, "checkbox", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(type, "radio", StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(Issue("input checked only applies to checkbox or radio.", "checked de input solo aplica a checkbox o radio."));
            }

            if (node.Attributes.ContainsKey("placeholder")
                && new[] { "checkbox", "radio", "file", "submit", "button" }.Contains(type, StringComparer.OrdinalIgnoreCase))
            {
                issues.Add(Issue(
                    $"input placeholder does not apply to type {type}.",
                    $"placeholder de input no aplica al tipo {type}."));
            }
        }
    }

    private static void ValidateSpecialChildren(ElementNode node, List<HtmlValidationIssue> issues)
    {
        if ((string.Equals(node.Tag, "ul", StringComparison.OrdinalIgnoreCase)
                || string.Equals(node.Tag, "ol", StringComparison.OrdinalIgnoreCase))
            && !node.Children.Any(child => string.Equals(child.Tag, "li", StringComparison.OrdinalIgnoreCase)))
        {
            issues.Add(Issue($"{node.Tag} must contain at least one li.", $"{node.Tag} debe contener al menos un li."));
        }

        if (string.Equals(node.Tag, "select", StringComparison.OrdinalIgnoreCase)
            && !node.Children.Any(child => string.Equals(child.Tag, "option", StringComparison.OrdinalIgnoreCase)))
        {
            issues.Add(Issue("select must contain at least one option.", "select debe contener al menos un option."));
        }

        if (string.Equals(node.Tag, "tr", StringComparison.OrdinalIgnoreCase)
            && !node.Children.Any(child => string.Equals(child.Tag, "th", StringComparison.OrdinalIgnoreCase) || string.Equals(child.Tag, "td", StringComparison.OrdinalIgnoreCase)))
        {
            issues.Add(Issue("tr must contain at least one cell.", "tr debe contener al menos una celda."));
        }

        if (string.Equals(node.Tag, "table", StringComparison.OrdinalIgnoreCase))
        {
            if (!node.Children.Any(ContainsTableRow))
            {
                issues.Add(Issue("table must contain a tr directly or through a table section.", "table debe contener un tr directo o dentro de una sección de tabla."));
            }

            EnsureFirstChild(node, "caption", issues);
        }

        if (string.Equals(node.Tag, "fieldset", StringComparison.OrdinalIgnoreCase))
        {
            EnsureFirstChild(node, "legend", issues);
        }

        if (string.Equals(node.Tag, "details", StringComparison.OrdinalIgnoreCase))
        {
            EnsureFirstChild(node, "summary", issues);
        }

        if (string.Equals(node.Tag, "picture", StringComparison.OrdinalIgnoreCase))
        {
            var imageCount = node.Children.Count(child => string.Equals(child.Tag, "img", StringComparison.OrdinalIgnoreCase));
            if (imageCount != 1)
            {
                issues.Add(Issue("picture must contain exactly one img.", "picture debe contener exactamente un img."));
            }

            var imgIndex = node.Children.FindIndex(child => string.Equals(child.Tag, "img", StringComparison.OrdinalIgnoreCase));
            if (imgIndex >= 0 && node.Children.Skip(imgIndex + 1).Any(child => string.Equals(child.Tag, "source", StringComparison.OrdinalIgnoreCase)))
            {
                issues.Add(Issue("source elements must appear before img inside picture.", "Los elementos source deben ir antes de img dentro de picture."));
            }
        }

        if ((string.Equals(node.Tag, "audio", StringComparison.OrdinalIgnoreCase)
                || string.Equals(node.Tag, "video", StringComparison.OrdinalIgnoreCase))
            && !HasValue(node, "src")
            && !node.Children.Any(child => string.Equals(child.Tag, "source", StringComparison.OrdinalIgnoreCase)))
        {
            issues.Add(Issue($"{node.Tag} must have src or at least one source.", $"{node.Tag} debe tener src o al menos un source."));
        }
    }

    private static void EnsureFirstChild(ElementNode node, string childTag, List<HtmlValidationIssue> issues)
    {
        var index = node.Children.FindIndex(child => string.Equals(child.Tag, childTag, StringComparison.OrdinalIgnoreCase));
        if (index > 0)
        {
            issues.Add(Issue(
                $"{childTag} must be the first child of {node.Tag}.",
                $"{childTag} debe ser el primer hijo de {node.Tag}."));
        }
    }

    private static bool ContainsTableRow(ElementNode child) =>
        string.Equals(child.Tag, "tr", StringComparison.OrdinalIgnoreCase)
        || child.Children.Any(grandchild => string.Equals(grandchild.Tag, "tr", StringComparison.OrdinalIgnoreCase));

    private static bool HasVisibleContent(ElementNode node) =>
        !string.IsNullOrWhiteSpace(node.Text)
        || node.Children.Any(child =>
            HasVisibleContent(child)
            || string.Equals(child.Tag, "img", StringComparison.OrdinalIgnoreCase)
            || string.Equals(child.Tag, "input", StringComparison.OrdinalIgnoreCase)
            || string.Equals(child.Tag, "textarea", StringComparison.OrdinalIgnoreCase)
            || string.Equals(child.Tag, "select", StringComparison.OrdinalIgnoreCase)
            || string.Equals(child.Tag, "iframe", StringComparison.OrdinalIgnoreCase));

    private static bool HasValue(ElementNode node, string attribute) =>
        node.Attributes.TryGetValue(attribute, out var value) && !string.IsNullOrWhiteSpace(value);

    private static HtmlValidationIssue Issue(string english, string spanish) => new(Rule(english, spanish));

    private static string Rule(string english, string spanish) =>
        Localizer.IsSpanish ? spanish : english;

    private static TagRule GetRule(string tag) =>
        Rules.TryGetValue(tag, out var rule) ? rule : Text(tag);

    private static TagRule Flow(string name, string[]? attributes = null, string[]? parents = null) =>
        Rule(name, TagContentKind.TextAndChildren, [], attributes ?? [], parents ?? []);

    private static TagRule Text(string name) =>
        Rule(name, TagContentKind.TextAndChildren, [], []);

    private static TagRule Phrasing(string name, string[]? attributes = null, string[]? parents = null) =>
        Rule(name, TagContentKind.TextAndChildren, [], attributes ?? [], parents ?? []);

    private static TagRule Rule(
        string name,
        TagContentKind contentKind,
        IReadOnlyList<TagField> fields,
        string[] attributes,
        string[]? parents = null,
        bool allowGlobalAttributes = true) =>
        new(
            name,
            contentKind,
            fields,
            new HashSet<string>(attributes, StringComparer.OrdinalIgnoreCase),
            new HashSet<string>(parents ?? [], StringComparer.OrdinalIgnoreCase),
            allowGlobalAttributes);

    private static TagHelp Help(string description, string accessibility, string advice) =>
        new(description, accessibility, advice);

    private static AttributeHelp Attribute(string description, string values, string advice) =>
        new(description, values, advice);
}
