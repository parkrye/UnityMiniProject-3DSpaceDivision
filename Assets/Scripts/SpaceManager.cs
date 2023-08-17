using System.Collections.Generic;
using UnityEngine;

public class SpaceManager : MonoBehaviour
{
    [SerializeField] List<Space> wholeSpace;
    public List<Space> WholeSpaces { get { return wholeSpace; } set { wholeSpace = value; } }

    [SerializeField][Range(1f, 5f)] float spaceSize;
    [SerializeField][Range(2, 20)] int spaceLine;
    [SerializeField] float radius;
    [SerializeField] GameObject spacePrefab;

    void Awake()
    {
        WholeSpaces = new List<Space>();
        SettingSpaces();
    }

    void Start()
    {
        SettingRoutes();
    }

    void SettingSpaces()
    {
        int index = 0;
        for (int i = (int)(-spaceLine * 0.5f); i < (int)(spaceLine * 0.5f); i++)
        {
            for (int j = (int)(-spaceLine * 0.5f); j < (int)(spaceLine * 0.5f); j++)
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

    // �߻� 1.
    // �� ��ġ�κ��� �ܰ� ������ raycast(obstacle)
    // hit1�κ��� �� ��ġ�� raycastAll(space)
    // hit2�� ���̴� Passable
    // ���� Space�� Impassable

    // �߻� 2.
    // �� ��ġ�κ��� �ܰ� ������ raycast(obstacle)
    // hit Space ��ȣ�� �긮����
    // ���� Space�� Impassable

    void SettingRoutes()
    {
        for (int i = 0; i < WholeSpaces.Count; i++)
        {
            if (!WholeSpaces[i].Usable)
                continue;

            for (int j = i + 1; j < WholeSpaces.Count; j++)
            {
                if (!WholeSpaces[j].Usable)
                    continue;

                if (Physics.SphereCast(WholeSpaces[i].Position, radius, (WholeSpaces[j].Position - WholeSpaces[i].Position).normalized, out _, Vector3.Distance(WholeSpaces[j].Position, WholeSpaces[i].Position), LayerMask.GetMask("Obstacle")))
                {
                    WholeSpaces[i].ImpassableSpaces.Add(WholeSpaces[j].SpaceIndex);
                    WholeSpaces[j].ImpassableSpaces.Add(WholeSpaces[i].SpaceIndex);
                }
                else
                {
                    WholeSpaces[i].PassableSpaces.Add(WholeSpaces[j].SpaceIndex);
                    WholeSpaces[j].PassableSpaces.Add(WholeSpaces[i].SpaceIndex);
                }
            }
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
