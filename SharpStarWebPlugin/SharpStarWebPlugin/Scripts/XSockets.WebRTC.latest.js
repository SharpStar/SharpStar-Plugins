String.prototype.capitalize = function () {
    return this.replace(/(^|\s)([a-z])/g, function (m, p1, p2) {
        return p1 + p2.toUpperCase();
    });
};

var RTCPeerConnection = null;
var getUserMedia = null;
var attachMediaStream = null;
var reattachMediaStream = null;
var webrtcDetectedBrowser = null;
var webrtcDetectedVersion = null;

function trace(text) {
    // This function is used for logging.
    if (text[text.length - 1] == '\n') {
        text = text.substring(0, text.length - 1);
    }
    console.log((performance.now() / 1000).toFixed(3) + ": " + text);
}

if (navigator.mozGetUserMedia) {
    console.log("This appears to be Firefox");

    webrtcDetectedBrowser = "firefox";

    webrtcDetectedVersion =
             parseInt(navigator.userAgent.match(/Firefox\/([0-9]+)\./)[1], 10);

    // The RTCPeerConnection object.
    RTCPeerConnection = mozRTCPeerConnection;

    // The RTCSessionDescription object.
    RTCSessionDescription = mozRTCSessionDescription;

    // The RTCIceCandidate object.
    RTCIceCandidate = mozRTCIceCandidate;

    // Get UserMedia (only difference is the prefix).
    // Code from Adam Barth.
    getUserMedia = navigator.mozGetUserMedia.bind(navigator);

    // Creates iceServer from the url for FF.
    createIceServer = function (url, username, password) {
        var iceServer = null;
        var url_parts = url.split(':');
        if (url_parts[0].indexOf('stun') === 0) {
            // Create iceServer with stun url.
            iceServer = { 'url': url };
        } else if (url_parts[0].indexOf('turn') === 0) {
            if (webrtcDetectedVersion < 27) {
                // Create iceServer with turn url.
                // Ignore the transport parameter from TURN url for FF version <=27.
                var turn_url_parts = url.split("?");
                // Return null for createIceServer if transport=tcp.
                if (turn_url_parts[1].indexOf('transport=udp') === 0) {
                    iceServer = {
                        'url': turn_url_parts[0],
                        'credential': password,
                        'username': username
                    };
                }
            } else {
                // FF 27 and above supports transport parameters in TURN url,
                // So passing in the full url to create iceServer.
                iceServer = {
                    'url': url,
                    'credential': password,
                    'username': username
                };
            }
        }
        return iceServer;
    };

    // Attach a media stream to an element.
    attachMediaStream = function (element, stream) {
        console.log("Attaching media stream");
        element.mozSrcObject = stream;
        element.play();
    };

    reattachMediaStream = function (to, from) {
        console.log("Reattaching media stream");
        to.mozSrcObject = from.mozSrcObject;
        to.play();
    };

    // Fake get{Video,Audio}Tracks
    MediaStream.prototype.getVideoTracks = function () {
        return [];
    };

    MediaStream.prototype.getAudioTracks = function () {
        return [];
    };
} else if (navigator.webkitGetUserMedia) {
    console.log("This appears to be Chrome");

    webrtcDetectedBrowser = "chrome";
    webrtcDetectedVersion =
           parseInt(navigator.userAgent.match(/Chrom(e|ium)\/([0-9]+)\./)[2], 10);

    // Creates iceServer from the url for Chrome.
    createIceServer = function (url, username, password) {
        var iceServer = null;
        var url_parts = url.split(':');
        if (url_parts[0].indexOf('stun') === 0) {
            // Create iceServer with stun url.
            iceServer = { 'url': url };
        } else if (url_parts[0].indexOf('turn') === 0) {
            // Chrome M28 & above uses below TURN format.
            iceServer = {
                'url': url,
                'credential': password,
                'username': username
            };
        }
        return iceServer;
    };

    // The RTCPeerConnection object.
    RTCPeerConnection = webkitRTCPeerConnection;

    // Get UserMedia (only difference is the prefix).
    // Code from Adam Barth.
    getUserMedia = navigator.webkitGetUserMedia.bind(navigator);

    // Attach a media stream to an element.
    attachMediaStream = function (element, stream) {
        if (typeof element.srcObject !== 'undefined') {
            element.srcObject = stream;
        } else if (typeof element.mozSrcObject !== 'undefined') {
            element.mozSrcObject = stream;
        } else if (typeof element.src !== 'undefined') {
            element.src = URL.createObjectURL(stream);
        } else {
            console.log('Error attaching stream to element.');
        }
    };

    reattachMediaStream = function (to, from) {
        to.src = from.src;
    };
} else {
    console.log("Browser does not appear to be WebRTC-capable");
}

