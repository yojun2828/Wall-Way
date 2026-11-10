using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // 引数を int ではなく Vector3（計算済みの位置）に変更
    public void MoveTo(Vector3 targetWorldPosition)
    {
        // GameManagerで計算された位置に、そのまま飛び込む
        transform.position = targetWorldPosition;

        Debug.Log($"プレイヤーがワールド座標 {targetWorldPosition} に移動しました");
    }
}