using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryManeger : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI winnerNameText;

    // ゲーム開始時はパネルを隠しておく
    private void Start()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
    }

    // 勝利時に呼び出す関数
    public void ShowVictoryUI(int winnerPlayerID)
    {
        Debug.Log($"★勝利UIを呼び出しました！ 勝者: P{winnerPlayerID}");

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("⚠️ victoryPanel が Inspector で割り当てられていませんわ！");
        }

        if (winnerPlayerID == 1)
        {
            winnerNameText.text = "PLAYER 1 WINS!";
            winnerNameText.color = new Color(1.0f, 0.4f, 0.6f); // P1カラー（ピンク・赤系）
        }
        else
        {
            winnerNameText.text = "PLAYER 2 WINS!";
            winnerNameText.color = new Color(0.3f, 0.7f, 1.0f); // P2カラー（青系）
        }
    }

    // 「REMATCH」ボタン用：現在のシーンを再読み込み
    public void OnRematchButtonClicked()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    // 「TITLE」ボタン用：タイトルシーンへ戻る
    public void OnTitleButtonClicked()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