window.requestAnimFrame = (function () {
    return window.requestAnimationFrame ||
            window.webkitRequestAnimationFrame ||
            window.mozRequestAnimationFrame ||
            function (callback) {
                window.setTimeout(callback, 1000 / 60);
            };
})();

XSockets.PeerContext = function (guid, context) {
    this.PeerId = guid;
    this.Context = context;
};


XSockets.AudioAnalyser = (function () {
    function AudioAnalyser(stream, interval, cb) {
        var self = this;
        var buflen = 2048;
        var buf = new Uint8Array(buflen);
        function autoCorrelate(buf, sampleRate) {
            var minSamples = 4;
            var maxSamples = 1000;
            var size = 1000;
            var bestOffset = -1;
            var bestCorrelation = 0;
            var rms = 0;
            var currentPitch = 0;
            if (buf.length < (size + maxSamples - minSamples))
                return;  // Not enough data
            for (var i = 0; i < size; i++) {
                var val = (buf[i] - 128) / 128;
                rms += val * val;
            }
            rms = Math.sqrt(rms / size);
            for (var offset = minSamples; offset <= maxSamples; offset++) {
                var correlation = 0;
                for (var i = 0; i < size; i++) {
                    correlation += Math.abs(((buf[i] - 128) / 128) - ((buf[i + offset] - 128) / 128));
                }
                correlation = 1 - (correlation / size);
                if (correlation > bestCorrelation) {
                    bestCorrelation = correlation;
                    bestOffset = offset;
                }
            }
            if ((rms > 0.01) && (bestCorrelation > 0.01)) {
                currentPitch = sampleRate / bestOffset;
                var result = {
                    Confidence: bestCorrelation,
                    CurrentPitch: currentPitch,
                    Fequency: sampleRate / bestOffset,
                    RMS: rms,
                    TimeStamp: new Date()
                };
                if (self.onResult)
                    self.onResult(result);
                self.analyzerResult.unshift(result);
            }
        }
        function pitcher() {
            self.analyser.getByteTimeDomainData(buf);
            autoCorrelate(buf, self.audioContext.sampleRate);
        }
        this.analyzerResult = [];
        this.isSpeaking = false;
        this.onResult = undefined;
        this.onAnalysis = undefined;
        this.audioContext = new AudioContext();
        var mediaStreamSource = this.audioContext.createMediaStreamSource(stream);
        this.analyser = this.audioContext.createAnalyser();

        mediaStreamSource.connect(this.analyser);
        this.analyser.smoothingTimeConstant = 0.8;
        this.analyser.fftSize = 2048;

        window.setInterval(function () {
            pitcher();
        }, (interval || 1000) / 10);

        setInterval(function () {
            if (self.analyzerResult.length > 5) {
                // How old is the latest confident audio analyze?
                var now = new Date();
                var result = self.analyzerResult[0];
                var lastKnown = new Date(self.analyzerResult[0].TimeStamp.getTime());
                if ((now - lastKnown) > 1000) {
                    if (self.isSpeaking) {
                        result.IsSpeaking = false;
                        if (self.onAnalysis) self.onAnalysis(result);
                        self.analyzerResult = [];
                    }
                    self.isSpeaking = false;
                } else {
                    if (!self.isSpeaking) {
                        result.IsSpeaking = true;
                        if (self.onAnalysis) self.onAnalysis(result);
                    }
                    self.isSpeaking = true;
                }
            }
        }, 250);
        if (cb) cb();
    }
    return AudioAnalyser;

})();

