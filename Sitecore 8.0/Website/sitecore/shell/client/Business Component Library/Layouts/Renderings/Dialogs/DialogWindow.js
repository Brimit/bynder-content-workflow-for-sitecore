﻿require.config({
    paths: {
        dialog: "/sitecore/shell/client/Speak/Assets/lib/ui/1.1/dialog"
    },
    shim: {
        'dialog': { deps: ['sitecore'] },
    }
});

define(["sitecore", "dialog"], function (_sc) {

    _sc.Factories.createBaseComponent({
        name: "Dialog",
        base: "ControlBase",
        selector: ".sc-dialogWindow",
        attributes: [
            { name: "keyboard", defaultValue: true },
            { name: "openModal", defaultValue: true },
            { name: "backdrop", defaultValue: true },
            { name: "remote", defaultValue: false },
            { name: "isAnimated", defaultValue: true },
            { name: "size", defaultValue: "medium" },
            { name: "focusOn", defaultValue: "" },
            { name: "spinner", defaultValue: "" },
            { name: "maxHeight", defaultValue: "" },
            { name: "consumeTab", defaultValue: true },
            { name: "replace", defaultValue: true },
            { name: "modalOverflow", defaultValue: false },
            { name: "manager", defaultValue: "GlobalModalManager" }
        ],
        extendModel: {
            show: function() {
                this.trigger("show");
            },
            hide: function() {
                this.trigger("hide");
            }
        },
        initialize: function() {
            //data-sc-open
            //data-sc-keyboard
          //data-sc-target
          var initialValues, self = this,
              initialValuesStr = this.$el.data("sc-initial-values");
          if (initialValuesStr) {
            if (_.isObject(initialValuesStr)) {
              initialValues = initialValuesStr;
            } else {
              if (_.isString(initialValuesStr)) {
                initialValues = JSON.parse(initialValuesStr);
              }
            }
          
            _.each(initialValues, function (val, key) {
              if (!_.isUndefined(val)) {
                self.model.set(key, val.toString());
              }
            });
          }
          //this.model.on("change", this.updatePlugin, this);
          this.model.on("show", this.show, this);
          this.model.on("hide", this.hide, this);
        },
        toggle: function () {
            this.$el.modal("toggle");
        },
        show: function () {
            this.$el.modal('show');
        },
        hide: function () {
            this.$el.modal("hide");
        },
        loading: function () {
            this.$el.modal("loading");
        }
    });
});