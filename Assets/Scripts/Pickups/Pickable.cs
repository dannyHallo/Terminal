using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioClip))]
public class Pickable : MonoBehaviour
{
    public PlayerController.InstrumentTypes instrumentType;
    public UIManager UIManager;
    public int _instrumentInt;
    public StageManagement stageManagement;
    private void Start()
    {
        stageManagement = FindObjectOfType<StageManagement>();
        UIManager = FindObjectOfType<UIManager>();
    }

    private void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Player")
        {
            return;

        }
        UIManager.pickUpUI.SetActive(true);
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag != "Player")
        {
            return;
        }
        UIManager.pickUpUI.SetActive(false);

    }
    private void OnTriggerStay(Collider other)
    {

        if (other.gameObject.tag != "Player")
        { return; }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (instrumentType == PlayerController.InstrumentTypes.Guitar)
            {
                stageManagement.StageSwitch(5);
            }
            if (instrumentType == PlayerController.InstrumentTypes.Sax)
            {
                stageManagement.StageSwitch(1);
            }
            if (instrumentType == PlayerController.InstrumentTypes.Dudelsa)
            {
                stageManagement.StageSwitch(3);
            }
            if (instrumentType == PlayerController.InstrumentTypes.Mic)
            {
                stageManagement.StageSwitch(7);
            }
            GameObject player = GameObject.Find("Player");
            // PlayerController.InstrumentTypes instrumentType = PlayerController.InstrumentTypes.Guitar;
            List<PlayerController.enumToInstrument> playerInstuments = player.GetComponent<PlayerController>().instruments;
            for (int i = 0; i < playerInstuments.Count; i++)
            {
                if (playerInstuments[i].e == instrumentType)
                {
                    PlayerController.enumToInstrument instrumentSetting = playerInstuments[i];
                    instrumentSetting.have = true;
                    playerInstuments[i] = instrumentSetting;
                }
            }

            player.GetComponent<PlayerController>().TryUseInstrument(instrumentType);

            Destroy(this.gameObject);
            UIManager.pickUpUI.SetActive(false);
            //UIManager.InstrumentsUI[_instrumentInt].SetActive(true);
            //player.GetComponent<PlayerController>().InstrumentUIColor(_instrumentInt);
        }

    }



}

