using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum States
{
    CanMove,
    CantMove
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public BoxCollider2D collider;
    public GameObject token1, token2;
    public int Size = 3;
    public int[,] Matrix;
    [SerializeField] private States state = States.CanMove;
    public Camera camera;
    void Start()
    {
        Instance = this;
        Matrix = new int[Size, Size];
        Calculs.CalculateDistances(collider, Size);
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                Matrix[i, j] = 0; // 0: desocupat, 1: fitxa jugador 1, -1: fitxa IA;
            }
        }
    }
    private void Update()
    {
        if (state == States.CanMove)
        {
            Vector3 m = Input.mousePosition;
            m.z = 10f;
            Vector3 mousepos = camera.ScreenToWorldPoint(m);
            if (Input.GetMouseButtonDown(0))
            {
                if (Calculs.CheckIfValidClick((Vector2)mousepos, Matrix))
                {
                    state = States.CantMove;
                    if(Calculs.EvaluateWin(Matrix)==2)
                        StartCoroutine(WaitingABit());
                }
            }
        }
    }
    private IEnumerator WaitingABit()
    {
        yield return new WaitForSeconds(1f);
        MinmaxAI();
    }
    public void MinmaxAI()
    {
        int bestScore = int.MinValue;
        int bestX = -1, bestY = -1;

        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                if (Matrix[x, y] == 0)
                {
                    Matrix[x, y] = -1;
                    int score = Minmax(Matrix, 0, false, int.MinValue, int.MaxValue);
                    Matrix[x, y] = 0;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestX = x;
                        bestY = y;
                    }
                }
            }
        }

        if (bestX != -1 && bestY != -1)
        {
            DoMove(bestX, bestY, -1);
        }

    }
    private int Minmax(int[,] board, int depth, bool isMaximizing, int alfa, int beta)
    {
        int result = Calculs.EvaluateWin(board);

        if (result == -1) return 10 - depth;
        if (result == 1) return depth - 10;
        if (result == 0) return 0;

        int bestScore = isMaximizing ? int.MinValue : int.MaxValue;

        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                if (board[x, y] == 0)
                {
                    board[x, y] = isMaximizing ? -1 : 1;
                    int score = Minmax(board, depth + 1, !isMaximizing, alfa, beta);
                    board[x, y] = 0;

                    if (isMaximizing)
                    {
                        bestScore = Mathf.Max(score, bestScore);
                        alfa = Mathf.Max(alfa, score);
                    }
                    else
                    {
                        bestScore = Mathf.Min(score, bestScore);
                        beta = Mathf.Min(beta, score);
                    }

                    if (beta <= alfa)
                    {
                        return bestScore;
                    }
                }
            }
        }
        return bestScore;
    }
    public void DoMove(int x, int y, int team)
    {
        Matrix[x, y] = team;
        if (team == 1)
            Instantiate(token1, Calculs.CalculatePoint(x, y), Quaternion.identity);
        else
            Instantiate(token2, Calculs.CalculatePoint(x, y), Quaternion.identity);
        int result = Calculs.EvaluateWin(Matrix);
        switch (result)
        {
            case 0:
                Debug.Log("Draw");
                break;
            case 1:
                Debug.Log("You Win");
                break;
            case -1:
                Debug.Log("You Lose");
                break;
            case 2:
                if(state == States.CantMove)
                    state = States.CanMove;
                break;
        }
    }
}
