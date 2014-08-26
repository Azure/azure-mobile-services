// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

exports.TableHelper = function TableHelper(isDotNet) {
    this.isDotNet = isDotNet;
    this.newItems = '';
    this.getNewItems = function (id) {
        if (this.isDotNet) {
            this.newItems += (this.newItems) ? ' or Id eq \'' + id + '\'' : 'Id eq \'' + id + '\'';
            return '(' + this.newItems + ')';
        } else {
            if (!this.newItems) {
                this.newItems = 'id ge ' + id;
            }
            return this.newItems;
        }
    }
}

