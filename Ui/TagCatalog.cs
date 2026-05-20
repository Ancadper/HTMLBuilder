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
        ["main"] = Flow("main", ["aria-label"]),
        ["section"] = Flow("section", ["aria-label", "aria-labelledby"]),
        ["article"] = Flow("article", ["aria-label", "aria-labelledby"]),
        ["aside"] = Flow("aside", ["aria-label", "aria-labelledby"]),
        ["footer"] = Flow("footer", ["aria-label"]),
        ["h1"] = Text("h1"),
        ["h2"] = Text("h2"),
        ["h3"] = Text("h3"),
        ["h4"] = Text("h4"),
        ["h5"] = Text("h5"),
        ["h6"] = Text("h6"),
        ["p"] = Text("p"),
        ["a"] = Rule("a", TagContentKind.TextAndChildren,
            [new("href", "Enlace o URL", true, "Destino del enlace. Ejemplo: https://ejemplo.com o contacto.html.")],
            ["href", "target", "rel", "download"]),
        ["strong"] = Text("strong"),
        ["em"] = Text("em"),
        ["small"] = Text("small"),
        ["mark"] = Text("mark"),
        ["blockquote"] = Rule("blockquote", TagContentKind.TextAndChildren,
            [new("cite", "Fuente de la cita", false, "URL de la fuente original de la cita.")],
            ["cite"]),
        ["q"] = Rule("q", TagContentKind.Text,
            [new("cite", "Fuente de la cita", false, "URL de la fuente original de la cita.")],
            ["cite"]),
        ["cite"] = Text("cite"),
        ["code"] = Text("code"),
        ["pre"] = Text("pre"),
        ["br"] = Rule("br", TagContentKind.Empty, [], []),
        ["hr"] = Rule("hr", TagContentKind.Empty, [], []),
        ["ul"] = Flow("ul"),
        ["ol"] = Rule("ol", TagContentKind.Children,
            [new("start", "Número inicial", false, "Primer número de la lista si no empieza en 1.")],
            ["start"]),
        ["li"] = Rule("li", TagContentKind.TextAndChildren, [], [], ["ul", "ol"]),
        ["figure"] = Flow("figure"),
        ["figcaption"] = Rule("figcaption", TagContentKind.TextAndChildren, [], [], ["figure"]),
        ["img"] = Rule("img", TagContentKind.Empty,
            [
                new("src", "Archivo o URL de la imagen", true, "Ruta de la imagen."),
                new("alt", "Texto alternativo", true, "Describe la información útil de la imagen. Puede quedar vacío si es decorativa.", AllowsEmpty: true)
            ],
            ["src", "alt", "width", "height", "loading"]),
        ["picture"] = Flow("picture"),
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
        ["table"] = Flow("table"),
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
        ["fieldset"] = Flow("fieldset"),
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
        ["details"] = Flow("details", ["open"]),
        ["summary"] = Rule("summary", TagContentKind.TextAndChildren, [], [], ["details"]),
        ["time"] = Rule("time", TagContentKind.Text,
            [new("datetime", "Fecha u hora legible por máquina", false, "Versión ISO de la fecha u hora visible.")],
            ["datetime"]),
        ["address"] = Flow("address"),
        ["div"] = Flow("div"),
        ["span"] = Text("span")
    };

    public static readonly string[] CommonTags = TagOrder;

    public static readonly string[] CommonAttributes =
        AttributeRules.Keys.Order(StringComparer.OrdinalIgnoreCase).ToArray();

    private static readonly HashSet<string> StructuralTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "html", "head", "body"
    };

    private static readonly HashSet<string> PhrasingContainers = new(StringComparer.OrdinalIgnoreCase)
    {
        "p", "h1", "h2", "h3", "h4", "h5", "h6",
        "a", "strong", "em", "small", "mark", "q", "cite", "code", "pre",
        "label", "button", "time", "span"
    };

    private static readonly HashSet<string> PhrasingChildren = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "strong", "em", "small", "mark", "q", "cite", "code",
        "br", "img", "input", "label", "span", "time"
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

    public static IReadOnlyList<TagField> FieldsFor(string tag) =>
        GetRule(tag).Fields;

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

        if (PhrasingContainers.Contains(parentTag))
        {
            return PhrasingChildren.Contains(childTag)
                && !(string.Equals(parentTag, "a", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(childTag, "a", StringComparison.OrdinalIgnoreCase));
        }

        return !string.Equals(childTag, "title", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(childTag, "meta", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(childTag, "link", StringComparison.OrdinalIgnoreCase);
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

    public static bool IsBooleanAttribute(string attribute) =>
        AttributeRules.TryGetValue(attribute, out var rule) && rule.IsBoolean;

    public static bool AttributeAllowsEmpty(string attribute) =>
        AttributeRules.TryGetValue(attribute, out var rule) && rule.AllowsEmpty;

    public static IReadOnlyList<string> PresetValuesFor(string attribute) =>
        PresetAttributeValues.TryGetValue(attribute, out var values) ? values : [];

    public static TagHelp GetHelp(string tag) =>
        TagHelp.TryGetValue(tag, out var help)
            ? help
            : Help(
                "Etiqueta HTML disponible en el editor.",
                "Puede aceptar atributos globales como id, class, lang, title y algunos aria-* según el caso.",
                "Elija siempre la etiqueta con el significado más cercano al contenido.");

    public static AttributeHelp GetAttributeHelp(string attribute) =>
        AttributeHelp.TryGetValue(attribute, out var help)
            ? help
            : Attribute(
                "Atributo disponible en el editor.",
                "Consulte la documentación de la etiqueta concreta si necesita valores específicos.",
                "Use atributos simples y semánticos siempre que sea posible.");

    private static TagRule GetRule(string tag) =>
        Rules.TryGetValue(tag, out var rule) ? rule : Text(tag);

    private static TagRule Flow(string name, string[]? attributes = null, string[]? parents = null) =>
        Rule(name, TagContentKind.TextAndChildren, [], attributes ?? [], parents ?? []);

    private static TagRule Text(string name) =>
        Rule(name, TagContentKind.TextAndChildren, [], []);

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
