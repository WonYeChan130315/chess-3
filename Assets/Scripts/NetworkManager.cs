using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks {
    public TMP_InputField roomInput;
    public TextMeshProUGUI roomNameTxt;

    public GameObject lobbyGroup;
    public GameObject gameGroup;

    bool connected = false;

    void Awake() {
        PhotonNetwork.JoinLobby();
    }

    public void Join() {
        if (connected) return;

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() {
        connected = true;
        
        PhotonNetwork.JoinOrCreateRoom(roomInput.text, new RoomOptions { MaxPlayers = 2 }, null);
    }

    public override void OnJoinedRoom() {
        lobbyGroup.SetActive(false);
        gameGroup.SetActive(true);

        roomNameTxt.text = PhotonNetwork.CurrentRoom.Name;
    }
}
