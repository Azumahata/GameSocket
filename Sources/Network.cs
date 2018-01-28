using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MsgPack;

public class Network : MonoBehaviour {
	GameSocket<MessagePackPacker> ws;

    void Start()
    {
		ws = new GameSocket<MessagePackPacker>("ws://localhost:8080/");
        // ws.RegistOnServerPush((_sender, _packet) => {
        //     Debug.Log("Server Pushed");
        // });
    }

    void Update()
    {

        if (Input.GetKeyUp("o")) {
            ws.Connect();
        }
        if (Input.GetKeyDown("s")) {
            ws.SequencialSend("test", new Dictionary<string, object>() {
                { "msg", "Sequencial Test Message"}
            }, (_request, _response) => {
                var payload = (MessagePackObject)_response.payload;
                Debug.Log("callback:" + payload.AsDictionary()["msg"]);
            }, (_completed) => {
                Debug.Log("Sequencial Test Message send complete");
            });
        }
        if (Input.GetKeyUp("i")) {
            ws.ImmidiateSend("test", new Dictionary<string, object>() {
                { "msg", "Immidiate Test Message"}
            }, (_request, _response) => {
                var payload = (MessagePackObject)_response.payload;
                Debug.Log("callback:" + payload.AsDictionary()["msg"]);
            }, (_completed) => {
                Debug.Log("Immidiate Test Message send complete");
            });
        }
        if (Input.GetKeyUp("c")) {
            ws.Close();
        }
    }

    void OnDestroy() {
		ws.Dispose();
    }
}
