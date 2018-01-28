using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PacketFactory {
	static uint sequence = 0;
	static public Packet CreatePacket(string _action, object _payload) {
		if (sequence == uint.MaxValue) {
			sequence = 0;
		}

		var header = new Dictionary<string, object>() {
			{ "seq", ++sequence },
			{ "act",    _action }
		};

		return new Packet(header, _payload);
	}
}
