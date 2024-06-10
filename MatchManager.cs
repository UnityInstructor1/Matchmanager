using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MatchManager : MonoBehaviourPunCallbacks , IOnEventCallback
{

    public static MatchManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            NewPlayerSend(PhotonNetwork.NickName);
        }
    }

    public enum EventCodes : byte
    {
        NewPlayer,
        ListPlayers,
        UpdateStat
    }

    public List<PlayerInfo> allPlayers = new List<PlayerInfo>();
    private int index;

    public void OnEvent(EventData photonEvent)
    {
        if(photonEvent.Code < 200)
        {
            EventCodes theEvent = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

            Debug.Log("received event " + theEvent);
            switch (theEvent)
            {
                case EventCodes.NewPlayer:
                    NewPlayerRecieve(data);
                    break;

                case EventCodes.ListPlayers:
                    ListPlayerRecieve(data);
                    break;

                case EventCodes.UpdateStat:
                    UpdatestatsRecieve(data);
                    break;
            }
        }
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void NewPlayerSend(string username)
    {
        object[] package = new object[4];
        package[0] = username;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;

        PhotonNetwork.RaiseEvent(
           (byte)EventCodes.NewPlayer, package, new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
           new SendOptions { Reliability = true }

            );
    }

    public void NewPlayerRecieve(object [] dataRecieved )
    {
        PlayerInfo player = new PlayerInfo((string)dataRecieved[0],(int) dataRecieved[1],(int) dataRecieved[2],(int) dataRecieved[3]);

        allPlayers.Add(player);

        listplayerSend();
    }

    public void listplayerSend()
    {
        object[] package = new object[allPlayers.Count];

        for(int i = 0; i< allPlayers.Count; i++)
        {
            object[] piece = new object[4];
            piece[0] = allPlayers[i].name;
            piece[1] = allPlayers[i].playerNumber;
            piece[2] = allPlayers[i].kills;
            piece[3] = allPlayers[i].deaths;

            package[i] = piece;
        }

        PhotonNetwork.RaiseEvent(
           (byte)EventCodes.ListPlayers, package, new RaiseEventOptions { Receivers = ReceiverGroup.All },
           new SendOptions { Reliability = true }

            );
    }

    public void ListPlayerRecieve(object[] dataRecieved)
    {
        allPlayers.Clear();
        for(int i=0; i < dataRecieved.Length; i++)
        {
            object[] piece = (object[])dataRecieved[i];
            PlayerInfo player = new PlayerInfo(
                (string)piece[0], (int)piece[1], (int)piece[2], (int)piece[3]);

            allPlayers.Add(player);

            if (PhotonNetwork.LocalPlayer.ActorNumber == player.playerNumber)
            {
                index = i;
            }

        }
    }
    public void UpdatestatsSend(int actorSending,int statToUpdate,int amountToChange)
    {
        object[] package = new object[] { actorSending, statToUpdate, amountToChange };

        PhotonNetwork.RaiseEvent(
           (byte)EventCodes.UpdateStat, package, new RaiseEventOptions { Receivers = ReceiverGroup.All },
           new SendOptions { Reliability = true }

            );

    }

    public void UpdatestatsRecieve(object[] dataRecieved)
    {
        int actor = (int)dataRecieved[0];
        int stattype = (int)dataRecieved[1];
        int amount = (int)dataRecieved[2];

        for(int i=0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i].playerNumber == actor)
            {
                switch (stattype)
                {
                    case 0:
                        allPlayers[i].kills += amount;
                        Debug.Log("Player" + allPlayers[i].name + ":kills " + allPlayers[i].kills);
                        break;

                    case 1:
                        allPlayers[i].deaths += amount;
                        Debug.Log("Player" + allPlayers[i].name + ":deaths " + allPlayers[i].deaths);
                        break;

                        
                }

                break;
            }
        }

    }

}

[System.Serializable]

public class PlayerInfo
{
    public string name;
    public int playerNumber, kills, deaths;

    public PlayerInfo(string _name,int _playerNumber,int _kills,int _deaths)
    {
        name = _name;
        playerNumber = _playerNumber;
        kills = _kills;
        deaths = _deaths;
    }

}
