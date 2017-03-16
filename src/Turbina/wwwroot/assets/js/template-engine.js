class TemplateEngine {
    constructor(session) {
        this._session = session;
        this._nodeTemplates = new Map();
        this._pinTemplates = new Set();
    }

    async loadTemplate(tagName) {
        if (this._pinTemplates.has(tagName)) {
            return;
        }
        this._pinTemplates.add(tagName);

        const templateString = await this._loadPinTemplate(tagName);
        const template = this._createTemplate(templateString);
        await this._loadPinJs(template, tagName);
    }

    async _loadPinTemplate(name) {
        const response = await fetch(`/components/editors/${name}.html`);
        return response.text();
    }

    async _loadPinJs(template, name) {
        const response = await fetch(`/components/editors/${name}.js`);
        const text = await response.text();
        eval(`(${text})(template)`);
    }

    _createTemplate(htmlStr) {
        const template = document.createElement("template");
        template.innerHTML = htmlStr;
        return template;
    }
}