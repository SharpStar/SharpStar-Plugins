/*
* XSockets.NET XSockets.fallback.latest.js 
* http://xsockets.net/
* Distributed in whole under the terms of the MIT
 
*
* Copyright 2014, Magnus Thor & Ulf Björklund 
*
* Permission is hereby granted, free of charge, to any person obtaining
* a copy of this software and associated documentation files (the
* "Software"), to deal in the Software without restriction, including
 
* without limitation the rights to use, copy, modify, merge, publish,
* distribute, sublicense, and/or sell copies of the Software, and to
* permit persons to whom the Software is furnished to do so, subject to
* the following conditions:
 
*
* The above copyright notice and this permission notice shall be
* included in all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 
* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
* NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
* LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 
* OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
* WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*
*/
if ("WebSocket" in window === false) {
    window.WebSocket = function (url, subprotocol) {
        var self = this;
        $(window).bind('beforeunload', function () {
            self.close();
        });
        subprotocol = subprotocol || url.match(/.+?\:\/\/.+?(\/.+?)(?:#|\?|$)/).pop().replace("/", "");
        this.handler = url;
        this.client = {
            ClientGuid: "",
            StorageGuid: "",
        };
        this.MessageEvent = function (data) {
            return {
                type: "message",
                data: JSON.stringify(data)
            };
        };
        this.payload = function (data) {
            return {
                handler: self.handler,
                client: self.client.ClientGuid,
                Json: data
            };
        };
        this.listener = function () {
            return {
                handler: self.handler,
                client: self.client.ClientGuid
            };
        };
        this.readystate = 0;
        this.ajax("/Fallback/Init", "GET", {
            url: self.handler,
            storageGuid: window.localStorage.getItem("XSocketsClientStorageGuid" + subprotocol)
        }, true, function (msg) {
            var args = JSON.parse(msg.data);
            self.client.ClientGuid = args.ClientGuid;
            self.client.StorageGuid = args.StorageGuid;
            self.readyState = 1;
            self.onmessage(new self.MessageEvent(msg));
            window.localStorage.setItem("XSocketsClientStorageGuid" + subprotocol, args.StorageGuid);
            self.listen();
        });
        return this;
    };
    window.WebSocket.prototype.close = function () {
        this.ajax("/fallback/close", "GET", {
            client: self.client.ClientGuid
        }, true, function () {
        });
    };
    window.WebSocket.prototype.readyState = 0;
    window.WebSocket.prototype.send = function (data) {
        
        var msg = JSON.parse(data);
        if (msg.event == XSockets.Events.open) return;
        if (msg.event == XSockets.Events.pubSub.unsubscribe) {
            this.ajax("/fallback/unbind", "GET", {
                client: this.client.ClientGuid,
                event: JSON.parse(msg.data).Event
            }, true, function () { });
        } else if (msg.event == XSockets.Events.pubSub.subscribe) {
            this.ajax("/fallback/bind", "GET", {
                client: this.client.ClientGuid,
                event: JSON.parse(msg.data).Event
            }, false, function () { });

        } else {
            this.ajax("/fallback/trigger", "POST", this.payload(data), false, function () { });
        }
    };
    window.WebSocket.prototype.close = function () {
        $.getJSON("/Fallback/Close", {
            client: this.client.ClientGuid
        }).done(function () {
            window.sessionStorage.clear();
        });
    };
    window.WebSocket.prototype.ajax = function (url, method, payload, async, callback) {
        var settings = {
            processData: true,
            dataType: "json",
            type: method,
            url: url,
            async: async,
            cache: false,
            success: callback,
            data: payload
        };
        $.ajax(settings);
    };
    window.WebSocket.prototype.onmessage = function () { };
    window.WebSocket.prototype.onerror = function () { };
    window.WebSocket.prototype.listen = function () {
        var self = this;
        this.ajax("/fallback/listen", "POST", this.listener(), true, function (messages) {
            $.each(messages, function (index, message) {
                self.onmessage(self.MessageEvent(message));
            });
            self.listen();
        });
    };

}