XSockets.WebRTC = function (ws, settings) {
    var isAudioMuted = false;
    var self = this;
    var localStreams = [];
    var subscriptions = new XSockets.Subscriptions();
    this.PeerConnections = {};
    this.DataChannels = undefined;
    var defaults = {
        iceServers: [{
            "url": "stun:stun.l.google.com:19302"
        }]
        ,
        sdpConstraints: {
            optional: [],
            mandatory: {
                OfferToReceiveAudio: true,
                OfferToReceiveVideo: true
            }
        },
        streamConstraints: {
            mandatory: {},
            optional: []
        },
        sdpExpressions: []
    };




    var options = XSockets.Utils.extend(defaults, settings);;


    this.bind = function (event, fn, opts, callback) {
        subscriptions.add(event, fn);
        if (callback && typeof (callback) === "function") {
            callback();
        }
        return this;
    };
    this.unbind = function (event, callback) {
        subscriptions.remove(event);
        if (callback && typeof (callback) === "function") {
            callback();
        }
        return this;
    };
    this.dispatch = function (eventName, message, obj) {
        var _eventName = "on" + eventName;
        
        if (this.hasOwnProperty(_eventName)) {
            self[_eventName](message);
        }
        if (subscriptions.get(eventName) === null) {
            return;
        }
        if (typeof message === "string") {
            message = JSON.parse(message);
        }
        subscriptions.fire(eventName, message, function () { });
    };



    this.muteAudio = function (cb) {
        /// <summary>Toggle mute on all local streams</summary>
        /// <param name="cb" type="Object">function to be invoked when toggled</param>
        localStreams.forEach(function (a, b) {
            var audioTracks = a.getAudioTracks();

            if (audioTracks.length === 0) {
                return;
            }
            if (isAudioMuted) {
                for (i = 0; i < audioTracks.length; i++) {
                    audioTracks[i].enabled = true;
                }
            } else {
                for (i = 0; i < audioTracks.length; i++) {
                    audioTracks[i].enabled = false;
                }
            }
        });
        isAudioMuted = !isAudioMuted;
        if (cb) cb(isAudioMuted);
    };

    this.hasStream = function () {
        /// <summary>Determin of there is media streams attach to the local peer</summary>
        return localStreams.length > 0;
    };


    this.leaveContext = function () {
        /// <summary>Leave the current context (hang up on all )</summary>
        ws.publish("leaveContext", {

        });
        return this;
    };
    this.changeContext = function (contextGuid) {
        /// <summary>Change context on broker</summary>
        /// <param name="contextGuid" type="Object">Unique identifer of the context to 'join'</param>
        ws.publish("changecontext", {
            context: contextGuid
        });
        return this;
    };




    this.getLocalStreams = function () {
        /// <summary>Get local streams</summary>
        return localStreams;
    };
    this.removeStream = function (id, fn) {
        /// <summary>Remove the specified local stream</summary>
        /// <param name="id" type="Object">Id of the media stream</param>
        /// <param name="fn" type="Object">callback function invoked when remote peers notified and stream removed.</param>
        localStreams.forEach(function (stream, index) {
            if (stream.id === id) {

                localStreams[index].stop();
                localStreams.splice(index, 1);
                for (var peer in self.PeerConnections) {
                    //  self.PeerConnections[peer].Connection.removeStream(_stream);
                    //createOffer({
                    //    PeerId: peer
                    //});
                    ws.publish("removestream", {
                        recipient: peer,
                        streamId: id
                    });
                    if (fn) fn(id);
                }
            }
        });
    };

    this.userMediaConstraints = {
        qvga: function (audio) {
            return {
                video: {
                    mandatory: {
                        maxWidth: 320,
                        maxHeight: 180
                    }
                },
                audio: typeof (audio) !== "boolean" ? false : audio
            };
        },
        vga: function (audio) {
            return {
                video: {
                    mandatory: {
                        maxWidth: 640,
                        maxHeight: 360
                    }
                },
                audio: typeof (audio) !== "boolean" ? false : audio
            };
        },
        hd: function (audio) {

            return {
                video: {
                    mandatory: {
                        minWidth: 1280,
                        minHeight: 720
                    }
                },
                audio: typeof (audio) !== "boolean" ? false : audio
            };
        },
        create: function (w, h, audio) {
            return {
                video: {
                    mandatory: {
                        minWidth: w,
                        minHeight: h
                    }
                },
                audio: typeof (audio) !== "boolean" ? false : audio
            };
        },
        screenSharing: function () {
            return {
                video: {
                    mandatory: {
                        chromeMediaSource: 'screen'
                    }
                }
            };
        }
    };

    this.getRemotePeers = function () {
        /// <summary>Returns a list of remotePeers (list of id's)</summary>
        var ids = [];
        for (var peer in self.PeerConnections)
            ids.push(peer);
        return ids;

    };

    this.refreshStreams = function (id, fn) {
        /// <summary>Reattach streams and renegotiate</summary>
        /// <param name="id" type="Object">PeerConnection id</param>
        /// <param name="fn" type="Object">callback that will be invoked when completed.</param>
        localStreams.forEach(function (stream, index) {
            self.PeerConnections[id].Connection.removeStream(localStreams[index]);
        });
        createOffer({
            PeerId: id
        });

        if (fn) fn(id);
    };

    this.addLocalStream = function (stream, cb) {
        var index = localStreams.push(stream);
        // Check it there is PeerConnections 
        ws.trigger("AddStream", {
            streamId: stream.id,
            description: ""
        });
        self.dispatch(XSockets.WebRTC.Events.onlocalStream, stream);
        if (cb) cb(stream, index);
        return this;
    };

    this.removePeerConnection = function (id, fn) {
        /// <summary>Remove the specified Peerconnection</summary>
        /// <param name="id" type="guid">Id of the PeerConnection. Id is the PeerId of the actual PeerConnection</param>
        /// <param name="fn" type="function">callback function invoked when the PeerConnection is removed</param>
        //ws.publish("peerconnectiondisconnect", {
        //    Recipient: id,
        //    Sender: self.CurrentContext.PeerId
        //});
        if (self.PeerConnections[id] !== undefined) {
            try {
                self.PeerConnections[id].Connection.close();
                self.dispatch(XSockets.WebRTC.Events.onPeerConnectionLost, {
                    PeerId: id
                });
            } catch (err) {

            }

        };
        delete self.PeerConnections[id];
        if (fn) fn();
    };

    this.getUserMedia = function (constraints, success, error) {

        /// <summary>get a media stream</summary>
        /// <param name="userMediaSettings" type="Object">connstraints. i.e .usersdpConstraints.hd()</param>
        /// <param name="success" type="Object">callback function invoked when media stream captured</param>
        /// <param name="error" type="Object">callback function invoked on faild to get the media stream </param>
        window.getUserMedia(constraints, function (stream) {
            localStreams.push(stream);
            ws.trigger("AddStream", {
                streamId: stream.id,
                description: ""
            });
            self.dispatch(XSockets.WebRTC.Events.onlocalStream, stream);
            if (success && typeof (success) === "function") success(self.CurrentContext);
        }, function (err) {
            if (error && typeof (error) === "function") error(err);
        });
        return this;
    };

    // DataChannels

    this.addDataChannel = function (dc) {
        /// <summary>Add a XSockets.WebRTC.DataChannel. Channel will be offered to remote peers</summary>
        /// <param name="dc" type="Object">XSockets.WebRTC.DataChannel to add</param>
        /// <param name="success" type="Object">callback function invoked when added.</param>
        this.DataChannels = this.DataChannels || {};
        if (!this.DataChannels.hasOwnProperty(dc.name)) {
            this.DataChannels[dc.name] = dc;
        } else {
            throw "A RTCDataChannel named '" + dc.Name + "' already exists and cannot be created.";
        }



    };
    this.removeDataChannel = function (name, cb) {
        /// <summary>Remove a Sockets.WebRTC.DataChannel </summary>
        /// <param name="name" type="Object">name of the XSockets.WebRTC.DataChannel to remove from offers</param>
        /// <param name="success" type="Object">callback function invoked when removed</param>

        if (this.DataChannels.hasOwnProperty(name)) {
            delete this.DataChannels[name];
            // remove delegates from peers..
            for (var pc in this.PeerConnections) {
                this.PeerConnections[pc].RTCDataChannels[name].close();
                delete this.PeerConnections[pc].RTCDataChannels[name];
            }
        } else {
            throw "A RTCDataChannel named '" + name + "' does not exists.";
        }
    };


    // Private methods
    var rtcPeerConnection = function (configuration, peerId) {
       
        var that = this;
        this.PeerId = peerId;
        this.RTCDataChannels = {};
        if (typeof (self.DataChannels) === "object" && configuration.sdpConstraints.optional.filter(function (option) { return option.RtpDataChannels; }).length === 0) {
            configuration.sdpConstraints.optional.push({ RtpDataChannels: true });
        }
        this.Connection = new RTCPeerConnection({ iceServers: configuration.iceServers }, configuration.sdpConstraints);
        this.Connection.oniceconnectionstatechange = function (data) {
            //   console.log("oniceconnectionstatechange", data);
        };
        this.Connection.onnegotiationneeded = function (data) {
            // console.log("onnegotiationneeded", data);
        };
        this.Connection.onremovestream = function (data) {
            //console.log("onremovestream", data);
        };
        this.Connection.onsignalingstatechange = function (data) {
            //console.log("onsignalingstatechange", data);
        };
        // If there is dataChannels attach, offer em
        for (var dc in self.DataChannels) {
            var dataChannel = self.DataChannels[dc];
            this.RTCDataChannels[dataChannel.name] = this.Connection.createDataChannel(dataChannel.name, { reliable: false });
            this.RTCDataChannels[dataChannel.name].onmessage = function (data) {
                var obj = JSON.parse(data.data).JSON;
                dataChannel.subscriptions.fire(obj.event, { peerId: that.PeerId, message: JSON.parse(obj.data) }, function () { });
            };
            this.RTCDataChannels[dataChannel.name].onopen = function (event) {
                //console.log("dataChannel open ", dataChannel.name, data);
                if (dataChannel.onopen) dataChannel.onopen(that.PeerId, event);
            };
            this.RTCDataChannels[dataChannel.name].closed = function (event) {
                if (dataChannel.onclose) dataChannel.onclose(that.PeerId, event);
            };
            dataChannel.onpublish = function (topic, data) {
                var message = new XSockets.Message(topic, data);
                for (var p in self.PeerConnections) {
                    self.PeerConnections[p].RTCDataChannels[dataChannel.name].send(JSON.stringify(message));
                }
            };
            dataChannel.onpublishTo = function (peerId,topic, data) {
                var message = new XSockets.Message(topic, data);
                if(self.PeerConnections[peerId])
                    self.PeerConnections[peerId].RTCDataChannels[dataChannel.name].send(JSON.stringify(message));
            };
        }
        this.Connection.onaddstream = function (event) {
            self.dispatch(XSockets.WebRTC.Events.onRemoteStream, {
                PeerId: that.PeerId,
                stream: event.stream
            });
        };
        this.Connection.onicecandidate = function (event) {
            if (event.candidate) {
                var candidate = {
                    type: 'candidate',
                    label: event.candidate.sdpMLineIndex,
                    id: event.candidate.sdpMid,
                    candidate: event.candidate.candidate
                };
                ws.publish("contextsignal", {
                    sender: self.CurrentContext.PeerId,
                    recipient: that.PeerId,
                    message: JSON.stringify(candidate)
                });
            }
        };
        self.dispatch(XSockets.WebRTC.Events.onPeerConnectionCreated, { PeerId: that.PeerId });
    };
    var createOffer = function (peer) {
        if (!peer) return;

        //if (self.PeerConnections[peer.PeerId]) {
        self.PeerConnections[peer.PeerId] = new rtcPeerConnection(options, peer.PeerId);
        localStreams.forEach(function (a, b) {

            self.PeerConnections[peer.PeerId].Connection.addStream(a, options.streamConstraints);
        });
        self.PeerConnections[peer.PeerId].Connection.createOffer(function (localDescription) {
            options.sdpExpressions.forEach(function (expr, b) {
                localDescription.sdp = expr(localDescription.sdp);
            }, function (failue) {
                console.log(failue);
            });
            self.PeerConnections[peer.PeerId].Connection.setLocalDescription(localDescription);
            ws.publish("contextsignal", {
                Sender: self.CurrentContext.PeerId,
                Recipient: peer.PeerId,
                Message: JSON.stringify(localDescription)
            });
        }, null, options.sdpConstraints);
        //}
    };
    self.bind("connect", function (peer) {
        createOffer(peer);
    });
    self.bind("candidate", function (event) {
        var candidate = JSON.parse(event.Message);
        if (!self.PeerConnections[event.Sender]) return;
        self.PeerConnections[event.Sender].Connection.addIceCandidate(new RTCIceCandidate({
            sdpMLineIndex: candidate.label,
            candidate: candidate.candidate
        }));
    });
    self.bind("answer", function (event) {
        self.dispatch(XSockets.WebRTC.Events.onAnswer, {
            PeerId: event.Sender
        });
        self.PeerConnections[event.Sender].Connection.setRemoteDescription(new RTCSessionDescription(JSON.parse(event.Message)));
    });
    self.bind("offer", function (event) {
        self.dispatch(XSockets.WebRTC.Events.onOffer, {
            PeerId: event.Sender
        });
        self.PeerConnections[event.Sender] = new rtcPeerConnection(options, event.Sender);
        self.PeerConnections[event.Sender].Connection.setRemoteDescription(new RTCSessionDescription(JSON.parse(event.Message)));



        localStreams.forEach(function (a, b) {
            self.PeerConnections[event.Sender].Connection.addStream(a, options.streamConstraints);
        });

        self.PeerConnections[event.Sender].Connection.createAnswer(function (description) {

            self.PeerConnections[event.Sender].Connection.setLocalDescription(description);
            options.sdpExpressions.forEach(function (expr, b) {

                description.sdp = expr(description.sdp);
            }, function (failure) {
                // dispatch the error
            });
            var answer = {
                Sender: self.CurrentContext.PeerId,
                Recipient: event.Sender,
                Message: JSON.stringify(description)
            };
            ws.publish("contextsignal", answer);
        }, null, options.sdpConstraints);

    });
    ws.subscribe("contextcreated", function (context) {
        self.CurrentContext = new XSockets.PeerContext(context.PeerId, context.Context);
        self.dispatch(XSockets.WebRTC.Events.onContextCreated, context);
    }, function () {
        ws.publish('GetContext');
    });
    ws.subscribe("contextsignal", function (signal) {
        var msg = JSON.parse(signal.Message);
        self.dispatch(msg.type, signal);
    });
    ws.subscribe("contextchanged", function (change) {
        self.dispatch(XSockets.WebRTC.Events.onContextChange, change);
    });
    ws.subscribe("contextconnect", function (peers) {
        peers.forEach(function (peer) {
            self.dispatch("connect", peer);
            self.dispatch(XSockets.WebRTC.Events.onPeerConnectionStarted, peer);
        });
    });
    ws.subscribe("peerconnectiondisconnect", function (peer) {
        if (self.PeerConnections[peer.Sender] !== undefined) {
            self.PeerConnections[peer.Sender].Connection.close();
            self.dispatch(XSockets.WebRTC.Events.onPeerConnectionLost, {
                PeerId: peer.Sender
            });
            delete self.PeerConnections[peer.Sender];
        }
    });
    ws.subscribe("streamadded", function (event) {
        self.dispatch(XSockets.WebRTC.Events.onLocalStreamCreated, event);
    });
    ws.subscribe("streamremoved", function (event) {
        self.dispatch(XSockets.WebRTC.Events.onRemoteStreamLost, {
            PeerId: event.Sender,
            StreamId: event.StreamId
        });
    });
    ws.subscribe("peerconnectionlost", function (peer) {
      
        self.dispatch(XSockets.WebRTC.Events.onPeerConnectionLost, {
            PeerId: peer.PeerId
        });
        if (self.PeerConnections[peer.PeerId] !== undefined) {
            self.PeerConnections[peer.PeerId].Connection.close();
            delete self.PeerConnections[peer.PeerId];
        };


    });





};


