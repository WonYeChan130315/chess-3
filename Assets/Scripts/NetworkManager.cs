using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks {
    public TMP_InputField roomInput;

    public GameObject lobbyGroup;
    public GameObject gameGroup;

    public void Join() {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() {
        PhotonNetwork.JoinOrCreateRoom(roomInput.name, new RoomOptions { MaxPlayers = 2 }, null);
    }

    public override void OnJoinedRoom() {
        lobbyGroup.SetActive(false);
        gameGroup.SetActive(true);
    }
}
