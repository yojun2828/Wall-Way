using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneManeger : MonoBehaviour
{
    // 「GAME START」ボタンに割当てる関数ですの
    public void OnStartButtonClicked()
    {
        // プレイ画面のシーン名（例: "BoardScene" や "MainScene"）を指定します
        SceneManager.LoadScene("PlayScene");
    }

    // 「EXIT」ボタンに割当てる関数ですの
    public void OnExitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // エディタ実行時の停止
#else
            Application.Quit(); // ビルド後のゲーム終了
#endif
    }
}
