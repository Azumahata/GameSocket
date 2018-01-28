using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MsgPack;
using MsgPack.Serialization;

public class MessagePackPacker : IPacketPacker {
	SerializationContext context;
	public MessagePackPacker() {
		context = new SerializationContext();
		context.SerializationMethod = SerializationMethod.Map;
	}

	public byte[] Pack(Packet _packet) {
		byte[] data;
		var serializer = MessagePackSerializer.Get<Dictionary<string, object>>(context);

		using (MemoryStream stream = new MemoryStream()) {
			serializer.Pack(stream, new Dictionary<string, object>() {
				{ "header",  _packet.header },
				{ "payload", _packet.payload }
			});
		
			data = new byte[(int)stream.Length];
			stream.Position = 0;   
			stream.Read(data, 0, (int)stream.Length);
		}
		
		return data;
	}

	public Packet Unpack(byte[] _data) {
		MessagePackObject rawObject;
		var serializer = MessagePackSerializer.Get<Dictionary<string, object>>(context);
		using (MemoryStream stream = new MemoryStream(_data)) {
			rawObject = Unpacking.UnpackObject(stream);
		}
		
		var response = rawObject.AsDictionary();
		MessagePackObjectDictionary rHeader  = response["header"].AsDictionary();
		MessagePackObject           rPayload = response["payload"];

		var header = new Dictionary<string, object>() {
			{ "seq", rHeader["seq"].AsUInt32() }
		};

		return new Packet(header, rPayload);
	}
}
