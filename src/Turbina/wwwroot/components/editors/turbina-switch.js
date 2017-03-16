function(template) {
    "use strict";

    document.registerElement(
        "turbina-switch",
        class extends HTMLElement {

            createdCallback() {
                this._root = this.createShadowRoot();
                this._root.appendChild(template.content.cloneNode(true));
            }

            async init(pin) {
                const cb = this._root.querySelector('#cb');
                const subscription = pin.subscribe(value => {
                    cb.checked = value;
                });
                cb.onchange = () => {
                    pin.push(cb.checked);
                };
            }
        });
}