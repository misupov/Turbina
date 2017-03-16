"use strict";

window.onload = function () {
    const session = new TurbinaSession(`ws://${window.location.host}`, workspace);
    const surface = document.getElementById("surface");

    session.onAddNode = async (node) => {
        return await surface.addNode(node);
    };

    session.onAddLink = async (link) => {
        return await surface.bind(link);
    };

    session.onResetWorkspace = async () => {
        return await surface.reset();
    }

    session.onOpen = () => {
        session.loadWorkspace();
    };

    document.session = session;
};
