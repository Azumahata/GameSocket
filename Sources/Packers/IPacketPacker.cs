using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPacketPacker {
	byte[] Pack(Packet _packet);
	Packet Unpack(byte[] _data);
}