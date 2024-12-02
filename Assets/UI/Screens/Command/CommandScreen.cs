using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CommandScreen : UIScreen
{
    protected override UIState ActiveState => UIState.Command;

    private const float Y_POSITION_FOR_HIGHLIGHTS = 10f;

    [SerializeField] private MeshRenderer highlightPrefab, wayPointPrefab;
    [SerializeField] private Material activeMaterial, inactiveMaterial;

    private MeshRenderer highlight;
    private readonly Dictionary<AIBoatController, MeshRenderer> wayPoints = new();
    private AIBoatController current;

    private InputSystem_Actions input;

    private void Awake()
    {
        UIManager.OnStateChanged += UIManager_OnStateChanged;

        input = new();
        input.Player.CommandSelect.performed += CommandSelect_performed;
        input.Player.CommandDeselect.performed += CommandDeselect_performed;

        highlight = Instantiate(highlightPrefab, transform);
        highlight.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (PlayerBoatController.Instance != null && PlayerBoatController.Instance.AdmiralController != null)
        {
            PlayerBoatController.Instance.AdmiralController.OnSubordinateChanged += AdmiralController_OnSubordinateChanged;
        }
    }

    private void OnDestroy()
    {
        UIManager.OnStateChanged -= UIManager_OnStateChanged;

        if (PlayerBoatController.Instance != null && PlayerBoatController.Instance.AdmiralController != null)
        {
            PlayerBoatController.Instance.AdmiralController.OnSubordinateChanged -= AdmiralController_OnSubordinateChanged;
        }

        input.Player.CommandSelect.performed -= CommandSelect_performed;
        input.Player.CommandDeselect.performed -= CommandDeselect_performed;

        input.Player.Disable();
    }

    private void Update()
    {
        if (UIManager.Instance.State == UIState.Command)
        {
            if (current != null)
            {
                highlight.gameObject.SetActive(true);
                highlight.transform.position = GetPositionAboveWater(current.transform.position);
            }

            else
            {
                highlight.gameObject.SetActive(false);
            }

            SetWayPoints();
        }

        else
        {
            highlight.gameObject.SetActive(false);
        }
    }

    private void UIManager_OnStateChanged(UIState _state)
    {
        if (_state == UIState.Command)
        {
            input.Player.Enable();

            foreach (MeshRenderer wayPoint in wayPoints.Values)
            {
                wayPoint.gameObject.SetActive(true);
            }
        }

        else
        {
            input.Player.Disable();

            foreach (MeshRenderer wayPoint in wayPoints.Values)
            {
                wayPoint.gameObject.SetActive(false);
                wayPoint.material = inactiveMaterial;
            }

            current = null;
        }
    }

    private void AdmiralController_OnSubordinateChanged(AIBoatController _boatController, bool wasAdded)
    {
        if (wasAdded)
        {
            Assert.IsFalse(wayPoints.ContainsKey(_boatController));
            wayPoints.Add(_boatController, Instantiate(wayPointPrefab, transform));
        }

        else
        {
            Assert.IsTrue(wayPoints.ContainsKey(_boatController));
            Destroy(wayPoints[_boatController].gameObject);
            wayPoints.Remove(_boatController);
        }
    }

    private void SetWayPoints()
    {
        foreach (KeyValuePair<AIBoatController, MeshRenderer> wayPoint in wayPoints)
        {
            if (wayPoint.Key.FormationPosition.HasValue)
            {
                wayPoint.Value.gameObject.SetActive(true);
                wayPoint.Value.transform.position = GetPositionAboveWater(wayPoint.Key.GetFormationPositionInWorld());
            }

            else
            {
                wayPoint.Value.gameObject.SetActive(false);
            }
        }
    }

    private void CommandSelect_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit assignmentHit))
        {
            TryAssignCurrent(assignmentHit);
        }

        else if (current != null)
        {
            SetCurrentFormation(ray);
        }
    }

    private void TryAssignCurrent(RaycastHit _hit)
    {
        AIBoatController boatController = _hit.transform.GetComponentInParent<AIBoatController>();

        if (boatController != null && boatController != current && boatController.Admiral == PlayerBoatController.Instance.AdmiralController)
        {
            TryRemoveCurrent();

            current = boatController;
            wayPoints[current].material = activeMaterial;
        }
    }

    private void SetCurrentFormation(Ray _ray)
    {
        float theta = Vector3.Angle(-Vector3.up, _ray.direction);
        float adjacent = Camera.main.transform.position.y;
        float hypotenuse = adjacent / Mathf.Cos(theta * Mathf.Deg2Rad);
        Vector3 hit = _ray.origin + (_ray.direction * hypotenuse);
        current.SetFormationPosition(PlayerBoatController.Instance.transform.InverseTransformVector(hit - PlayerBoatController.Instance.transform.position));
    }

    private void CommandDeselect_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        TryRemoveCurrent();
    }

    private void TryRemoveCurrent()
    {
        if (current != null)
        {
            wayPoints[current].material = inactiveMaterial;
            current = null;
            highlight.transform.position = ObjectPoolManager.InactiveObjectPosition;
        }
    }

    private Vector3 GetPositionAboveWater(Vector3 _position)
    {
        _position.y = Y_POSITION_FOR_HIGHLIGHTS;

        return _position;
    }

    public override void Generate()
    {
        VisualElement container = new();
        container.AddToClassList("command-container");
        root.Add(container);
    }
}