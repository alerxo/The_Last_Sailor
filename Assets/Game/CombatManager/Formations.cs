using UnityEngine;

public static class Formations
{
    private const float POSITION_FORWARD_OFFSET = 10f;

    private const float LINE_MARGIN = 50f;

    private const float SPEARHEAD_MARGIN = 100f;

    private const int RING_START_COUNT = 6;
    private const int RING_GROWTH_SIZE = 4;
    private const float RING_MARGIN = 200f;

    public static Vector3[] GetLine(Vector3 _origin, Vector3 _forward, int _count)
    {
        Vector3[] positions = new Vector3[_count];

        for (int i = 0; i < _count; i++)
        {
            positions[i] = _origin + ((i + 1) * LINE_MARGIN * -_forward) + GetForwardOffset(_forward);
        }

        return positions;
    }

    public static Vector3[] GetSpearhead(Transform _transform, int _count)
    {
        Vector3[] positions = new Vector3[_count];

        for (int i = 0; i < _count; i++)
        {
            positions[i] = _transform.position + _transform.TransformVector((GetSpearheadDirection(i) * GetSpearheadDistance(i)) + GetForwardOffset(_transform.forward));
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

    public static Vector3[] GetRing(Transform _transform, int _count)
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

            positions[i] = _transform.position + _transform.TransformVector((GetRingDirection(ringIndex, ringCount) * GetRingDistance(ringIteration)) + GetForwardOffset(_transform.forward));

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

    private static Vector3 GetForwardOffset(Vector3 _forward)
    {
        return POSITION_FORWARD_OFFSET * _forward;
    }
}

public enum Formation
{
    Line,
    Spearhead,
    Ring
}