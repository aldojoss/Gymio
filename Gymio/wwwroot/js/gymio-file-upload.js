window.gymioFileUpload = {
    bindDropZone: (zoneId, inputId) => {
        const zone = document.getElementById(zoneId);
        const input = document.getElementById(inputId);

        if (!zone || !input || zone.dataset.gymioDropBound === "true") {
            return;
        }

        zone.dataset.gymioDropBound = "true";

        const stop = (event) => {
            event.preventDefault();
            event.stopPropagation();
        };

        if (!zone.hasAttribute("tabindex")) {
            zone.setAttribute("tabindex", "0");
        }

        const setFiles = (files) => {
            if (!files || files.length === 0) {
                return;
            }

            const transfer = new DataTransfer();
            transfer.items.add(files[0]);
            input.files = transfer.files;
            input.dispatchEvent(new Event("change", { bubbles: true }));
        };

        ["dragenter", "dragover"].forEach((name) => {
            zone.addEventListener(name, (event) => {
                stop(event);
                zone.classList.add("drag-over");
            });
        });

        ["dragleave", "drop"].forEach((name) => {
            zone.addEventListener(name, (event) => {
                stop(event);
                zone.classList.remove("drag-over");
            });
        });

        zone.addEventListener("drop", (event) => {
            setFiles(event.dataTransfer?.files);
        });

        zone.addEventListener("paste", (event) => {
            setFiles(event.clipboardData?.files);
        });
    }
};
