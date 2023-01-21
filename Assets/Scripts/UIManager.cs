using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using Unity.Netcode;
//using FishNet.Managing;
//using FishNet.Transporting;
using UnityEngine.SceneManagement;
public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] GameObject gameManager;
    [SerializeField] GameObject manager;
    [SerializeField] Button hostButton;
    [SerializeField] Button serverButton;
    [SerializeField] Button clientButton;

    [SerializeField] GameObject endMenu;
    [SerializeField] Button RestartButton;
    [SerializeField] Button QuitButton;

    [SerializeField] GameObject UImanager;
    [SerializeField] GameObject PlayerUI;
    [SerializeField] InputField joinCode;
    // [SerializeField] GameObject cameraMain;
    //NetworkManager _networkManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        //_networkManager = FindObjectOfType<NetworkManager>();
    }
    // Start is called before the first frame update
    void Start()
    {
        hostButton.onClick.AddListener(() =>
        {
            //if (RelayManager.instance.IsRelayEnabled)
            //{
            //    await RelayManager.instance.SetupRelay();
            //}
            //gameManager.SetActive(true);
            //_networkManager.ServerManager.StartConnection();
            Logger.Instance.LogInfo("Host Connected!");
            
            //cameraMain.SetActive(false);
            //Instantiate(manager, Vector3.zero, Quaternion.identity);
            UImanager.SetActive(false);

            //PlayerUI.SetActive(true);
            //SoundManager.Manager.StopMusic();
            





        });

        serverButton.onClick.AddListener(() =>
        {
            //_networkManager.ServerManager.StartConnection();
            // cameraMain.SetActive(false);
            UImanager.SetActive(false);
            
            Logger.Instance.LogInfo("Server Connected!");

        });

        clientButton.onClick.AddListener(() =>
        {
            //if (RelayManager.instance.IsRelayEnabled && !string.IsNullOrEmpty(joinCode.text))
            //    await RelayManager.instance.JoinRelay(joinCode.text);

            //FindObjectOfType<PlayerMovement>().ClientHasSpawned();
            //_networkManager.ClientManager.StartConnection();
            //HealthBar.instance.CheckPlayerJoined();
            // cameraMain.SetActive(false);
            //Instantiate(manager, Vector3.zero, Quaternion.identity);
            UImanager.SetActive(false);
            

            Logger.Instance.LogInfo("Client Connected!");


        });

        //RestartButton.onClick.AddListener(() =>
        //{
        //    NetworkManager.Singleton.Shutdown();
        //    UImanage.SetActive(true);
        //    SceneManager.LoadScene(0);
        //});
        //QuitButton.onClick.AddListener(() =>
        //{
        //    Application.Quit();
        //});

    }
    public void Shutdown()
    {

        //NetworkManager.Singleton.Shutdown();
        UImanager.SetActive(true);
    }
    public void ShowEndMenu()
    {
        endMenu.SetActive(true);
    }

}
