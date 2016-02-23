﻿var id = getUrlVars()["id"];
var gcTemplateProxyId = getUrlVars()["gcTemplateProxyId"];
var url = '/api/sitecore/mappings/GetMapping?id=' + id + "&gcTemplateProxyId=" + gcTemplateProxyId;

function ViewModel() {
    var self = this;

    this.Rules = ko.observableArray();
    this.SelectedTemplate = ko.observable();
    this.IsEdit = ko.observable();
    this.GcTemplateProxyId = ko.observable();
    this.GcProjectName = ko.observable();
    this.GcTemplateName = ko.observable();
    this.GcMappingTitle = ko.observable();
    this.OpenerId = ko.observable();
    this.DefaultLocation = ko.observable();
    this.DefaultLocationTitle = ko.observable();
    this.IsShowing = ko.observable();
    this.GcTemplateId = ko.observable();
    this.SelectedTemplateId = ko.observable();
    this.SitecoreTemplates = ko.observableArray();
    this.SitecoreFields = ko.observableArray();
    this.Tabs = ko.observableArray();
    this.errorText = ko.observable();
    this.isError = ko.observable();


    jQuery.getJSON(url, null, function (data) {
        if (data.status != "error") {
            self.GcProjectName("Project:" + " " + data.GcProjectName);
            self.GcTemplateName("Template:" + " " + data.GcTemplateName);
            self.SitecoreTemplates(data.SitecoreTemplates),
            self.Rules(data.Rules),
            self.GcTemplateProxyId(data.GcTemplateProxyId),
            self.SelectedTemplate(self.find("SitrecoreTemplateId", data.AddMappingModel.SelectedTemplateId));
            self.GcMappingTitle(data.AddMappingModel.GcMappingTitle);
            self.OpenerId(data.AddMappingModel.OpenerId);
            self.DefaultLocation(data.AddMappingModel.DefaultLocation);
            self.DefaultLocationTitle(data.AddMappingModel.DefaultLocationTitle);
            self.IsShowing(false);
            self.templateChanged();
            self.GcTemplateId(data.AddMappingModel.GcTemplateId),
            self.Tabs(data.AddMappingModel.Tabs);
            self.IsEdit(data.AddMappingModel.IsEdit);
            self.isError(false);
        } else {
            self.errorText("Error:" + " " + data.message);
            self.isError(true);
        }

        jQuery(".preloader").hide();
        tabInitSlide();
    });
    function tabInitSlide() {
        jQuery(".content_mapping").slideUp(0);
        jQuery(".title_mapping").removeClass("open");
        jQuery(jQuery(".title_mapping")[0]).addClass("open");
        jQuery(jQuery(".content_mapping")[0]).slideDown(0);
        jQuery("body").on("click", ".title_mapping", function () {
            if (jQuery(this).hasClass("open")) {
                jQuery(this).next(".content_mapping").slideUp(200);
                jQuery(this).removeClass("open");
            } else {
                jQuery(".title_mapping.open").next(".content_mapping").slideUp(200);
                jQuery(".title_mapping.open").removeClass("open");
                jQuery(this).addClass("open");
                jQuery(this).next(".content_mapping").slideDown(200);

            }
        });
    }


    this.saveMapping = function () {
        //var dataObject = ko.toJSON(this);
   
        var model = new function () {
            this.TemplateTabs = self.Tabs();
            this.IsEdit = self.IsEdit();
            this.SelectedTemplateId = self.SelectedTemplateId();
            this.TemplateId = self.GcTemplateId();
            this.GcMappingTitle = self.GcMappingTitle();
            this.GcTemplateProxyId = self.GcTemplateProxyId();
            this.DefaultLocation = self.DefaultLocation();
        }

        jQuery.ajax({
            url: '/api/sitecore/mappings/Post?isEdit=' + self.IsEdit() + '&templateId=' + self.GcTemplateId() +
                '&selectedTemplateId=' + self.SelectedTemplateId() + '&gcMappingTitle=' + self.GcMappingTitle() + '&gcTemplateProxyId=' + self.GcTemplateProxyId(),
            type: 'post',
            data: JSON.stringify(model),
            contentType: 'application/json',
            success: function (data) {
                if (data.status != "error") {
                    window.opener.location.reload(true);
                    window.top.dialogClose();
                } else {
                    self.errorText("Error:" + " " + data.message);
                    self.isError(true);
                }
            },

        });
    };


    this.openDropTree = function () {
        var id = this.OpenerId();

        if (!this.IsShowing()) {
            //TODO use Knockout
            jQuery("#" + id).show();
            this.IsShowing(true);
            var mapping = this;
            jQuery("#" + id).dynatree({
                autoFocus: false,
                imagePath: "~/icon/",
                initAjax: {
                    url: '/api/sitecore/Import/GetTopLevelNode',
                    data: { mode: "funnyMode" }
                },
                onActivate: function (node) {
                    jQuery('[data-openerid="' + id + '"]').val(node.data.title);
                    mapping.DefaultLocation(node.data.key);
                    mapping.DefaultLocationText(node.data.title);
                },
                onLazyRead: function (node) {
                    node.appendAjax({
                        url: "/api/sitecore/Import/GetChildrenAsJson?id=" + node.data.key,
                        data: {
                            key: node.data.key,
                            mode: "funnyMode"
                        }
                    });
                }
            });
        }
        else {
            //TODO use Knockout
            jQuery("#" + id).hide();
            this.IsShowing(false);
        }
    }

    this.templateChanged = function () {
        this.SitecoreFields(self.SelectedTemplate().SitecoreFields);
        this.SelectedTemplateId(self.SelectedTemplate().SitrecoreTemplateId);
    }



    this.GetCurrentFields = function (item) {
        var fieldType = item.FieldType;
        var allowedFields = self.Rules()[fieldType];
        if (typeof allowedFields === 'undefined') {
            return self.SelectedTemplate().SitecoreFields[0];
        } else {
            if (allowedFields != null) {
                var allowedFieldsArr = allowedFields.split(",");
                var currentCollection = self.SelectedTemplate().SitecoreFields;
                var resultCollection = [];
                resultCollection.push(self.SelectedTemplate().SitecoreFields[0]);
                for (var i = 0; i < currentCollection.length; i++) {
                    var currentElement = currentCollection[i];
                    for (var f = 0; f < allowedFieldsArr.length; f++) {
                        var field = allowedFieldsArr[f];
                        if (currentElement.SitecoreFieldType == field.trim()) {
                            resultCollection.push(currentElement);
                        }
                    }
                }
                return resultCollection;
            }
        }
        return self.SelectedTemplate().SitecoreFields[0];
    };



    this.returnFieldName = function (item) {
        if (item.FieldName === null) {
            return "[Empty]" + " (" + item.FieldId + ")";
        } else {
            return item.FieldName;
        }
    };



    this.find = function (prop, data) {
        return ko.utils.arrayFirst(self.SitecoreTemplates(), function (item) {
            return item[prop] === data;
        });
    };
};

jQuery(window).resize(function () {
    jQuery(".tabs_mapping").css("max-height", jQuery(".gathercontent-dialog").height() - 240);
});
