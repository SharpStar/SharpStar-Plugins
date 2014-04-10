if ("WebSocket" in window === false) {
    window.WebSocket = (function () {
        var self;
        function WebSocket(url) {
            this.client = {
            };
            self = this;
            $.getJSON("/API/XSocketsWebApi?url=" + url).done(
                function (connection) {
                    self.onopen(connection.data);
                    self.client = JSON.parse(connection.data);
                    listner(self.client);
                    self.readyState = 1;
                    self.onmessage(new self.MessageEvent(connection));
                }
            );
        }
        WebSocket.prototype.MessageEvent = function (data) {
            return {
                type: "message",
                data: JSON.stringify(data)
            };
        };
        WebSocket.prototype._bind = function (evt) {
            $.get("/API/XSocketsWebApi", { c: self.client.ClientGuid, e: evt }).done(function () {
            });
        };
        WebSocket.prototype.readystate = 0;
        WebSocket.prototype.send = function (data) {
            
            var msg = JSON.parse(data);
            if (msg.event == XSockets.Events.open) return;
            if (msg.event == XSockets.Events.pubSub.unsubscribe) {
                $.get("/API/XSocketsWebApi", { client: self.client.ClientGuid, action: XSockets.Events.pubSub.unsubscribe, data: JSON.parse(msg.data).Event }).done(function () {
                });
            } else if (msg.event == XSockets.Events.pubSub.subscribe) {
                $.get("/API/XSocketsWebApi", { client: self.client.ClientGuid, action: XSockets.Events.pubSub.subscribe, data: JSON.parse(msg.data).Event }).done(function () {
                });
            } else {
                $.post("/API/XSocketsWebApi", { Client: self.client.ClientGuid, Json: JSON.stringify(msg) }).done(function (d) {
                });
            }
        };
        var listner = function (c) {
            return $.post("/API/XSocketsWebApi?client=" + c.ClientGuid).done(function (result) {
                var messages = JSON.parse(result) || [];
                $.each(messages, function () {
                    self.onmessage(new self.MessageEvent(this));
                });
                listner(self.client);
            });
        };
        WebSocket.prototype.onmessage = function (data) {
        };
        WebSocket.prototype.onopen = function (data) {
        };
        return WebSocket;
    })();
}