XSockets.WebRTC.DataChannel = (function () {
    function DataChannel(name) {
        var self = this;
        this.subscriptions = new XSockets.Subscriptions();
        this.name = name;
        this.subscribe = function (topic, cb) {
            self.subscriptions.add(topic, cb);
        };
        this.publish = function (topic, data, cb) {
            if (!self.onpublish) return;
            self.onpublish(topic, data);
            if (cb) cb(data);
        };
        this.publishTo = function(peerId,topic, data, cb) {

        };
        this.unsubscribe = function (topic, cb) {
            self.subscriptions.remove(topic);
            if (cb) cb();
        };
        this.onbinary = undefined;
        this.onpublish = undefined;
        this.onclose = undefined;
        this.onopen = undefined;
    }
    return DataChannel;
})();


XSockets.WebRTC.CallManager = function (ws, settings) {
    var events = settings.events;
    this.call = function (recipient) {
        ws.trigger("offercontext", {
            PeerId: recipient.PeerId
        });
    };

    this.acceptCall = function (call) {
        events.onAcceptCall(call);
    };
    this.denyCall = function (recipient) {
        ws.trigger("denycontext", {
            PeerId: recipient.PeerId
        });
    };
    this.endCall = function () {
        ws.trigger("leavecontext", {});
    };
    ws.subscribe("contextoffer", events.onCall);
    ws.subscribe("contextdeny", events.onDenyCall);
};

XSockets.WebRTC.Events = {
    onlocalStream: "localstream",
    onRemoteStream: "remotestream",
    onRemoteStreamLost: "removestream",
    onLocalStreamCreated: "streamadded",
    onContextChange: "contextchanged", // Fires when the current context changes
    onContextCreated: "contextcreated", // Fires when a client recives a new context
    onPeerConnectionStarted: "peerconnectionstarted", // Fires when a new RTCPeerConnection is initialized
    onPeerConnectionCreated: "peerconnectioncreated", // Fires when a new RTCPeerConnection is created
    onPeerConnectionLost: "peerconnectionlost", // Fires when a RTCPeerConnection is lost
    onDataChannelOpen: "datachannelopen", // Fires when a datachannel is open
    onDataChannelClose: "datachannelclose", // Fires when a datachannes is closed
    onOffer: 'sdoffer',
    onAnswer: 'sdanswer'

};