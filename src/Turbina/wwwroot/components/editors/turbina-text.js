function(template) {
    "use strict";

    document.registerElement(
        "turbina-text",
        class extends HTMLElement {

            createdCallback() {
                this._root = this.createShadowRoot();
                this._root.appendChild(template.content.cloneNode(true));
            }

            async init(pin) {
                const text = this._root.querySelector("#text");
                const subscription = pin.subscribe(value => {
                    text.value = value;
                    //pin.unsubscribe(subscription);
                });
                text.oninput = () => {
                    pin.push(text.value);
                };
            }
        });
}