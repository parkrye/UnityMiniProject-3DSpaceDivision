using System;
using System.Collections.Generic;
using UnityEngine;

public class SpaceManager : MonoBehaviour
{
    [SerializeField] List<Space> wholeSpace;
    public List<Space> WholeSpaces { get { return wholeSpace; } set { wholeSpace = value; } }

    [SerializeField][Range(1f, 5f)] float spaceSize;
    [SerializeField][Range(2, 30)] int spaceLine;
    [SerializeField] GameObject spacePrefab;

    public int SpaceLine { get { return spaceLine; } }

    void Awake()
    {
        WholeSpaces = new List<Space>();
    }

    public void SettingSpaces()
    {
        int index = 0;
        for (int i = -spaceLine / 2; i < spaceLine / 2; i++)
        {
            for (int j = -spaceLine / 2; j < spaceLine / 2; j++)
            {
                Space space = Instantiate(spacePrefab, transform).GetComponent<Space>();
                space.transform.Translate((Vector3.forward * i + Vector3.right * j + Vector3.up * 0.5f) * spaceSize);
                space.transform.localScale = Vector3.one * spaceSize;
                space.Initialize(index);
                index++;
                WholeSpaces.Add(space);
            }
        }
    }

    // �þ߸� �����Ϸ��� raycast �ʿ�?

    // �߻� 1.
    // �� ��ġ�κ��� �ܰ� ������ raycast(obstacle)
    // hit1�κ��� �� ��ġ�� raycastAll(space)
    // hit2�� ���̴� Passable
    // ���� Space�� Impassable

    // �߻� 2. => ���� ���� raycast
    // �� ��ġ�κ��� �ܰ� ������ raycast(obstacle)
    // hit Space ��ȣ�� �극����
    // ���� Space�� Impassable

    // �߻� 2-1 => raycast ����
    // �� �ܰ����κ��� �ٸ� �ܰ� ��ġ�� �극����
    // space���� ������� ��ȸ�ϸ� unsable�� �ƴ϶�� ����, �׶����� spaces���� ���� passable

    // �߻� 3. => A* ��� �̵��� ����
    // ������ Space�� Usable���� �˻�
    // Usable�ϴٸ� ���� Passable
    // �ƴ϶�� ���� Impassable

    // i-n-1  i-1   i+n-1
    //  i-n    i     i+n
    // i-n+1  i+1   i+n+1
    [Flags] enum Plane 
    { 
        Top = 0b0001,
        Bottom = 0b0010,
        Left = 0b0100,
        Right = 0b1000,
        TopLeft = Top + Left,
        BottomLeft = Bottom + Left,
        TopRight = Top + Right,
        BottomRight = Bottom + Right
    }

    public void SettingRoutes()
    {
        // �ε��� ��ȣ, �����¿� ��
        List<(int index, Plane plane)> outlineIndexList = new();

        // ��
        for (int i = 1; i < spaceLine - 1; i++)
            outlineIndexList.Add((i * spaceLine, Plane.Top));
        // ��
        for (int i = 1; i < spaceLine - 1; i++)
            outlineIndexList.Add((i * spaceLine + spaceLine - 1, Plane.Bottom));
        // ��
        for (int i = 1; i < spaceLine - 1; i++)
            outlineIndexList.Add((i, Plane.Left));
        // ��
        for (int i = 1; i < spaceLine - 1; i++)
            outlineIndexList.Add(((i + 1) * spaceLine - 1, Plane.Right));

        // �»�
        outlineIndexList.Add((0, Plane.TopLeft));
        // ����
        outlineIndexList.Add((spaceLine - 1, Plane.BottomLeft));
        // ���
        outlineIndexList.Add((spaceLine * (spaceLine - 1), Plane.TopRight));
        // ����
        outlineIndexList.Add((spaceLine * (spaceLine - 1) + spaceLine - 1, Plane.BottomRight));

        for(int i = 0; i < outlineIndexList.Count; i++)
        {
            int iRow = i / spaceLine;
            int iCol = i % spaceLine;

            for (int j = i + 1; j < outlineIndexList.Count; j++)
            {
                if ((outlineIndexList[i].plane & outlineIndexList[j].plane) == 0b0000)
                    continue;

                int jRow = j / spaceLine;
                int jCol = j % spaceLine;

                int rowDiff = iRow - jRow;
                int colDiff = iCol - jCol;

                float ascend = colDiff / rowDiff;


            }
        }
    }

