define([
    "dojo/_base/declare",
    "dojo/_base/lang",
    "dojo/html",
    "dijit/_TemplatedMixin",
    "dijit/_WidgetBase",
    "epi-cms/_ContentContextMixin",
    "epi/i18n!epi/cms/nls/egandalf.pagepreview",
    "dojo/text!./templates/PreviewControl.html"
],
    function (declare, lang, html, _TemplatedMixin, _WidgetBase, _ContentContextMixin, resources, template) {
        return declare("egandalf/PagePreview", [_WidgetBase, _TemplatedMixin, _ContentContextMixin], {
            templateString: template,
            postCreate: function () {
                var self = this;
                dojo.when(this.getCurrentContext(), function (context) {
                    self.refreshByContext(context);
                });
            },
            contextChanged: function (context, callerData) {
                this.refreshByContext(context);
            },
            refreshByContext: function (context) {
                if (context !== undefined && context.capabilities !== undefined && context.capabilities.isPage == true) {
                    html.set(this.previewHeader, resources["header"]);
                    html.set(this.previewInstructions, resources["instructions"]);
                    html.set(this.previewLink, '<a class="eg-btn" href="' + context["publicUrl"] + context["id"] + '" target="_blank">' + resources["button"] + '</a>');
                } else {
                    html.set(this.previewHeader, resources["errorheader"]);
                    html.set(this.previewInstructions, resources["errormessage"]);
                    html.set(this.previewLink, '');
                }
            }
        });
    }
);