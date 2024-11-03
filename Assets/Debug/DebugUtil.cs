using UnityEngine;

public static class DebugUtil
{
    public static void DrawBox(Vector3 _position, Quaternion _rotation, Vector3 _scale, Color _color, float _duration)
    {
        Matrix4x4 transform = new();
        transform.SetTRS(_position, _rotation, _scale);

        Vector3 point1 = transform.MultiplyPoint(new Vector3(-0.5f, -0.5f, 0.5f));
        Vector3 point2 = transform.MultiplyPoint(new Vector3(0.5f, -0.5f, 0.5f));
        Vector3 point3 = transform.MultiplyPoint(new Vector3(0.5f, -0.5f, -0.5f));
        Vector3 point4 = transform.MultiplyPoint(new Vector3(-0.5f, -0.5f, -0.5f));

        Vector3 point5 = transform.MultiplyPoint(new Vector3(-0.5f, 0.5f, 0.5f));
        Vector3 point6 = transform.MultiplyPoint(new Vector3(0.5f, 0.5f, 0.5f));
        Vector3 point7 = transform.MultiplyPoint(new Vector3(0.5f, 0.5f, -0.5f));
        Vector3 point8 = transform.MultiplyPoint(new Vector3(-0.5f, 0.5f, -0.5f));

        Debug.DrawLine(point1, point2, _color, _duration);
        Debug.DrawLine(point2, point3, _color, _duration);
        Debug.DrawLine(point3, point4, _color, _duration);
        Debug.DrawLine(point4, point1, _color, _duration);

        Debug.DrawLine(point5, point6, _color, _duration);
        Debug.DrawLine(point6, point7, _color, _duration);
        Debug.DrawLine(point7, point8, _color, _duration);
        Debug.DrawLine(point8, point5, _color, _duration);

        Debug.DrawLine(point1, point5, _color, _duration);
        Debug.DrawLine(point2, point6, _color, _duration);
        Debug.DrawLine(point3, point7, _color, _duration);
        Debug.DrawLine(point4, point8, _color, _duration);
    }
}