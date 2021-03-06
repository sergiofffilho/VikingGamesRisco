﻿using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Advertisements;

public class ControladorGameGameOver : MonoBehaviour {
	public GameObject RecordeLigado;
	Text textRecorde;
	GameObject audio;

	ControladorPlayer controladorPlayer;
	GameObject pingo;
	ControladorPlataformas controladorPlataforma;
	ControladorAudio ControladorAudio;

	GameObject pontuacaoInGame;

	Image botaoContinue;

	Button button;

	ParticleSystem particula; 

	Color c1;
	Color c2;

	public GameObject loadingImage;
	private AsyncOperation async;

	public GameObject painelContinue;
	public GameObject painelReady;
	public Text tempo;

	GameObject botaoCancelarAds;

	int verificadorAleatorio;

	public Button skipVideo;
	public Button watchAds;

	PlayerMoviments playerMoviments;

	// Use this for initialization
	void Start () {
		particula = GameObject.FindGameObjectWithTag ("controladorLinha").GetComponent<ParticleSystem> ();
		controladorPlayer = GameObject.FindGameObjectWithTag ("Player").GetComponent<ControladorPlayer> ();
		controladorPlataforma =  GameObject.FindGameObjectWithTag("controladorPlat").GetComponent<ControladorPlataformas>();
		ControladorAudio = GameObject.FindGameObjectWithTag ("Audio").GetComponent<ControladorAudio> ();

		pontuacaoInGame = GameObject.FindGameObjectWithTag ("pontuacaoInGame");

		audio = GameObject.FindGameObjectWithTag ("Audio");

		textRecorde = GameObject.FindGameObjectWithTag ("Pontuacao").GetComponent<Text> ();

		playerMoviments = GameObject.Find ("PlayerMoviments").GetComponent<PlayerMoviments> ();

		pontuacaoInGame.SetActive (false);

		textRecorde.text = PlayerPrefs.GetFloat ("Pontuacao").ToString();

		botaoContinue = GameObject.FindGameObjectWithTag ("botaoContinue").GetComponent<Image>();
		c1 = botaoContinue.color;
		c2 = botaoContinue.color;
		c1.a = 0.5f;
		c2.a = 1;

		if (PlayerPrefs.GetInt ("continue") == 0) {
			botaoContinue.color = c2;
		} else {
			botaoContinue.color = c1;
		}

		if (PlayerPrefs.GetFloat ("Pontuacao") >= PlayerPrefs.GetFloat ("Recorde")) {
			RecordeLigado.SetActive(true);
		}

		SceneManager.SetActiveScene (SceneManager.GetSceneByName("GameOver"));

		Transform[] trans = GameObject.Find ("Player").GetComponentsInChildren<Transform> (true);
		foreach (Transform t in trans) {
			if (t.gameObject.name == "Animacao") {
				pingo = t.gameObject;
			}
		}
		pingo.SetActive (true);

		botaoCancelarAds = GameObject.FindGameObjectWithTag ("cancelarAds");

		verificadorAleatorio = UnityEngine.Random.Range(0,100);
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit();
		}
	}

	public void returnMenu(){
		loadingImage.SetActive(true);
		int level = 0;
		StartCoroutine( loadingPlay (level));

		Destroy(audio);
		Destroy(playerMoviments.gameObject);
		Application.LoadLevel (0);	
	}

	IEnumerator loadingPlay (int level)
	{
		async = Application.LoadLevelAsync(level);
		while (!async.isDone)
		{
			yield return null;
		}
	}

	public void continueGame(){
		StartCoroutine(checkInternetConnection((isConnected)=>{
		}));
	}

	public void cancelarAds(){
		painelContinue.SetActive (false);
	}

	public void getAds(){
		if (Advertisement.IsReady("rewardedVideo"))
		{
			painelContinue.SetActive (false);
			loadingImage.SetActive(true);
			ShowOptions options = new ShowOptions { resultCallback = HandleShowResult };
			Advertisement.Show("rewardedVideo", options);
		}
	}

	private void HandleShowResult(ShowResult result)
	{
		switch (result)
		{
			case ShowResult.Finished:
				Debug.Log ("The ad was successfully shown.");
//				SceneManager.LoadScene("Continue");
				painelReady.SetActive(true);
				StartCoroutine (ContagemRegressivaInicio ());
				break;
			case ShowResult.Skipped:
				Debug.Log("The ad was skipped before reaching the end.");
				break;
			case ShowResult.Failed:
				Debug.LogError("The ad failed to be shown.");
				break;
		}
	}

	IEnumerator ContagemRegressivaInicio(){
		yield return new WaitForSeconds (1f);
		tempo.text = "2";
		StartCoroutine (ContagemRegressivaContinue ());
	}

	IEnumerator ContagemRegressivaContinue(){
		yield return new WaitForSeconds (1f);
		tempo.text = "1";
		StartCoroutine (ContagemRegressivaGo ());
	}

	IEnumerator ContagemRegressivaGo(){
		yield return new WaitForSeconds (1f);
		tempo.text = "Go";
		StartCoroutine (ContagemRegressivaFim ());
	}

	IEnumerator ContagemRegressivaFim(){
		yield return new WaitForSeconds (1f);
		voltarInGame ();
	}



	public void buttonRate(){
		Application.OpenURL("https://play.google.com/store/apps/details?id=com.amazingplay.risco&hl=pt_BR");
	}

	//--------------------------------------------------------------
	//Check Internet

	IEnumerator checkInternetConnection(Action<bool> action){
		WWW www = new WWW("http://google.com");
		yield return www;
		Debug.Log (action);
		if (www.error != null) {
				Debug.Log ("desligado");
				watchAds.interactable = false;
//				painelContinue.SetActive (true);
//
//				if(PlayerPrefs.GetFloat("gotas") < 100){
//					skipVideo.interactable = false;
//				}

			action (false);
		} else {
			Debug.Log ("ligado");

		}
		painelContinue.SetActive (true);

		if(PlayerPrefs.GetFloat("gotas") < 100){
			skipVideo.interactable = false;
		}
		action (true);
	} 

	public void voltarInGame(){
		SceneManager.UnloadScene ("GameOver");
		GameObject.Find ("Canvas").transform.GetChild (1).gameObject.SetActive (true);
//		controladorPlayer.gameObject.transform.GetChild(2).gameObject.SetActive (true);
		controladorPlayer.detectSwipe = true;
		controladorPlayer.setIsAlive (true);
		particula.maxParticles = 10000;
		ControladorAudio.playGame ();
		controladorPlayer.textGotas.text = PlayerPrefs.GetFloat ("gotas").ToString ();
		pontuacaoInGame.SetActive (true);
		controladorPlayer.initCoroutine ();
//		PlayerPrefs.SetInt ("continue", 1);
	}

	public void SkipVideo(){
		PlayerPrefs.SetFloat ("gotas", PlayerPrefs.GetFloat ("gotas") - 100);
		painelReady.SetActive(true);
		StartCoroutine (ContagemRegressivaInicio ());
	}
}
