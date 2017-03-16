function(template) {
    "use strict";

    document.registerElement(
        "turbina-timespan",
        class extends HTMLElement {

            createdCallback() {
                this._root = this.createShadowRoot();
                this._root.appendChild(template.content.cloneNode(true));
            }

            async init(pin) {
                const timespan = this._root.querySelector("#timespan");
                const subscription = pin.subscribe(value => {
                    timespan.value = value;
                    //pin.unsubscribe(subscription);
                });
                timespan.onchange = () => {
                    pin.push(timespan.value);
                };
            }
        });
}