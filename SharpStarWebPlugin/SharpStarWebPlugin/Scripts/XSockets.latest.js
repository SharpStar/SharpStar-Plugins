/*
* XSockets.latest.js
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
*/

var XSockets = {
    Version: "3.0",
    Events: {
        onError: "0x1f4",
        open: "0xc8",
        close: "0x12e0",
        storage: {
            set: "0x190",
            get: "0x191",
            getAll: "0x192",
            remove: "0x193"
        },
        onBlob: "0x12d0",
        connection: {
            onclientconnect: "0xc9",
            onclientdisconnect: "0xca",
            disconnect: "0xcb"
        },
        bindings: {
            completed: "0x12c0"
        },
        pubSub: {
            subscribe: "0x12c",
            unsubscribe: "0x12d"
        }
    },
    Utils: {
        longToByteArray: function (long) {
            var byteArray = [0, 0, 0, 0, 0, 0, 0, 0];
            for (var index = 0; index < byteArray.length; index++) {
                var byte = long & 0xff;
                byteArray[index] = byte;
                long = (long - byte) / 256;
            }
            return byteArray;
        },
        stringToBuffer: function (string) {
            var i, len = string.length,
                arr = new Array(len);
            for (i = 0; i < len; i++) {
                arr[i] = string.charCodeAt(i) & 0xFF;
            }
            return new Uint8Array(arr).buffer;
        },

        parseUri: function (url) {
            var uriParts = {};

            var uri = {
                port: 80,
                relative: "/"
            };
            var keys = ["url", "scheme", "host", "domain", "port", "fullPath", "path", "relative", "controller", "query"],
                parser = /^(?:([^:\/?#]+):)?(?:\/\/(([^:\/?#]*)(?::(\d*))?))?((((?:[^?#\/]*\/)*)([^?#]*))(?:\?([^#]*))?)/;

            var result = url.match(parser);
            for (var i = 0; i < keys.length; i++) {
                uriParts[keys[i]] = result[i];
            }
            uriParts.rawQuery = uriParts.query || "";
            if (!uriParts.query) {
                uriParts.query = {};
            } else {
                var stack = {};
                var arrQuery = uriParts.query.split("&");
                for (var q = 0; q < arrQuery.length; q++) {
                    var keyValue = arrQuery[q].split("=");
                    stack[keyValue[0]] = keyValue[1];
                }
                uriParts.query = stack;
            }

            uriParts = XSockets.Utils.extend(uri, uriParts);
            uriParts.absoluteUrl = uriParts.scheme + "://" + uriParts.domain + ":" + uriParts.port + uri.relative + uriParts.controller;


            return uriParts;
        },
        randomString: function (x) {
            var s = "";
            while (s.length < x && x > 0) {
                var r = Math.random();
                s += String.fromCharCode(Math.floor(r * 26) + (r > 0.5 ? 97 : 65));
            }
            return s;
        },
        getParameterByName: function (name) {
            name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
            var regexS = "[\\?&]" + name + "=([^&#]*)";
            var regex = new RegExp(regexS);
            var results = regex.exec(window.location.search);
            if (results == null)
                return "";
            else
                return decodeURIComponent(results[1].replace(/\+/g, " "));
        },
        extend: function (obj, extObj) {
            if (arguments.length > 2) {
                for (var a = 1; a < arguments.length; a++) {
                    this.extend(obj, arguments[a]);
                }
            } else {
                for (var i in extObj) {
                    obj[i] = extObj[i];
                }
            }
            return obj;
        },
        guid: function (a, b) {
            for (b = a = ''; a++ < 36; b += a * 51 & 52 ? (a ^ 15 ? 8 ^ Math.random() * (a ^ 20 ? 16 : 4) : 4).toString(16) : '-');
            return b;
        }
    }
};

XSockets.Callback = (function () {
    function Callback(name, func, opt) {
        this.name = name;
        this.fn = func;
        this.state = opt;
        this.id = XSockets.Utils.guid();
    }
    return Callback;
})();
XSockets.Subscription = (function () {
    function Subscription(name) {
        this.Name = name;
        this.Callbacks = [];
        this.addCallback = function (fn, opt) {
            this.Callbacks.push(new XSockets.Callback(name, fn, opt));
        };
        this.removeCallback = function (ix) {
            this.Callbacks.splice(ix, 1);
        };
        this.fireCallback = function (message, cb, ix) {
            this.Callbacks[ix].fn(message);
            if (typeof (this.Callbacks[ix].state) === "object") {
                if (typeof (this.Callbacks[ix].state.options) !== "undefined" && typeof (this.Callbacks[ix].state.options.counter) !== "undefined") {
                    this.Callbacks[ix].state.options.counter.messages--;
                    if (this.Callbacks[ix].state.options.counter.messages === 0) {
                        if (typeof (this.Callbacks[ix].state.options.counter.completed) === 'function') {
                            this.Callbacks[ix].state.options.counter.completed();
                        }
                    }
                }
            }
        };
        this.fireCallbacks = function (message, cb) {
            for (var c = 0; c < this.Callbacks.length; c++) {
                this.fireCallback(message, cb, c);
            }
        };
    }
    return Subscription;
})();
XSockets.Subscriptions = (function () {
    function Subscriptions() {
        this.subscriptions = new Array();
        this.add = function (name, fn, opt) {
            /// <summary>
            ///     if a subscription with the same name exists the function will be
            ///     added to the callback list.
            ///     if the subscription does not exist a new one will be created.
            ///     
            ///     Returns the index of the callback added to be used if you only want to remove a specific callback.
            /// </summary>
            /// <param name="name" type="string">
            ///    Name of the subscription.
            /// </param>
            /// <param name="fn" type="function">
            ///    The callback function to add
            /// </param>
            name = name.toLowerCase();
            var storedSub = this.get(name);
            if (storedSub === null) {
                var sub = new XSockets.Subscription(name);
                sub.addCallback(fn, opt);
                this.subscriptions.push(sub);
                return 1;
            }
            storedSub.addCallback(fn, opt);
            return storedSub.Callbacks.length;
        };
        this.get = function (name) {
            /// <summary>
            ///     Returns the subscription and all its callbacks.
            ///     if not found null is returned.
            /// </summary>
            /// <param name="name" type="string">
            ///    Name of the subscription.
            /// </param> 
            if (typeof (name) === "undefined") return;
            name = name.toLowerCase();
            for (var i = 0; i < this.subscriptions.length; i++) {

                if (this.subscriptions[i].Name === name) {
                    return this.subscriptions[i];
                };
            }
            return null;
        };
        this.getAll = function () {
            /// <summary>
            ///     Returns all the subscriptions.        
            /// </summary>
            return this.subscriptions;
        };
        this.remove = function (name, ix) {
            /// <summary>
            ///     Removes a subscription with the matching name.
            ///     if ix of a callback is passed only the specific callback will be removed.
            ///     
            ///     Returns true if something was removed, false if nothing was removed.
            /// </summary>
            /// <param name="name" type="string">
            ///    Name of the subscription.
            /// </param>
            /// <param name="ix" type="number">
            ///    The index of the callback to remove (optional)
            /// </param>
            name = name.toLowerCase();
            for (var i = 0; i < this.subscriptions.length; i++) {
                if (this.subscriptions[i].Name === name) {
                    this.subscriptions[i].Callbacks.splice(ix, 1);
                    if (this.subscriptions[i].Callbacks.length === 0) this.subscriptions.splice(i, 1);
                    return true;
                }
            }
            return false;
        };
        this.fire = function (name, message, cb, ix) {
            /// <summary>
            ///     Triggers all callbacks on the subscription, or if ix is set only that callback will be fired.
            /// </summary>
            /// <param name="name" type="string">
            ///    Name of the subscription.
            /// </param>
            /// <param name="ix" type="number">
            ///    The index of the callback to trigger (optional)
            /// </param>
            if (typeof (name) === "undefined") return;
            name = name.toLowerCase();
            for (var i = 0; i < this.subscriptions.length; i++) {
                if (this.subscriptions[i].Name === name) {
                    if (ix === undefined) {
                        this.subscriptions[i].fireCallbacks(message, cb);
                    } else {
                        this.subscriptions[i].fireCallback(message, cb, ix);
                    }
                }
            }
        };
    };
    return Subscriptions;
})();
XSockets.Message = (function () {
    /// <summary>
    ///     Create a new Message
    /// </summary>
    /// <param name="event" type="string">
    ///     Name of the event
    /// </param>             
    /// <param name="object" type="object">
    ///     The message payload (JSON)
    /// </param>  
    function Message(event, object) {
        this.event = event;
        this.data = object;
        this.JSON = {
            event: event,
            data: JSON.stringify(object)
        };
    }
    Message.prototype.toString = function () {
        return JSON.stringify(this.JSON);
    };
    return Message;

})();
XSockets.BinaryMessage = (function () {
    BinaryMessage.prototype.stringToBuffer = function (str) {
        /// <summary>convert a string to a byte buffer</summary>
        /// <param name="str" type="String"></param>
        var i, len = str.length,
            arr = new Array(len);
        for (i = 0; i < len; i++) {
            arr[i] = str.charCodeAt(i) & 0xFF;
        }
        return new Uint8Array(arr).buffer;
    };
    BinaryMessage.prototype.appendBuffer = function (a, b) {
        /// <summary>Returns a new Uint8Array array </summary>
        /// <param name="a" type="arrayBuffer">buffer A</param>
        /// <param name="b" type="arrayBuffer">buffer B</param>
        var c = new Uint8Array(a.byteLength + b.byteLength);
        c.set(new Uint8Array(a), 0);
        c.set(new Uint8Array(b), a.byteLength);
        return c.buffer;
    };
    BinaryMessage.prototype.getHeader = function () {
        /// <summary>Get the Binary message header</summary>
        return this.header;
    };

    function BinaryMessage(message, arrayBuffer, cb) {
        /// <summary>Create a new XSockets.BinaryMessage</summary>
        /// <param name="message" type="Object">XSockets.Message</param>
        /// <param name="arrayBuffer" type="Object">buffer</param>
        /// <param name="cb">callback function to be invoved when BinaryMessage is created</param>
        if (!window.Uint8Array) throw ("Unable to create a XSockets.BinaryMessage, the browser does not support Uint8Array");

        if (arguments.length === 3) {

            var payload = message.toString();
            this.id = new XSockets.Utils.guid;
            this.header = new Uint8Array(XSockets.Utils.longToByteArray(payload.length));
            this.buffer = this.appendBuffer(this.appendBuffer(this.header, this.stringToBuffer(payload)), arrayBuffer);
            if (cb) cb(this);

        } else if (arguments.length === 2) {

            var header = new Uint8Array(message, 0, 8);
            byteArrayToLong = function (byteArray) {
                var value = 0;
                for (var i = byteArray.byteLength - 1; i >= 0; i--) {
                    value = (value * 256) + byteArray[i];
                }
                return value;
            };
            var payloadLength = byteArrayToLong(header);
            var offset = 8 + byteArrayToLong(header);
            var buffer = new Uint8Array(message, offset, message.byteLength - offset);
            var payload = new Uint8Array(message, 8, payloadLength);

            function ab2str(buf) {
                return String.fromCharCode.apply(null, new Uint16Array(buf));
            }
            arrayBuffer(JSON.parse(ab2str(payload)), buffer);


        }



    }



    return BinaryMessage;
})();
XSockets.WebSocket = (function () {

    function WebSocket(url, subprotocol, settings) {
        /// <summary></summary>
        /// <param name="url" type="Object"></param>
        /// <param name="subprotocol" type="Object"></param>
        /// <param name="settings" type="Object"></param>
        var self = this;
        this.id = XSockets.Utils.guid();

        this.subscriptions = new XSockets.Subscriptions();
        var parameters = function (p) {
            var str = "?";
            for (var key in p) {
                str += key + '=' + encodeURIComponent(p[key]) + '&';
            }
            str = str.slice(0, str.length - 1);
            return str;
        };

        this.dispatchEvent = function (eventName, message) {

            if (self.subscriptions.get(eventName) === null) {
                return;
            }
            if (typeof message === "string") {
                try {
                    message = JSON.parse(message);
                } catch (e) { }
            }
            self.subscriptions.fire(eventName, message, function (evt) { });
        };

        this.getClientType = function () {

            if (typeof (window.WebSocket) === "undefined") return "Fallback";
            return "WebSocket" in window && window.WebSocket.CLOSED > 2 ? "RFC6455" : "Hixie";
        };
        this.options = XSockets.Utils.extend({
            parameters: {},
            binaryType: "arraybuffer"
        }, (typeof (subprotocol) === "object" && !settings) ? subprotocol : settings);

        this.uri = XSockets.Utils.parseUri(url);

        if ('WebSocket' in window && window.WebSocket) {
            if (this.getClientType() === "Fallback") {
                this.webSocket = new window.WebSocket(url, subprotocol);
            } else {

                var storageGuid = window.localStorage.getItem("XSocketsClientStorageGuid" + self.uri.controller) !== null ?
                    window.localStorage.getItem("XSocketsClientStorageGuid" + self.uri.controller) : null;
                if (storageGuid !== null) {
                    self.uri.query.XSocketsClientStorageGuid = storageGuid;
                }
                url = self.uri.absoluteUrl + parameters(XSockets.Utils.extend(this.options.parameters, self.uri.query));
                this.webSocket = new window.WebSocket(url, subprotocol || "XSocketsNET");
                this.webSocket.binaryType = this.options.binaryType;
            }
        }
        if (this.webSocket !== null) {

            this.bind(XSockets.Events.open, function (data) {

                self.connection = XSockets.Utils.extend(data, {
                    clientType: self.getClientType()
                });
                window.localStorage.setItem("XSocketsClientStorageGuid" + self.uri.controller, data.StorageGuid);
                var chain = self.subscriptions.getAll();

                for (var e = 0; e < chain.length; e++) {
                    for (var c = 0; c < chain[e].Callbacks.length; c++) {
                        if (chain[e].Callbacks[c].state.ready === 0) {
                            if (chain[e].Callbacks[c].state.options)
                                if (chain[e].Name, chain[e].Callbacks[c].state.options.hasOwnProperty("skip")) {
                                    continue;
                                }
                            self.trigger(new XSockets.Message(XSockets.Events.pubSub.subscribe, {
                                Event: chain[e].Name,
                                Confirm: chain[e].Callbacks[c].state.confirm
                            }));
                        }
                    }
                }
                // If subscibes to bindings completed, dispatch a list of subscriptions
                self.dispatchEvent(XSockets.Events.bindings.completed, self.subscriptions.getAll());
                if (typeof (self.onopen) === "function") self.onopen(data);
            }, {
                skip: true
            });

            this.webSocket.onerror = function (err) {
                if (self.onerror) self.onerror(err);
                self.dispatchEvent(XSockets.Events.onError, err);
            };
            this.webSocket.onclose = function (msg) {
                if (self.onclose) self.onclose(msg);
                self.dispatchEvent('close', msg);
            };
            this.webSocket.onopen = function (msg) {
                self.dispatchEvent('open', msg);
            };
            this.webSocket.onmessage = function (message) {
                if (typeof message.data === "string") {
                    var msg = JSON.parse(message.data);
                    self.dispatchEvent(msg.event, msg.data);
                    if (msg.event === XSockets.Events.onError) {
                        if (typeof (self.onmessage) === "function") self.onmessage(message);
                        if (self.onerror) self.onerror(message.data, message);
                        self.dispatchEvent(XSockets.Events.onError, JSON.parse(message.data));
                    }

                } else {

                    if (self.onblob) self.onblob(message.data);
                    self.dispatchEvent(XSockets.Events.onBlob, message.data);
                }


            };
        }
    }
    WebSocket.prototype.readyState = function () {
        return this.webSocket.readyState;
    };
    WebSocket.prototype.onblob = undefined;

    WebSocket.prototype.onopen = undefined;
    WebSocket.prototype.onclose = undefined;
    WebSocket.prototype.onmessage = undefined;
    WebSocket.prototype.onerror = undefined;
    WebSocket.prototype.removeCallback = function (from, ix) {
        /// <summary>Remove a callback from a subscription by it's index</summary>
        /// <param name="name" type="Object">Name of the event</param>
        /// <param name="ix" type="Object"></param>
        var sub = from.toLowerCase();
        return this.subscriptions.remove(sub, ix);
    };
    WebSocket.prototype.channel = {};
    WebSocket.prototype.connection = {};
    WebSocket.prototype.close = function (fn) {
        /// <summary>
        ///     Close the current XSocket WebSocket instance and the underlaying WebSocket. Server will fire the XSockets.Event.close event
        //      Just add a subscription i.e ws.bind(XSockets.Event.close,function(){  });
        /// </summary>
        /// <param name="fn" type="function">
        ///    A function to execute when closed.
        /// </param>   
        this.trigger(XSockets.Events.connection.disconnect, {}, fn);
    };
    WebSocket.prototype.getSubscriptions = function () {
        /// <summary>
        ///     Get all subscriptions for the current Handler
        /// </summary>
        return this.subscriptions.subscriptions;
    };
    WebSocket.prototype.bind = function (event, fn, opts, callback) {
        /// <summary>
        ///     Establish a subscription on the XSocketsController connected.
        /// </summary>
        /// <param name="event" type="string">
        ///    Name unique name of the subscription (event)
        /// </param> 
        /// <param name="fn" type="function">
        ///    A function to execute each time the event (subscription) is triggered.
        /// </param>   
        /// <param name="options" type="object">
        ///   Options, connsult the documentation
        /// </param>
        /// <param name="callback" type="function">
        ///    A function to execute when completed, if provided the server will pass a confirm message when subscriptions is established
        /// </param>
        /// 
        var that = this;
        var o = {
            options: !(opts instanceof Function) ? opts : {},
            ready: this.webSocket.readyState,
            confirm: (callback || opts) instanceof Function
        };

        if (o.ready === 1) {
            this.trigger(new XSockets.Message(XSockets.Events.pubSub.subscribe, {
                Event: event,
                Confirm: o.confirm
            }));
        }
        if (fn instanceof Function) {
            this.subscriptions.add(event, fn, o);
        } else if (fn instanceof Array) {
            fn.forEach(function (cb) {
                that.subscriptions.add(event, cb, o);
            });
        }
        if (typeof (callback) === "function" || typeof (opts) === "function")
            this.subscriptions.add("__" + event, callback || opts, {
                options: {
                    ready: 2
                }
            });
        return this;
    };
    WebSocket.prototype.on = function (event, fn, opts, callback) {
        return this.bind(event, fn, opts, callback);
    };
    WebSocket.prototype.subscribe = function (event, fn, opts, callback) {
        return this.bind(event, fn, opts, callback);
    };
    WebSocket.prototype.many = function (event, count, fn, opts, callback) {
        /// <summary>
        ///    Establish a subscription on the XSocketsController connected.  unbinds when the subscriptions callback has fired specified number of (count) times.
        /// </summary>
        /// <param name="event" type="String">
        ///    Name of the event (subscription)
        /// </param>           
        /// <param name="count" type="Number">
        ///     Number of times to listen to this event (subscription)
        /// </param>           
        /// <param name="fn" type="Function">
        ///    A function to execute at the time the event is triggered the specified number of times.
        /// </param> 
        /// <param name="options" type="object">
        ///   event (subscriptions) options
        /// </param>
        /// <param name="callback" type="function">
        ///    A function to execute when completed, if provided the server will pass a confirm message when subscriptions is established
        /// </param>
        var that = this;
        this.bind(event, fn, XSockets.Utils.extend({
            counter: {
                messages: count,
                completed: function () {
                    that.unbind(event);
                }
            }
        }, opts), callback || opts);
        return this;
    };
    WebSocket.prototype.one = function (event, fn, opts, callback) {
        /// <summary>
        ///    Establish a subscription on the XSocketsController connected.  unbinds when the subscriptions callback has fired once (1)
        /// </summary>
        /// <param name="event" type="String">
        ///    Name of the event (subscription)
        /// </param>           
        /// <param name="fn" type="Function">
        ///    A function to trigger when executed once.
        /// </param>       
        /// <param name="options" type="object">
        ///   event (subscriptions) options
        /// </param>  
        var that = this;

        this.bind(event, fn, XSockets.Utils.extend({
            counter: {
                messages: 1,
                completed: function () {
                    that.unbind(event);
                }
            }
        }, opts), callback || opts);
        return this;
    };
    WebSocket.prototype.unbind = function (event, callback) {
        /// <summary>
        ///     Remove a subscription for the current client on the connected XSocketsController
        /// </summary>
        /// <param name="event" type="String">
        ///    Name of the event (subscription) to unbind.
        /// </param>           
        /// <param name="callback" type="function">
        ///    A function to execute when completed.
        /// </param>   

        if (this.subscriptions.remove(event)) {
            this.trigger(new XSockets.Message(XSockets.Events.pubSub.unsubscribe, {
                Event: event
            }));
        }
        if (callback && typeof (callback) === "function") {
            callback();
        }
        return this;
    };
    WebSocket.prototype.unsubscribe = function (event, callback) {
        return this.unbind(event, callback);
    };
    WebSocket.prototype.off = function (event, callback) {
        return this.unbind(event, callback);
    };
    WebSocket.prototype.trigger = function (event, json, callback) {
        /// <summary>
        ///      Trigger (Publish) a WebSocketMessage (event) to the current WebSocket Handler.
        /// </summary>
        /// <param name="event" type="string">
        ///     Name of the event (publish) 
        /// </param>                
        /// <param name="json" type="JSON">
        ///     JSON representation of the WebSocketMessage to trigger/send (publish)
        /// </param>
        /// <param name="callback" type="function">
        ///      A function to execute when completed. 
        /// </param>
        if (typeof (event) !== "object") {
            if (arguments.length !== 2 || typeof (json) !== "function") {
                if (arguments.length === 1) {
                    json = {};
                }
            } else {
                callback = json;
                json = {};
            }
        }

        if (typeof (event) !== "object") {
            event = event.toLowerCase();
            var message = new XSockets.Message(event, json);
            this.webSocket.send(message.toString());
            if (callback && typeof (callback) === "function") {
                callback();
            }
        } else {
            this.webSocket.send(event.toString());
            if (json && typeof (json) === "function") {
                json();
            }
        }
        return this;
    };
    WebSocket.prototype.publish = function (event, json, callback) {
        return this.trigger(event, json, callback);
    };
    WebSocket.prototype.emit = function (event, json, callback) {
        return this.trigger(event, json, callback);
    };
    WebSocket.prototype.send = function (payload) {
        /// <summary>
        ///     Send a message
        /// </summary>
        /// <param name="payload" type="object">
        ///     string / blob to send
        /// </param> 
        this.webSocket.send(payload);
        return this;
    };
    WebSocket.prototype.triggerBinary = function (bytes) {
        return this.send(bytes);
    };

    WebSocket.prototype.setProperty = function (name, value) {
        /// <summary>
        ///    Set a property on the connected controller
        /// </summary>
        /// <param name="name" type="string">
        ///     Name of the property to set
        /// </param> 
        /// <param name="value" type="object">
        ///     Value of the property (string,number,array,object)
        /// </param> 
        var property = "set_" + name;
        this.publish(property, typeof (value) === "object" ? value : {
            value: value
        });
    };

    WebSocket.prototype.setEnum = function (name, value) {
        /// <summary>
        ///    Set a property (Enum) on the connected controller
        /// </summary>
        /// <param name="name" type="string">
        ///     Name of the property to set
        /// </param> 
        /// <param name="value" type="object">
        ///     Value of the property (Enum)
        /// </param> 
        var property = "set_" + name;
        this.publish(property, value);
    };

    return WebSocket;
})();
XSockets.Channel = (function () {
    function Channel() {
        /// <summary></summary>
    }
    Channel.prototype.create = function (url, handler, parameters, cb) {
        /// <summary>
        ///      Create a new Channel (Private connection to the specified handler)
        /// </summary>
        /// <param name="url" type="string">
        ///     WebSocket URL
        /// </param>                
        /// <param name="handler" type="string">
        ///     The handler/controller to connect to 
        /// </param>
        /// <param name="parameters" type="object">
        ///     settings / options (parameters etc)
        /// </param>

        var _channelUri = XSockets.Utils.parseUri(url);
        var id = XSockets.Utils.guid();
        var channelUrl = _channelUri.absoluteUrl + "/" + id;

        var channelWs = new XSockets.WebSocket(channelUrl, handler, XSockets.Utils.extend(_channelUri.query, parameters));
        channelWs.channel = {
            Id: id,
            args: [channelUrl, handler, XSockets.Utils.extend(_channelUri.query, parameters)]
        };

        return channelWs;
    };
    Channel.prototype.connect = function (channel) {
        /// <summary>
        ///      Connect to a channel 
        /// </summary>
        /// <param name="channel" type="object">
        ///     Channel to connect to
        /// </param>                
        return new XSockets.WebSocket(channel.args[0], channel.args[1], channel.args[2]);
    };
    return Channel;
})();