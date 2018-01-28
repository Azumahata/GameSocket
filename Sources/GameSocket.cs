using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Net;

public class GameSocket<TPacketPacker> : IDisposable
    where TPacketPacker : IPacketPacker, new() {
	string host;
	WebSocket ws;
	TPacketPacker packer;

	Dictionary<uint, Request> requests;
	Queue<Request> roundTrips;

	EventHandler<Packet> onServerPush;

	protected class Request {
		public readonly Packet packet;
		public readonly EventHandler<Packet> callback;
		public readonly Action<bool> completed;
		public readonly bool isRoundTrip;

		public Request(Packet _packet,
						EventHandler<Packet> _callback = null,
						Action<bool> _completed        = null,
						bool _isRoundTrip              = false
		) {
			packet      = _packet;
			callback    = _callback;

			completed = _completed;
			if (completed == null) {
				completed = (_c) => {};
			}

			isRoundTrip = _isRoundTrip;
		}
	}

	public GameSocket(string _host) {
		host   = _host;
		ws     = new WebSocket(_host);
		packer = new TPacketPacker();
		Init();
	}

	void Init() {
		ws.Compression = CompressionMethod.Deflate;

		requests   = new Dictionary<uint, Request>();
		roundTrips = new Queue<Request>();

		RegistReceive();

		// Regist Debug Message
		RegiestOnOpen((sender, e) => { Debug.Log("WebSocket Open"); });
		RegistOnError((sender, e) => { Debug.LogError("WebSocket Error Message: " + e.Exception + "\n" + e.Message); });
		RegistOnClose((sender, e) => { Debug.Log("WebSocket Close"); });
	}

	public void Connect() { ws.ConnectAsync(); }
	public void Close() {
		if (IsClosing() || IsClosed()) return;
		ws.CloseAsync();
		requests   = new Dictionary<uint, Request>();
		roundTrips = new Queue<Request>();
	}

	public void Dispose() {
		Close();
		ws = null;
	}

	public void RegiestOnOpen(EventHandler _onOpen) { ws.OnOpen += _onOpen; }
	public void UnregistOnOpen(EventHandler _onOpen) { ws.OnOpen -= _onOpen; }
	public void RegistOnError(EventHandler<ErrorEventArgs> _onError) { ws.OnError += _onError; }
	public void UnregistOnError(EventHandler<ErrorEventArgs> _onError) { ws.OnError -= _onError; }
	public void RegistOnClose(EventHandler<CloseEventArgs> _onClose) { ws.OnClose += _onClose; }
	public void UnregiseOnClose(EventHandler<CloseEventArgs> _onClose) { ws.OnClose -= _onClose; }
	public void RegistOnServerPush(EventHandler<Packet> _onPush) { onServerPush += _onPush; }
	public void UnregistOnServerPush(EventHandler<Packet> _onPush) { onServerPush -= _onPush; }

	public void ImmidiateSend(string _action, object _payload, EventHandler<Packet> _callback = null, Action<bool> _completed = null) {
		if (!IsOpen()) return;
		var packet = PacketFactory.CreatePacket(_action, _payload);
		var request = new Request(packet, _callback, _completed , false);
		Send(request);
	}

	public void SequencialSend(string _action, object _payload, EventHandler<Packet> _callback, Action<bool> _completed = null) {
		if (!IsOpen()) return;
		var packet =  PacketFactory.CreatePacket(_action, _payload);
		var request = new Request(packet, _callback, _completed, true);
		roundTrips.Enqueue(request);
		if (roundTrips.Peek() == request) {
			Send(request);
		}
	}

	void Send(Request _request) {
		requests.Add(_request.packet.sequence, _request);
		ws.SendAsync(packer.Pack(_request.packet), _request.completed);
	}

	void RegistReceive() {
		ws.OnMessage += (sender, e) => {
			Packet response = packer.Unpack(e.RawData);
			Debug.Log(
				"WebSocket Message Type: , Data: " + e.Data
				+ "\nseq:"    + response.sequence
				+ "\npayload" + response.payload
			);

			if (response.IsServerPush()) {
				onServerPush(host, response);
				return;
			}

			Request request;
			if (!requests.TryGetValue(response.sequence, out request)) {
				Debug.LogError("request was losted or connection closed."); // BUG?
				return;
			}
			requests.Remove(response.sequence);

			if (request.isRoundTrip) {
				if (roundTrips.Peek() != request) {
					Debug.LogError("Bug Queue was borken."); // BUG
					return;
				}
				roundTrips.Dequeue();
				if (roundTrips.Count > 0) {
					Send(roundTrips.Peek());
				}
			}
			request.callback(request.packet, response);
		};
	}

	public bool IsConencting() { return ws.ReadyState == WebSocketState.Connecting; }
	public bool IsOpen() { return ws.ReadyState == WebSocketState.Open; }
	public bool IsClosing() { return ws.ReadyState == WebSocketState.Closing; }
	public bool IsClosed() { return ws.ReadyState == WebSocketState.Open; }
}
