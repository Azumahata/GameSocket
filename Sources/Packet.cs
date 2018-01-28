using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Packet {
	public readonly uint sequence;
	public readonly Dictionary<string, object> header;
	public readonly object payload;

	const uint SERVER_PUSH = 0;

	public Packet(Dictionary<string, object> _header, object _payload) {
		header      = _header;
		payload     = _payload;

		object seq = null;
		if (header.TryGetValue("seq", out seq)) {
			sequence = (uint)seq;
		} else {
			sequence = SERVER_PUSH;
		}
	}

	public bool IsServerPush() {
		return sequence == SERVER_PUSH;
	}
}
