﻿var UpdateManager = function () {
    var MODE = {
        ChooseItmesForImort: 1,
        CheckItemsBeforeImport: 2,
        Import: 3,
        ImportResult: 4,
        Close: 5,
        Error: 6
    };

    var allItems = [];
    var self = this;

    self.errorText = ko.observable(),
    self.successImportedItemsCount = ko.observable(),
    self.currentMode = ko.observable(MODE.ChooseItmesForImort);

    self.items = ko.observableArray([]),
    self.statuses = ko.observableArray([]),
    self.templates = ko.observableArray([]),

    self.statusPostState = ko.observable(false),

    self.statusFilter = ko.observable(),
    self.templateFilter = ko.observable(),

    self.query = ko.observable(''),

    self.init = function () {
        var callbackFunction = function (response) {
            self.initVariables(response);
        }
        self.initRequestHandler(callbackFunction);
    },

     self.initRequestHandler = function (callbackFunction) {
         var id = getUrlVars()["id"];
         
         jQuery.getJSON('/sitecore/api/getItemsForUpdate?id={' + id + '}').success(function (response) {
             callbackFunction(response);
             jQuery(".preloader").hide();
             initTooltip();
         }).error(function(response) {
             self.errorCallbackHandle(response);
         });

         document_resize();
     }

    self.errorCallbackHandle = function (response) {
        jQuery(".preloader").hide();
        self.errorText(response.responseJSON);
        self.buttonClick(MODE.Error);
    }

    self.initVariables = function (response) {
        var items = self.setupWatcher(response.Data.Items);
        self.items(items);
        allItems = items.slice(0);
        
        self.statuses(response.Filters.Statuses);

        self.templates(response.Filters.Templates);
    }
    
    self.setupDefaultValuesToFilters = function () {
        self.query('');
        self.statusFilter();
        self.templateFilter();
    }

    //filters
    self.filter = function () {
        self.items.removeAll();

        var currentCollection = allItems.slice(0);
        currentCollection = self.search(currentCollection);
        currentCollection = self.filterByStatus(currentCollection);
        currentCollection = self.filterByTemplate(currentCollection);

        self.items(currentCollection);
        jQuery(".tooltip").remove();
        initTooltip();
    }

    self.search = function (currentCollection) {
        var resultCollection = currentCollection;
        var value = self.query();
        if (value !== '') {
            resultCollection = [];
            for (var i = 0; i < currentCollection.length; i++) {
                var currentElement = currentCollection[i];
                if (currentElement.Title.toLowerCase().indexOf(value.toLowerCase()) >= 0) {
                    resultCollection.push(currentElement);
                }
            }
        }

        return resultCollection;
    },

    self.filterByStatus = function (currentCollection) {
        var resultCollection = currentCollection;
        var value = self.statusFilter();
        if (value) {
            resultCollection = [];
            for (var i = 0; i < currentCollection.length; i++) {
                var currentElement = currentCollection[i];
                if (currentElement.Status.id === value) {
                    resultCollection.push(currentElement);
                }
            }
        }
        return resultCollection;
    },

    self.filterByTemplate = function (currentCollection) {
        var resultCollection = currentCollection;
        var value = self.templateFilter();
        if (value) {
            resultCollection = [];
            for (var i = 0; i < currentCollection.length; i++) {
                var currentElement = currentCollection[i];
                if (currentElement.Template.id === value) {
                    resultCollection.push(currentElement);
                }
            }
        }

        return resultCollection;
    }

    //table rendering
    self.AllChecked = ko.computed({
        read: function () {
            var items = self.items();
            if (items.length === 0)
                return false;

            var firstUnchecked = ko.utils.arrayFirst(items, function (item) {
                if (item.Checked)
                    return item.Checked() === false;
            });

            return firstUnchecked == null;
        },
        write: function (value) {
            ko.utils.arrayForEach(self.items(), function (item) {
                item.Checked(value);
            });
        }
    });

    self.getCheckedCount = ko.computed(function () {
        var counter = 0;
        ko.utils.arrayForEach(self.items(), function (item) {
            if (item.Checked && item.Checked() === true)
                counter++;
        });

        return counter;
    });

    self.afterStatusesSelectRender = function (option, status) {
        if (status.color) {
            option.style.color = status.color;
        }
    };

    self.getSelectedStatusColor = function () {
        var result = "";
        ko.utils.arrayForEach(self.statuses(), function (status) {
            if (status.id.toLowerCase() === self.statusFilter()) {
                result = status.color;
                return;
            }
        });

        return result;
    }

    self.checkRow = function () {
        this.Checked(!this.Checked());
    }

    self.query.subscribe(self.filter);

    //button click events
    self.switchToCheckItemsBeforeImport = function () {
        var result = [];
        ko.utils.arrayForEach(self.items(), function (item) {
            if (item.Checked && item.Checked() === true)
                result.push(item);
        });

        self.items(result);
    }

    self.import = function () {
        var id = getUrlVars()["id"];
        var items = self.items();
        var status = self.statusFilter();
        if (!self.statusPostState())
            status = "";
        jQuery.ajax
        ({
            type: "POST",
            url: '/sitecore/api/update?id={' + id + '}&statusId=' + status,
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify(items),
            success: function (response) {
                var count = self.items().length - response.Items.length;
                self.successImportedItemsCount(count);
                self.items(response.Items);
                self.buttonClick(MODE.ImportResult);
            },
            error: function (response) {
                self.errorCallbackHandle(response);
            }
        });
    }

    self.close = function () {
        window.top.dialogClose();
    }

    self.backButtonClick = function () {
        self.items(allItems.slice(0));
    }

    self.buttonClick = function (newMode) {
        if (newMode === MODE.CheckItemsBeforeImport) {
            self.currentMode(newMode);
            self.switchToCheckItemsBeforeImport();
        } else if (newMode === MODE.Import) {
            self.currentMode(newMode);
            self.import();
        } else if (newMode === MODE.Close) {
            self.currentMode(newMode);
            self.close();
        } else if (newMode === MODE.ChooseItmesForImort) {
            self.statusFilter = ko.observable();
            self.currentMode(newMode);
            self.backButtonClick();
        } else {
            self.currentMode(newMode);
        }
    }

    self.getMode = function (section) {
        if (self.currentMode() === section) {
            return true;
        }
        return false;
    }

    self.setupWatcher = function (items) {
        for (var i = 0; i < items.length; i++) {
            items[i].Checked = ko.observable(false);
        }

        return items;
    }

    self.getImportResultTemplateColor = function (item) {
        if (!item.IsImportSuccessful)
            return 'red';
    }

    self.getImportResultMessageColor = function (item) {
        if (!item.IsImportSuccessful)
            return 'red';
        return 'green';
    }

    self.getDateColor = function (item) {
        //if (item.IsNeedToHighlightingDate)
        return 'dateWarnings';
    }

    self.init();
}