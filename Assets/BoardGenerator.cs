using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    [Header("勝利演出UI")]
    [SerializeField] private TextMeshProUGUI wallCountText;
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private TextMeshProUGUI wallDirectionText;
    [SerializeField] private GameObject wallPreviewPrefab;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI victoryText;

    public GameObject tilePrefab;
    public GameObject PlayerPrefab1;
    public GameObject PlayerPrefab2;
    public GameObject wallPrefab;
    private GameObject Player1;
    private GameObject Player2;
    private GameObject currentPreview;
    private SpriteRenderer previewRenderer;
    public int boardSize = 9;
    List<string> verticalwallpositions = new List<string>(); // 縦の壁専用
    List<string> horizontalwallpositions = new List<string>(); // 横の壁専用
    private int player1WallCount = 10;
    private int player2WallCount = 10;
    private bool isGameOver = false;

    // 現在の座標を管理（0〜8）
    private int player1X = 4;
    private int player1Y = 0;

    // プレイヤー2の座標
    private int player2X = 4;
    private int player2Y = 8;

    // 現在の手番
    private bool isPlayer1Turn = true;

    private int offset;
    [SerializeField] private float spacing = 1.1f;
    private bool isVertical = true;

    private bool player1IsVertical = true;
    private bool player2IsVertical = true;

    private Coroutine warningCoroutine;

    void Start()
    {
        offset = boardSize / 2; // 4

        // 盤面タイルの生成
        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                Vector3 pos = new Vector3((x - offset) * spacing, (y - offset) * spacing, 0);
                Instantiate(tilePrefab, pos, Quaternion.identity, transform);
            }
        }

        // プレイヤーの初期配置
        Vector3 p1startPos = new Vector3((player1X - offset) * spacing, (player1Y - offset) * spacing, 0);
        Player1 = Instantiate(PlayerPrefab1, p1startPos, Quaternion.identity);

        Vector3 p2startPos = new Vector3((player2X - offset) * spacing, (player2Y - offset) * spacing, 0);
        Player2 = Instantiate(PlayerPrefab2, p2startPos, Quaternion.identity);

        UpdateWallCountText();
        UpdateWallDirectionText(player1IsVertical);

        if (warningText != null)
        {
            warningText.text = "";
            Color c = warningText.color;
            c.a = 0f;
            warningText.color = c;
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        turnText.text = "P1";

        currentPreview = Instantiate(wallPreviewPrefab, Vector3.zero, Quaternion.identity);
        currentPreview.SetActive(false);
        previewRenderer = currentPreview.GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (isGameOver) return;

        GameObject activePlayer;
        int currentX, currentY, enemyX, enemyY;

        bool currentIsVertical = isPlayer1Turn ? player1IsVertical : player2IsVertical;
        isVertical = currentIsVertical;

        if (isPlayer1Turn)
        {
            activePlayer = Player1;
            currentX = player1X;
            currentY = player1Y;
            enemyX = player2X;
            enemyY = player2Y;
        }
        else
        {
            activePlayer = Player2;
            currentX = player2X;
            currentY = player2Y;
            enemyX = player1X;
            enemyY = player1Y;
        }

        // 左クリックの処理（プレイヤー移動）
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            int targetX = Mathf.RoundToInt(mousePos.x / spacing) + offset;
            int targetY = Mathf.RoundToInt(mousePos.y / spacing) + offset;

            if (targetX == enemyX && targetY == enemyY)
            {
                Debug.Log("相手がいるので移動できません");
                return;
            }

            // 右移動の制限
            if (targetX == currentX + 1 && targetY == currentY)
            {
                Vector3 checkPosA = new Vector3(((currentX + 0.5f) - offset) * spacing, ((currentY + 0.5f) - offset) * spacing, 0);
                Vector3 checkPosB = new Vector3(((currentX + 0.5f) - offset) * spacing, ((currentY - 0.5f) - offset) * spacing, 0);
                if (verticalwallpositions.Contains(FormatVector3(checkPosA)) || verticalwallpositions.Contains(FormatVector3(checkPosB)))
                {
                    Debug.Log("壁があります");
                    return;
                }
            }

            // 左移動の制限
            if (targetX == currentX - 1 && targetY == currentY)
            {
                Vector3 checkPosA = new Vector3(((currentX - 0.5f) - offset) * spacing, ((currentY + 0.5f) - offset) * spacing, 0);
                Vector3 checkPosB = new Vector3(((currentX - 0.5f) - offset) * spacing, ((currentY - 0.5f) - offset) * spacing, 0);
                if (verticalwallpositions.Contains(FormatVector3(checkPosA)) || verticalwallpositions.Contains(FormatVector3(checkPosB)))
                {
                    Debug.Log("壁があります");
                    return;
                }
            }

            // 前移動の制限
            if (targetX == currentX && targetY == currentY + 1)
            {
                Vector3 checkPosA = new Vector3(((currentX + 0.5f) - offset) * spacing, ((currentY + 0.5f) - offset) * spacing, 0);
                Vector3 checkPosB = new Vector3(((currentX - 0.5f) - offset) * spacing, ((currentY + 0.5f) - offset) * spacing, 0);
                if (horizontalwallpositions.Contains(FormatVector3(checkPosA)) || horizontalwallpositions.Contains(FormatVector3(checkPosB)))
                {
                    Debug.Log("壁があります");
                    return;
                }
            }

            // 後ろ移動の制限
            if (targetX == currentX && targetY == currentY - 1)
            {
                Vector3 checkPosA = new Vector3(((currentX + 0.5f) - offset) * spacing, ((currentY - 0.5f) - offset) * spacing, 0);
                Vector3 checkPosB = new Vector3(((currentX - 0.5f) - offset) * spacing, ((currentY - 0.5f) - offset) * spacing, 0);
                if (horizontalwallpositions.Contains(FormatVector3(checkPosA)) || horizontalwallpositions.Contains(FormatVector3(checkPosB)))
                {
                    Debug.Log("壁があります");
                    return;
                }
            }

            // 1. 盤面内をクリックしたか
            if (targetX >= 0 && targetX < boardSize && targetY >= 0 && targetY < boardSize)
            {
                int diffX = Mathf.Abs(targetX - currentX);
                int diffY = Mathf.Abs(targetY - currentY);

                bool isNormalMove = (diffX == 1 && diffY == 0) || (diffX == 0 && diffY == 1);
                bool isJumpMove = false;

                // 上下左右2マスのジャンプ判定
                if ((diffX == 2 && diffY == 0) || (diffX == 0 && diffY == 2))
                {
                    int midX = (currentX + targetX) / 2;
                    int midY = (currentY + targetY) / 2;

                    if (midX == enemyX && midY == enemyY)
                    {
                        bool hasWall = false;

                        if (targetY == currentY + 2)
                        {
                            Vector3 check1A = new Vector3(((currentX + 0.5f) - offset) * spacing, ((currentY + 0.5f) - offset) * spacing, 0);
                            Vector3 check1B = new Vector3(((currentX - 0.5f) - offset) * spacing, ((currentY + 0.5f) - offset) * spacing, 0);
                            Vector3 check2A = new Vector3(((currentX + 0.5f) - offset) * spacing, ((currentY + 1.5f) - offset) * spacing, 0);
                            Vector3 check2B = new Vector3(((currentX - 0.5f) - offset) * spacing, ((currentY + 1.5f) - offset) * spacing, 0);

                            if (horizontalwallpositions.Contains(FormatVector3(check1A)) || horizontalwallpositions.Contains(FormatVector3(check1B)) ||
                                horizontalwallpositions.Contains(FormatVector3(check2A)) || horizontalwallpositions.Contains(FormatVector3(check2B)))
                            {
                                hasWall = true;
                            }
                        }
                        else if (targetY == currentY - 2)
                        {
                            Vector3 check1A = new Vector3(((currentX + 0.5f) - offset) * spacing, ((currentY - 0.5f) - offset) * spacing, 0);
                            Vector3 check1B = new Vector3(((currentX - 0.5f) - offset) * spacing, ((currentY - 0.5f) - offset) * spacing, 0);
                            Vector3 check2A = new Vector3(((currentX + 0.5f) - offset) * spacing, ((currentY - 1.5f) - offset) * spacing, 0);
                            Vector3 check2B = new Vector3(((currentX - 0.5f) - offset) * spacing, ((currentY - 1.5f) - offset) * spacing, 0);

                            if (horizontalwallpositions.Contains(FormatVector3(check1A)) || horizontalwallpositions.Contains(FormatVector3(check1B)) ||
                                horizontalwallpositions.Contains(FormatVector3(check2A)) || horizontalwallpositions.Contains(FormatVector3(check2B)))
                            {
                                hasWall = true;
                            }
                        }
                        else if (targetX == currentX + 2)
                        {
                            Vector3 check1A = new Vector3(((currentX + 0.5f) - offset) * spacing, ((currentY + 0.5f) - offset) * spacing, 0);
                            Vector3 check1B = new Vector3(((currentX + 0.5f) - offset) * spacing, ((currentY - 0.5f) - offset) * spacing, 0);
                            Vector3 check2A = new Vector3(((currentX + 1.5f) - offset) * spacing, ((currentY + 0.5f) - offset) * spacing, 0);
                            Vector3 check2B = new Vector3(((currentX + 1.5f) - offset) * spacing, ((currentY - 0.5f) - offset) * spacing, 0);

                            if (verticalwallpositions.Contains(FormatVector3(check1A)) || verticalwallpositions.Contains(FormatVector3(check1B)) ||
                                verticalwallpositions.Contains(FormatVector3(check2A)) || verticalwallpositions.Contains(FormatVector3(check2B)))
                            {
                                hasWall = true;
                            }
                        }
                        else if (targetX == currentX - 2)
                        {
                            Vector3 check1A = new Vector3(((currentX - 0.5f) - offset) * spacing, ((currentY + 0.5f) - offset) * spacing, 0);
                            Vector3 check1B = new Vector3(((currentX - 0.5f) - offset) * spacing, ((currentY - 0.5f) - offset) * spacing, 0);
                            Vector3 check2A = new Vector3(((currentX - 1.5f) - offset) * spacing, ((currentY + 0.5f) - offset) * spacing, 0);
                            Vector3 check2B = new Vector3(((currentX - 1.5f) - offset) * spacing, ((currentY - 0.5f) - offset) * spacing, 0);

                            if (verticalwallpositions.Contains(FormatVector3(check1A)) || verticalwallpositions.Contains(FormatVector3(check1B)) ||
                                verticalwallpositions.Contains(FormatVector3(check2A)) || verticalwallpositions.Contains(FormatVector3(check2B)))
                            {
                                hasWall = true;
                            }
                        }

                        if (!hasWall)
                        {
                            isJumpMove = true;
                            Debug.Log("相手を飛び越えるジャンプ移動を検知しました！");
                        }
                        else
                        {
                            Debug.Log("飛び越えるルート上に壁があります！");
                        }
                    }
                }

                if (isNormalMove || isJumpMove)
                {
                    Vector3 targetWorldPos = new Vector3((targetX - offset) * spacing, (targetY - offset) * spacing, 0);
                    activePlayer.GetComponent<Player>().MoveTo(targetWorldPos);

                    if (isPlayer1Turn)
                    {
                        player1X = targetX;
                        player1Y = targetY;
                        Debug.Log($"[P1移動完了] 現在座標: ({player1X}, {player1Y})");
                    }
                    else
                    {
                        player2X = targetX;
                        player2Y = targetY;
                        Debug.Log($"[P2移動完了] 現在座標: ({player2X}, {player2Y})");
                    }

                    // 勝利判定チェック（決着がついたらターン交代をスキップして終了）
                    if (CheckWinCondition()) return;

                    // ターン交代
                    isPlayer1Turn = !isPlayer1Turn;

                    UpdateWallCountText();
                    if (turnText != null) turnText.text = isPlayer1Turn ? "P1" : "P2";

                    bool nextIsVertical = isPlayer1Turn ? player1IsVertical : player2IsVertical;
                    UpdateWallDirectionText(nextIsVertical);
                }
                else if (diffX == 0 && diffY == 0)
                {
                    Debug.Log("今いる場所です");
                }
                else
                {
                    Debug.Log("そこには移動できません");
                }
            }
        }

        // Rキーで向きを切り替える
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentIsVertical = !currentIsVertical;
            isVertical = currentIsVertical;

            if (isPlayer1Turn) player1IsVertical = currentIsVertical;
            else player2IsVertical = currentIsVertical;

            UpdateWallDirectionText(currentIsVertical);
        }

        // 右クリックの処理（壁の設置）
        if (Input.GetMouseButtonDown(1))
        {
            if (isPlayer1Turn && player1WallCount <= 0)
            {
                Debug.Log("プレイヤー1はもう壁がありません！");
                return;
            }
            if (!isPlayer1Turn && player2WallCount <= 0)
            {
                Debug.Log("プレイヤー2はもう壁がありません！");
                return;
            }

            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = 10.0f;
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

            float gridX = (mouseWorldPos.x / spacing) + offset;
            float gridY = (mouseWorldPos.y / spacing) + offset;

            float snapX = Mathf.Floor(gridX) + 0.5f;
            float snapY = Mathf.Floor(gridY) + 0.5f;

            if (snapX < 0.5f || snapX > 7.5f || snapY < 0.5f || snapY > 7.5f)
            {
                Debug.Log("壁がはみ出します");
                return;
            }

            Vector3 wallSnapPos = new Vector3((snapX - offset) * spacing, (snapY - offset) * spacing, 0);
            string wallSnapPosstr = FormatVector3(wallSnapPos);

            if (isVertical)
            {
                Vector3 checkUp = wallSnapPos + new Vector3(0, spacing, 0);
                Vector3 checkDown = wallSnapPos - new Vector3(0, spacing, 0);

                if (verticalwallpositions.Contains(wallSnapPosstr) ||
                    verticalwallpositions.Contains(FormatVector3(checkUp)) ||
                    verticalwallpositions.Contains(FormatVector3(checkDown)) ||
                    horizontalwallpositions.Contains(wallSnapPosstr))
                {
                    Debug.Log("そこには設置できない（重なり・交差）");
                    return;
                }
            }
            else
            {
                Vector3 checkRight = wallSnapPos + new Vector3(spacing, 0, 0);
                Vector3 checkLeft = wallSnapPos - new Vector3(spacing, 0, 0);

                if (horizontalwallpositions.Contains(wallSnapPosstr) ||
                    horizontalwallpositions.Contains(FormatVector3(checkRight)) ||
                    horizontalwallpositions.Contains(FormatVector3(checkLeft)) ||
                    verticalwallpositions.Contains(wallSnapPosstr))
                {
                    Debug.Log("そこには設置できない（重なり・交差）");
                    return;
                }
            }

            if (isVertical) verticalwallpositions.Add(wallSnapPosstr);
            else horizontalwallpositions.Add(wallSnapPosstr);

            bool pP1Reachable = CanReachGoal(player1X, player1Y, true);
            bool pP2Reachable = CanReachGoal(player2X, player2Y, false);

            if (pP1Reachable && pP2Reachable)
            {
                Quaternion wallRotation = isVertical ? Quaternion.identity : Quaternion.Euler(0, 0, 90);
                GameObject placedWall = Instantiate(wallPrefab, wallSnapPos, wallRotation);

                SpriteRenderer wallRenderer = placedWall.GetComponentInChildren<SpriteRenderer>();
                if (wallRenderer != null)
                {
                    wallRenderer.color = isPlayer1Turn ? new Color(1.0f, 0.2f, 0.5f, 1.0f) : new Color(0.2f, 0.5f, 1.0f, 1.0f);
                }

                if (isPlayer1Turn) player1WallCount--;
                else player2WallCount--;

                if (isPlayer1Turn) player1IsVertical = true;
                else player2IsVertical = true;

                isPlayer1Turn = !isPlayer1Turn;

                UpdateWallCountText();
                if (turnText != null) turnText.text = isPlayer1Turn ? "P1" : "P2";

                bool nextIsVertical = isPlayer1Turn ? player1IsVertical : player2IsVertical;
                UpdateWallDirectionText(nextIsVertical);
            }
            else
            {
                if (isVertical) verticalwallpositions.Remove(wallSnapPosstr);
                else horizontalwallpositions.Remove(wallSnapPosstr);

                ShowWarning("プレイヤーを完全に閉じ込める壁は置けません！");
            }
        }

        // プレビューの描画処理
        if (!isGameOver)
        {
            Vector3 pMousePos = Input.mousePosition;
            pMousePos.z = 10.0f;
            Vector3 pMouseWorldPos = Camera.main.ScreenToWorldPoint(pMousePos);

            float pGridX = (pMouseWorldPos.x / spacing) + offset;
            float pGridY = (pMouseWorldPos.y / spacing) + offset;

            float pSnapX = Mathf.Floor(pGridX) + 0.5f;
            float pSnapY = Mathf.Floor(pGridY) + 0.5f;

            if (pSnapX >= 0.5f && pSnapX <= 7.5f && pSnapY >= 0.5f && pSnapY <= 7.5f)
            {
                currentPreview.SetActive(true);

                Vector3 pWallSnapPos = new Vector3((pSnapX - offset) * spacing, (pSnapY - offset) * spacing, 0);

                bool pIsVertical = isPlayer1Turn ? player1IsVertical : player2IsVertical;
                Quaternion pWallRotation = pIsVertical ? Quaternion.identity : Quaternion.Euler(0, 0, 90);

                currentPreview.transform.position = pWallSnapPos;
                currentPreview.transform.rotation = pWallRotation;

                bool canPlace = CheckIfCanPlaceRealtime(pWallSnapPos, pIsVertical);

                if (previewRenderer != null)
                {
                    previewRenderer.color = canPlace ? new Color(0.2f, 1.0f, 0.2f, 0.4f) : new Color(1.0f, 0.2f, 0.2f, 0.4f);
                }
            }
            else
            {
                currentPreview.SetActive(false);
            }
        }
    }

    private bool CheckIfCanPlaceRealtime(Vector3 snapPos, bool isVert)
    {
        string posStr = FormatVector3(snapPos);

        if (isVert)
        {
            Vector3 checkUp = snapPos + new Vector3(0, spacing, 0);
            Vector3 checkDown = snapPos - new Vector3(0, spacing, 0);
            if (verticalwallpositions.Contains(posStr) ||
                verticalwallpositions.Contains(FormatVector3(checkUp)) ||
                verticalwallpositions.Contains(FormatVector3(checkDown)) ||
                horizontalwallpositions.Contains(posStr))
            {
                return false;
            }
        }
        else
        {
            Vector3 checkRight = snapPos + new Vector3(spacing, 0, 0);
            Vector3 checkLeft = snapPos - new Vector3(spacing, 0, 0);
            if (horizontalwallpositions.Contains(posStr) ||
                horizontalwallpositions.Contains(FormatVector3(checkRight)) ||
                horizontalwallpositions.Contains(FormatVector3(checkLeft)) ||
                verticalwallpositions.Contains(posStr))
            {
                return false;
            }
        }

        if (isVert) verticalwallpositions.Add(posStr);
        else horizontalwallpositions.Add(posStr);

        bool p1Ok = CanReachGoal(player1X, player1Y, true);
        bool p2Ok = CanReachGoal(player2X, player2Y, false);

        if (isVert) verticalwallpositions.Remove(posStr);
        else horizontalwallpositions.Remove(posStr);

        return p1Ok && p2Ok;
    }

    private bool CanReachGoal(int startX, int startY, bool isPlayer1)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        bool[,] visited = new bool[9, 9];

        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (isPlayer1 && current.y == 8) return true;
            if (!isPlayer1 && current.y == 0) return true;

            int[] dx = { 0, 0, 1, -1 };
            int[] dy = { 1, -1, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                int nextX = current.x + dx[i];
                int nextY = current.y + dy[i];

                if (nextX < 0 || nextX > 8 || nextY < 0 || nextY > 8) continue;
                if (visited[nextX, nextY]) continue;

                bool hasWall = false;

                if (dy[i] == 1) // 上
                {
                    Vector3 wA = new Vector3(((current.x + 0.5f) - offset) * spacing, ((current.y + 0.5f) - offset) * spacing, 0);
                    Vector3 wB = new Vector3(((current.x - 0.5f) - offset) * spacing, ((current.y + 0.5f) - offset) * spacing, 0);
                    if (horizontalwallpositions.Contains(FormatVector3(wA)) || horizontalwallpositions.Contains(FormatVector3(wB))) hasWall = true;
                }
                else if (dy[i] == -1) // 下
                {
                    Vector3 wA = new Vector3(((current.x + 0.5f) - offset) * spacing, ((current.y - 0.5f) - offset) * spacing, 0);
                    Vector3 wB = new Vector3(((current.x - 0.5f) - offset) * spacing, ((current.y - 0.5f) - offset) * spacing, 0);
                    if (horizontalwallpositions.Contains(FormatVector3(wA)) || horizontalwallpositions.Contains(FormatVector3(wB))) hasWall = true;
                }
                else if (dx[i] == 1) // 右
                {
                    Vector3 wA = new Vector3(((current.x + 0.5f) - offset) * spacing, ((current.y + 0.5f) - offset) * spacing, 0);
                    Vector3 wB = new Vector3(((current.x + 0.5f) - offset) * spacing, ((current.y - 0.5f) - offset) * spacing, 0);
                    if (verticalwallpositions.Contains(FormatVector3(wA)) || verticalwallpositions.Contains(FormatVector3(wB))) hasWall = true;
                }
                else if (dx[i] == -1) // 左
                {
                    Vector3 wA = new Vector3(((current.x - 0.5f) - offset) * spacing, ((current.y + 0.5f) - offset) * spacing, 0);
                    Vector3 wB = new Vector3(((current.x - 0.5f) - offset) * spacing, ((current.y - 0.5f) - offset) * spacing, 0);
                    if (verticalwallpositions.Contains(FormatVector3(wA)) || verticalwallpositions.Contains(FormatVector3(wB))) hasWall = true;
                }

                if (!hasWall)
                {
                    visited[nextX, nextY] = true;
                    queue.Enqueue(new Vector2Int(nextX, nextY));
                }
            }
        }

        return false;
    }

    private void ShowWarning(string message)
    {
        Debug.Log($"<color=orange>{message}</color>");

        if (warningText == null) return;

        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
        }

        warningCoroutine = StartCoroutine(FadeWarningText(message));
    }

    private IEnumerator FadeWarningText(string message)
    {
        warningText.text = message;
        float duration = 0.25f;
        float stayTime = 2.0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / duration);
            SetWarningTextAlpha(alpha);
            yield return null;
        }
        SetWarningTextAlpha(1f);

        yield return new WaitForSeconds(stayTime);

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsed / duration));
            SetWarningTextAlpha(alpha);
            yield return null;
        }
        SetWarningTextAlpha(0f);
        warningText.text = "";
    }

    private void SetWarningTextAlpha(float alpha)
    {
        if (warningText != null)
        {
            Color c = warningText.color;
            c.a = alpha;
            warningText.color = c;
        }
    }

    private void UpdateWallCountText()
    {
        if (wallCountText == null) return;

        if (isPlayer1Turn)
        {
            wallCountText.text = $"壁の残り:{player1WallCount}枚";
            wallCountText.color = Color.white;
        }
        else
        {
            wallCountText.text = $"壁の残り:{player2WallCount}枚";
            wallCountText.color = Color.white;
        }
    }

    private void UpdateWallDirectionText(bool isVerticalMode)
    {
        if (wallDirectionText == null) return;

        wallDirectionText.text = isVerticalMode ? "壁の向き: 縦" : "壁の向き: 横";
        wallDirectionText.color = isPlayer1Turn ? Color.white : Color.white;
    }

    public string FormatVector3(Vector3 vector)
    {
        return $"{vector.x:F2}:{vector.y:F2}:{vector.z:F2}";
    }

    private bool CheckWinCondition()
    {
        if (player1Y == 8)
        {
            Debug.Log("P1が勝利条件を満たしました！");
            OnGameEnd(1);
            return true;
        }
        else if (player2Y == 0)
        {
            Debug.Log("P2が勝利条件を満たしました！");
            OnGameEnd(2);
            return true;
        }

        return false;
    }

    private void OnGameEnd(int winnerID)
    {
        isGameOver = true;
        if (currentPreview != null) currentPreview.SetActive(false);

        string winnerName = (winnerID == 1) ? "PLAYER 1" : "PLAYER 2";

        if (turnText != null)
        {
            turnText.text = $"{winnerName} WIN!";
            turnText.color = Color.yellow;
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        if (victoryText != null)
        {
            victoryText.text = $"{winnerName} WINS!";
            victoryText.color = (winnerID == 1) ? new Color(1.0f, 0.4f, 0.6f) : new Color(0.3f, 0.7f, 1.0f);
        }

        Debug.Log($"<color=gold><b>🏆✨ 【GAME OVER】 {winnerName} WIN! ✨🏆</b></color>");
    }
}