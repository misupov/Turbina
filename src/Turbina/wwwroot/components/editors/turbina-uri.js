function(template) {
    "use strict";

    document.registerElement(
        "turbina-uri",
        class extends HTMLElement {

            createdCallback() {
                this._root = this.createShadowRoot();
                this._root.appendChild(template.content.cloneNode(true));
            }

            async init(pin) {
                const uri = this._root.querySelector("#uri");
                const subscription = pin.subscribe(value => {
                    uri.value = value;
                    //pin.unsubscribe(subscription);
                });
                uri.oninput = () => {
                    pin.push(uri.value);
                };
            }
        });
}