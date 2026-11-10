using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManeger : MonoBehaviour
{
    // 「GAME START」ボタンに割当てる関数ですの
    public void OnTitleButtonClicked()
    {
        // プレイ画面のシーン名（例: "BoardScene" や "MainScene"）を指定します
        SceneManager.LoadScene("TitleScene");
    }

}
