﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public enum GameStates {Create,Place, Select,Move,Wait}
public enum TurnState {Player1,Player2}
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameStates gamestate;
    public TurnState turn;
    public GameObject buyWindow;
    public Transform characterParent;
    public Player player1 = new Player(100, "Player1");
    public Player player2 = new Player(100, "Player2");

    private float points;
    public Player curPlayer;
    public GameObject selectedPlayer;

    public float roundTime = 10.0f;
    public float timeLeft = 10.0f;
    public Text startText;

    public GameObject[] items;
    public Transform[] spawnplaces;


    public void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
        }
    }

    void Update()
    {
        GameStateManager();
        TurnManager();
    }

    public void UpdateBuyWindow(bool state)
    {
        buyWindow.SetActive(state);
    }

    public void SwitchState(GameStates state)
    {
        gamestate = state;
    }

    public void PlaceCharacter()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            CreateActor.instance.actor.transform.parent = characterParent;
            CreateActor.instance.actor.layer = 0;
            SwitchState(GameStates.Create);
            NextTurn();
        }
    }

    public void GameStateManager()
    {
        switch (gamestate)
        {
            case GameStates.Create:
                UpdateBuyWindow(true);
                if(player1.ready && player2.ready)
                {
                    SwitchState(GameStates.Select);
                }
                break;
            case GameStates.Place:
                UpdateBuyWindow(false);
                PlaceCharacter();
                break;
            case GameStates.Select:
                UpdateBuyWindow(false);

                break;
            case GameStates.Move:
                UpdateBuyWindow(false);
                MoveSelectedPlayer(selectedPlayer);
                Timer();
                break;
        }
    }

    public void TurnManager()
    {
        switch (turn)
        {
            case TurnState.Player1:
                curPlayer = player1;
                break;
            case TurnState.Player2:
                curPlayer = player2;
                break;
        }
    }

    public void NextTurn()
    {
        switch (turn)
        {
            case TurnState.Player1:
                turn = TurnState.Player2;
                break;
            case TurnState.Player2:
                turn = TurnState.Player1;
                break;
        }
        timeLeft = roundTime;
        if(gamestate == GameStates.Move || gamestate == GameStates.Place) {
            StartCoroutine(SpawnItems());
        }
    }
    public void ReadyUp()
    {
        curPlayer.ready = true;
        NextTurn();
    }

    public void SelectPlayer(GameObject selectedPlayer)
    {
        this.selectedPlayer = selectedPlayer;
        CameraBehaviour.instance.target = selectedPlayer.transform;
        SwitchState(GameStates.Move);

    }

    public void MoveSelectedPlayer(GameObject selectedPlayer)
    {
        Character player = selectedPlayer.GetComponent<Character>();
        player.characterState = CharacterStates.Moving;
    }

    public void Timer()
    {
        timeLeft -= Time.deltaTime;
        startText.text = (timeLeft).ToString("0");
        if (timeLeft < 0)
        {
            SwitchState(GameStates.Select);
            NextTurn();
            Character player = selectedPlayer.GetComponent<Character>();
            player.characterState = CharacterStates.Idle;
            CameraBehaviour.instance.ResetPosition();
            GameObject[] player1 = GameObject.FindGameObjectsWithTag("Player1");
            GameObject[] player2 = GameObject.FindGameObjectsWithTag("Player2");

            if(player1.Length <= 0)
            {
                Debug.Log("player2 wins");
                ResetScene();
            }
            if (player2.Length <= 0)
            {
                Debug.Log("player2 wins");
                ResetScene();
            }
        }

    }

    public void ResetScene()
    {
        SceneManager.LoadScene(0);
        gamestate = GameStates.Create;
    }

    public IEnumerator SpawnItems()
    {
        GameObject g = Instantiate(items[Random.Range(0, items.Length)], spawnplaces[Random.Range(0, spawnplaces.Length)].position, Quaternion.identity);
        CameraBehaviour.instance.target = g.transform;
        var prevGameState = gamestate;
        gamestate = GameStates.Wait;
        CameraBehaviour.instance.transform.position = g.transform.position + new Vector3(0, 6.5f, -10);
        CameraBehaviour.instance.transform.LookAt(g.transform);
        yield return new WaitForSeconds(5);
        gamestate = prevGameState;
    }
}