    void AddPassableOrNot(int from, int to)
    {
        if (WholeSpaces[from].Usable && WholeSpaces[to].Usable)
        {
            WholeSpaces[from].PassableSpaces.Add(WholeSpaces[to].SpaceIndex);
            WholeSpaces[to].PassableSpaces.Add(WholeSpaces[from].SpaceIndex);
        }
        else
        {
            WholeSpaces[from].ImpassableSpaces.Add(WholeSpaces[to].SpaceIndex);
            WholeSpaces[to].ImpassableSpaces.Add(WholeSpaces[from].SpaceIndex);
        }
    }

    /// <summary>
    /// �Ϲ��� ���� ���� ����Ʈ�� ��ȯ
    /// </summary>
    /// <param name="passableSpaceIndex">�̵� �����ؾ� �ϴ� ���� �ε���</param>
    /// <param name="impassableSpaceIndex">�̵� �Ұ����ؾ� �ϴ� ���� �ε���</param>
    /// <returns>�Ϲ��� ���� ���� ����Ʈ</returns>
    public List<int> GetOneWaySpaces(int passableSpaceIndex, int impassableSpaceIndex)
    {
        List<int> spaces = new();

        foreach (Space space in WholeSpaces)
        {
            if (!space.Usable)
                continue;
            if (WholeSpaces[passableSpaceIndex].ImpassableSpaces.Contains(space.SpaceIndex))
                continue;
            if (WholeSpaces[impassableSpaceIndex].PassableSpaces.Contains(space.SpaceIndex))
                continue;
            spaces.Add(space.SpaceIndex);
        }

        return spaces;
    }

    /// <summary>
    /// �ֹ��� ���� ���� �ε��� ����Ʈ�� ��ȯ
    /// </summary>
    /// <param name="passableSpaceAIndex">�̵� �����ؾ� �ϴ� ����1 �ε���</param>
    /// <param name="passableSpaceBIndex">�̵� �����ؾ� �ϴ� ����2 �ε���</param>
    /// <returns>�ֹ��� ���� ���� �ε��� ����Ʈ</returns>
    public List<int> GetTwoWaySpaces(int passableSpaceAIndex, int passableSpaceBIndex)
    {
        List<int> spaces = new();

        foreach (Space space in WholeSpaces)
        {
            if (!space.Usable)
                continue;
            if (WholeSpaces[passableSpaceAIndex].ImpassableSpaces.Contains(space.SpaceIndex))
                continue;
            if (WholeSpaces[passableSpaceBIndex].ImpassableSpaces.Contains(space.SpaceIndex))
                continue;
            spaces.Add(space.SpaceIndex);
        }

        return spaces;
    }

    /// <summary>
    /// Ư�� ��ġ�κ��� ���� ����� ���� �ε����� ��ȯ
    /// </summary>
    /// <param name="targetTransform">�Ÿ� ���� ��ġ</param>
    /// <param name="searchSpaces">Ž�� ���� ����Ʈ</param>
    /// <param name="exceptList">���� ���� �ε��� ����Ʈ</param>
    /// <returns>Ư�� ��ġ�κ��� ���� ����� ���� �ε���</returns>
    public int GetNearestSpace(int targetSpaceIndex, List<int> searchSpaces, List<int> exceptList)
    {
        if (searchSpaces.Count == 0)
            return -1;
        int nearestIndex = searchSpaces[0];
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < searchSpaces.Count; i++)
        {
            if (exceptList.Contains(searchSpaces[i]))
                continue;
            float distanceDiff = Vector3.Distance(WholeSpaces[targetSpaceIndex].Position, WholeSpaces[searchSpaces[i]].Position);
            if (distanceDiff < nearestDistance)
            {
                nearestIndex = searchSpaces[i];
                nearestDistance = distanceDiff;
            }
        }

        return nearestIndex;
    }
}
