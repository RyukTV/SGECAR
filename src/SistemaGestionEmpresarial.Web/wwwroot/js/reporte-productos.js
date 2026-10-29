window.descargarReporteProductosPdf = async (nombreArchivo, referenciaStream) => {
    const datos = await referenciaStream.arrayBuffer();
    const url = URL.createObjectURL(new Blob([datos], { type: "application/pdf" }));
    const enlace = document.createElement("a");
    enlace.href = url;
    enlace.download = nombreArchivo;
    document.body.appendChild(enlace);
    enlace.click();
    enlace.remove();
    setTimeout(() => URL.revokeObjectURL(url), 60000);
};
