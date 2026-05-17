window.gymioPrintTicket = (elementId) => {
    const source = document.getElementById(elementId);
    if (!source) {
        throw new Error("No se encontro el ticket para imprimir.");
    }

    const frame = document.createElement("iframe");
    frame.style.position = "fixed";
    frame.style.right = "0";
    frame.style.bottom = "0";
    frame.style.width = "0";
    frame.style.height = "0";
    frame.style.border = "0";
    frame.style.opacity = "0";
    document.body.appendChild(frame);

    const frameWindow = frame.contentWindow;
    const frameDocument = frameWindow.document;

    frameDocument.open();
    frameDocument.write(`
<!doctype html>
<html>
<head>
    <meta charset="utf-8">
    <title>Ticket Gymio</title>
    <style>
        @page { size: 80mm auto; margin: 0; }
        * { box-sizing: border-box; }
        body {
            margin: 0;
            background: white;
            color: black;
            font-family: "Courier New", Courier, monospace;
            font-size: 12px;
        }
        #ticket-impresion {
            width: 80mm;
            max-width: 80mm;
            padding: 8px;
            background: white;
            color: black;
        }
        .text-center { text-align: center; }
        .text-start { text-align: left; }
        .text-end { text-align: right; }
        .fw-bold, .fw-black { font-weight: 900; }
        .mb-0 { margin-bottom: 0; }
        .mb-2 { margin-bottom: .5rem; }
        .mb-3 { margin-bottom: 1rem; }
        .mt-2 { margin-top: .5rem; }
        .mt-3 { margin-top: 1rem; }
        .p-3 { padding: 1rem; }
        .w-100 { width: 100%; }
        table { width: 100%; border-collapse: collapse; }
        th, td { padding: 2px 0; vertical-align: top; }
        .border-bottom { border-bottom: 1px solid black; }
    </style>
</head>
<body>${source.outerHTML}</body>
</html>`);
    frameDocument.close();

    frameWindow.focus();
    setTimeout(() => {
        frameWindow.print();
        setTimeout(() => frame.remove(), 1000);
    }, 150);
};
