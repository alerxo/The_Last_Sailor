using UnityEngine;

public static class Formations
{
    private const float POSITION_FORWARD_OFFSET = 10f;

    private const float LINE_MARGIN = 60f;

    private const float SPEARHEAD_MARGIN = 90f;

    private const int RING_START_COUNT = 6;
    private const int RING_GROWTH_SIZE = 4;
    private const float RING_MARGIN = 300f;

    public static Vector3[] GetFleetPositions(Formation _formation, int _count)
    {
        switch (_formation)
        {
            case Formation.Line:
                return GetLine(_count);

            case Formation.Spearhead:
                return GetSpearhead(_count);

            case Formation.Ring:
                return GetRing(_count);

            default:
                Debug.LogError($"Defaulted for case {_formation}");
                return null;
        }
    }

    public static Vector3[] GetLine(int _count)
    {
        Vector3[] positions = new Vector3[_count];

        for (int i = 0; i < _count; i++)
        {
            positions[i] = ((i + 1) * LINE_MARGIN * -Vector3.forward) + GetForwardOffset();
        }

        return positions;
    }

    public static Vector3[] GetSpearhead(int _count)
    {
        Vector3[] positions = new Vector3[_count];

        for (int i = 0; i < _count; i++)
        {
            positions[i] = (GetSpearheadDirection(i) * GetSpearheadDistance(i)) + GetForwardOffset();
        }

        return positions;
    }

    private static Vector3 GetSpearheadDirection(int _index)
    {
        return new Vector3(_index % 2 == 0 ? 0.5f : -0.5f, 0, -0.5f);
    }

    private static float GetSpearheadDistance(int _index)
    {
        return ((_index / 2) + 1) * SPEARHEAD_MARGIN;
    }

    public static Vector3[] GetRing(int _count)
    {
        Vector3[] positions = new Vector3[_count];

        int ringIndex = 0;
        int ringCount = RING_START_COUNT;
        int ringIteration = 1;

        for (int i = 0; i < _count; i++)
        {
            if (ringIndex == ringCount)
            {
                ringIndex = 0;
                ringCount = Mathf.Clamp(ringCount + RING_GROWTH_SIZE, 0, _count);
                ringIteration++;
            }

            positions[i] = (GetRingDirection(ringIndex, ringCount) * GetRingDistance(ringIteration)) + GetForwardOffset();

            ringIndex++;
        }

        return positions;
    }

    private static Vector3 GetRingDirection(int _ringIndex, int _ringCount)
    {
        float step = 360 / _ringCount;
        float degrees = step * _ringIndex;
        float radians = degrees * Mathf.Deg2Rad;

        return new Vector3(Mathf.Cos(radians) * 0.4f, 0, Mathf.Sin(radians) * 0.6f);
    }

    private static float GetRingDistance(int _ringIteration)
    {
        return ((_ringIteration / 2) + 1) * RING_MARGIN;
    }

    private static Vector3 GetForwardOffset()
    {
        return POSITION_FORWARD_OFFSET * -Vector3.forward;
    }
}

public enum Formation
{
    Line,
    Spearhead,
    Ring
}