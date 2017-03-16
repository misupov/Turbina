"use strict";

class TurbinaSession {
    constructor(endpoint, workspace) {
        this._workspace = workspace;
        this._updateSubscriptions = new Map();
        this._webSocket = new WebSocket(endpoint);
        this._promise = new Promise((r) => r());
        this._webSocket.onopen = () => {
            this.onOpen();
        };

        this._webSocket.onmessage = async (evt) => {
            this._promise = this._onMessage(evt);
        };

        this._webSocket.onerror = (evt) => {
            alert(evt.message);
        };

        this._webSocket.onclose = () => {
            alert("CLOSE");
        };
    }

    _onMessage(evt) {
        return this._promise.then(async () => {
            const msg = JSON.parse(evt.data);

            const results = [];

            if (msg["workspace"] === workspace) {
                for (let message of msg["messages"]) {
                    results.push(await this.handleMessage(message));
                }
            }

            results.forEach(r => {
                if (!!r) {
                    r();
                }
            });
        });
    }

    send(type, action, args) {
        const id = "123";
        const workspace = this._workspace;
        const command = {
            "id": id,
            "workspace": workspace,
            "type": type,
            "action": action,
            "args": args
        }
        this._webSocket.send(JSON.stringify(command));
    }

    push(node, pin, value) {
        this.send("pin",
            "push",
            {
                "node": node,
                "pin": pin,
                "value": value
            });
    }

    async handleMessage(message) {
        switch (message.type) {
        case "add-node":
        {
            return await this.onAddNode(message);
        }
        case "add-link":
        {
            return this.onAddLink(message);
        }
        case "reset-workspace":
        {
            return await this.onResetWorkspace();
        }
        case "value-snapshot":
        {
            this.onValueSnapshotReceived(message["node"], message["pin"], message["value"]);
            break;
        }
        case "new-value":
        {
            this._handleNewValue(message["node"], message["pin"], message["value"]);
            break;
        }
        }
    }

    resetWorkspace() {
        this.send("workspace", "reset");
    }

    loadWorkspace() {
        this.send(
            "workspace",
            "load");
    }

    createNewNode(node, left, top) {
        this.send(
            "node",
            "add",
            {
                "node-type": node,
                "left": left,
                "top": top,
                "title": "none!"
            });
    }

    bindPins(fromNode, fromPin, toNode, toPin) {
        this.send(
            "pin",
            "bind",
            {
                from: { "node": fromNode, "pin": fromPin },
                to: { "node": toNode, "pin": toPin }
            });
    }

    unbind(node, pin) {
        this.send(
            "pin",
            "unbind",
            {
                "node": node,
                "pin": pin
            });
    }

    getValueSnapshot(node, pin) {
        this.send(
            "pin",
            "get-value",
            {
                "node": node,
                "pin": pin
            });
    }

    subscribe(node, pin, onUpdate) {
        this.send(
            "pin",
            "subscribe",
            {
                "node": node,
                "pin": pin
            });

        const updateSubscription = new UpdateSubscription(node, pin, onUpdate);

        let subscriptionsSet = this._updateSubscriptions.get(node + ":" + pin);
        if (!subscriptionsSet) {
            subscriptionsSet = new Set();
            this._updateSubscriptions.set(node + ":" + pin, subscriptionsSet);
        }
        subscriptionsSet.add(updateSubscription);

        return updateSubscription;
    }

    unsubscribe(subscription) {
        const key = subscription.node + ":" + subscription.pin;
        const subscriptionsSet = this._updateSubscriptions.get(key);
        if (!!subscriptionsSet) {
            subscriptionsSet.delete(subscription);
        }
        if (subscriptionsSet.size === 0) {
            this._updateSubscriptions.delete(key);
        }
//        this.send(
//            "pin",
//            "unsubscribe",
//            {
//                "node": subscription.node,
//                "pin": subscription.pin
//            });
    }

    _handleNewValue(node, pin, value) {
        const subscriptions = this._updateSubscriptions.get(node + ":" + pin);
        if (!!subscriptions) {
            subscriptions.forEach(s => s.onUpdate(value));
            this.send(
                "pin",
                "subscribe",
                {
                    "node": node,
                    "pin": pin,
                    "latestValue": value
                });
        }
    }
}

class UpdateSubscription {
    constructor(node, pin, onUpdate) {
        this.node = node;
        this.pin = pin;
        this.onUpdate = onUpdate;
    }
}