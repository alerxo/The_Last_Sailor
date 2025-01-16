using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UIElements;

public class FormationScreen : UIScreen
{
    protected override List<UIState> ActiveStates => new() { UIState.Formation };

    private const float Y_POSITION_FOR_HIGHLIGHTS = 7f;
    [SerializeField] private LayerMask wayPointLayer;

    [SerializeField] private GameObject highlightPrefab, wayPointPrefab, holdOriginPrefab;
    [SerializeField] private Transform trailPrefab;
    [SerializeField] private Material playerMaterial, formationMaterial, holdMaterial, chargeMaterial;

    private GameObject playerShipHighlight, holdOrigin;
    private readonly Dictionary<AIBoatController, CommandItem> commandItems = new();

    private InputSystem_Actions input;
    public static bool IsHoverinfBoatList { get; private set; }
    private readonly List<CommandItem> SelectedWayPoints = new();
    private Vector3 wayPointMoveOrigin;

    private const int HIGHLIGHT_ROTATION_SPEED = 20;

    private void Awake()
    {
        IsHoverinfBoatList = false;

        UIManager.OnStateChanged += UIManager_OnStateChanged;

        input = new();
        input.Player.WayPointSelect.started += WayPointSelect_started;
        input.Player.WayPointSelect.canceled += WayPointSelect_canceled;
        input.Player.WayPointDeselect.performed += WayPointDeselect_performed;

        playerShipHighlight = Instantiate(highlightPrefab, transform);

        foreach (MeshRenderer meshRenderer in playerShipHighlight.GetComponentsInChildren<MeshRenderer>())
        {
            meshRenderer.material = playerMaterial;
        }

        playerShipHighlight.SetActive(false);

        holdOrigin = Instantiate(holdOriginPrefab, transform);

        foreach (MeshRenderer meshRenderer in holdOrigin.GetComponentsInChildren<MeshRenderer>())
        {
            meshRenderer.material = holdMaterial;
        }

        holdOrigin.SetActive(false);
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

        input.Player.Disable();
        input.Player.WayPointSelect.started -= WayPointSelect_started;
        input.Player.WayPointSelect.canceled -= WayPointSelect_canceled;
        input.Player.WayPointDeselect.performed -= WayPointDeselect_performed;
    }

    private void Update()
    {
        if (UIManager.Instance.GetState() == UIState.Formation)
        {
            playerShipHighlight.transform.position = GetPositionAboveWater(PlayerBoatController.Instance.transform.position);

            foreach (CommandItem item in commandItems.Values)
            {
                item.Update(formationMaterial, holdMaterial, chargeMaterial);
            }

            if (SelectedWayPoints.Count > 0)
            {
                Vector3 movement = GetRayHitOnWaterSurface(Camera.main.ScreenPointToRay(Input.mousePosition)) - wayPointMoveOrigin;

                foreach (CommandItem item in SelectedWayPoints)
                {
                    item.SetWayPointMovePosition(movement);
                }
            }
        }

        else if (SelectedWayPoints.Count > 0)
        {
            foreach (CommandItem item in SelectedWayPoints)
            {
                item.SetWayPointMovePosition(null);
            }

            SelectedWayPoints.Clear();
        }
    }

    private void WayPointSelect_started(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        if (IsHoverinfBoatList || PlayerBoatController.Instance.AdmiralController.Command != Command.Follow) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, wayPointLayer))
        {
            wayPointMoveOrigin = GetRayHitOnWaterSurface(ray);
            SelectedWayPoints.Add(commandItems.Values.ToList().Find((c) => hit.transform == c.WayPoint.transform));
        }
    }

    private Vector3 GetRayHitOnWaterSurface(Ray _ray)
    {
        float theta = Vector3.Angle(-Vector3.up, _ray.direction);
        float adjacent = Camera.main.transform.position.y;
        float hypotenuse = adjacent / Mathf.Cos(theta * Mathf.Deg2Rad);

        Vector3 hit = _ray.origin + (_ray.direction * hypotenuse);
        hit = PlayerBoatController.Instance.transform.InverseTransformVector(hit - PlayerBoatController.Instance.transform.position);
        hit.y = 0;

        return hit;
    }

    private void WayPointSelect_canceled(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        foreach (CommandItem item in SelectedWayPoints)
        {
            item.ApplyWayPointMovePosition();
        }

        SelectedWayPoints.Clear();
    }

    private void WayPointDeselect_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        foreach (CommandItem item in SelectedWayPoints)
        {
            item.SetWayPointMovePosition(null);
        }

        SelectedWayPoints.Clear();
    }

    public override void Generate()
    {
        VisualElement container = new();
        container.AddToClassList("formation-container");
        container.pickingMode = PickingMode.Ignore;
        Root.Add(container);
    }

    private void UIManager_OnStateChanged(UIState _state)
    {
        if (_state == UIState.Formation)
        {
            input.Player.Enable();
            playerShipHighlight.gameObject.SetActive(true);

            foreach (CommandItem item in commandItems.Values)
            {
                item.Activate();
            }
        }

        else
        {
            input.Player.Disable();
            playerShipHighlight.gameObject.SetActive(false);

            foreach (CommandItem item in commandItems.Values)
            {
                item.Deactivate();
            }
        }
    }

    private void AdmiralController_OnSubordinateChanged(AIBoatController _boatController, bool _wasAdded)
    {
        if (_wasAdded)
        {
            Assert.IsFalse(commandItems.ContainsKey(_boatController));
            commandItems.Add(_boatController, new CommandItem(
                _boatController,
                Instantiate(highlightPrefab, transform),
                Instantiate(wayPointPrefab, transform),
                Instantiate(trailPrefab, transform)));
            commandItems[_boatController].Deactivate();
        }

        else
        {
            Assert.IsTrue(commandItems.ContainsKey(_boatController));
            commandItems[_boatController].Dispose();
            commandItems.Remove(_boatController);
        }
    }

    public static Vector3 GetPositionAboveWater(Vector3 _position)
    {
        _position.y = Y_POSITION_FOR_HIGHLIGHTS;

        return _position;
    }

    private class CommandItem
    {
        public AIBoatController BoatController;
        public GameObject Highlight;
        public GameObject WayPoint;
        public Transform Trail;
        public Vector3? WayPointMovePosition;

        private const float TRAIL_SPEED = 170f;

        public bool IsTrailMoving { get; private set; } = false;

        public CommandItem(AIBoatController _boatController, GameObject _highlight, GameObject _wayPoint, Transform _trail)
        {
            BoatController = _boatController;
            Highlight = _highlight;
            WayPoint = _wayPoint;
            Trail = _trail;
        }

        public void Update(Material _formationMaterial, Material _holdMaterial, Material _chargeMaterial)
        {
            if (BoatController.Boat.IsSunk)
            {
                HideWaypoint();
                Deactivate();

                return;
            }

            switch (BoatController.Command)
            {
                case Command.Follow:
                    SetMaterial(_formationMaterial);
                    SetHighlightPosition();
                    SetWaypointPositionAtFormation();
                    MoveTrail();
                    break;

                case Command.Wait:
                    SetMaterial(_holdMaterial);
                    SetHighlightPosition();
                    SetWaypointPositionAtHold();
                    MoveTrail();
                    break;

                case Command.Charge:
                    SetMaterial(_chargeMaterial);
                    SetHighlightPosition();
                    HideWaypoint();
                    StopMoveTrail();
                    break;

                default:
                    Debug.LogWarning("Defaulted");
                    break;
            }
        }

        private void SetMaterial(Material _material)
        {
            if (_material != Highlight.GetComponentInChildren<MeshRenderer>().material)
            {
                foreach (MeshRenderer meshRenderer in Highlight.GetComponentsInChildren<MeshRenderer>())
                {
                    meshRenderer.material = _material;
                }

                foreach (MeshRenderer meshRenderer in WayPoint.GetComponentsInChildren<MeshRenderer>())
                {
                    meshRenderer.material = _material;
                }
            }
        }

        private void SetHighlightPosition()
        {
            Highlight.transform.position = GetPositionAboveWater(BoatController.transform.position);

            foreach (MeshRenderer meshRenderer in Highlight.GetComponentsInChildren<MeshRenderer>())
            {
                if (meshRenderer.CompareTag("FormationHighlightCanRotate"))
                {
                    meshRenderer.transform.Rotate(new Vector3(0, 0, HIGHLIGHT_ROTATION_SPEED) * Time.deltaTime);
                }
            }

            ShowHighlight();
        }

        private void SetWaypointPositionAtFormation()
        {
            WayPoint.transform.position = GetPositionAboveWater(BoatController.GetPositionRelativeToAdmiral(BoatController.FormationPosition.Value + (WayPointMovePosition ?? Vector3.zero)));

            foreach (MeshRenderer meshRenderer in WayPoint.GetComponentsInChildren<MeshRenderer>())
            {
                if (meshRenderer.CompareTag("FormationHighlightCanRotate"))
                {
                    meshRenderer.transform.Rotate(new Vector3(0, 0, HIGHLIGHT_ROTATION_SPEED) * Time.deltaTime);
                }
            }

            ShowWayPoint();
        }

        private void SetWaypointPositionAtHold()
        {
            WayPoint.transform.position = GetPositionAboveWater(BoatController.HoldPosition.Value + (WayPointMovePosition ?? Vector3.zero));
            ShowWayPoint();
        }

        private void ShowHighlight()
        {
            Highlight.SetActive(true);
        }

        private void ShowWayPoint()
        {
            WayPoint.transform.localScale = WayPointMovePosition.HasValue ? Vector3.one * 1.5f : Vector3.one;
            WayPoint.SetActive(true);
            Trail.gameObject.SetActive(true);
        }

        private void HideWaypoint()
        {
            WayPoint.SetActive(false);
            Trail.gameObject.SetActive(false);
        }

        private bool CanMoveTrail() => BoatController.FormationPosition.HasValue;

        private void MoveTrail()
        {
            if (!CanMoveTrail() || Vector3.Distance(Trail.transform.position, WayPoint.transform.position) < 1)
            {
                StopMoveTrail();
                return;
            }

            if (IsTrailMoving)
            {
                Trail.transform.position = GetPositionAboveWater(Vector3.MoveTowards(Trail.transform.position, WayPoint.transform.position, TRAIL_SPEED * Time.deltaTime));
            }

            else if (CanMoveTrail())
            {
                Trail.gameObject.SetActive(true);
                Trail.transform.position = BoatController.transform.position;
                IsTrailMoving = true;
            }
        }

        public void StopMoveTrail()
        {
            if (IsTrailMoving)
            {
                IsTrailMoving = false;
                Trail.gameObject.SetActive(false);
            }
        }

        public void SetWayPointMovePosition(Vector3? _point)
        {
            WayPointMovePosition = _point;
        }

        public void ApplyWayPointMovePosition()
        {
            BoatController.SetFormationPosition(BoatController.FormationPosition + WayPointMovePosition);
            WayPointMovePosition = null;
        }

        public void Activate()
        {
            WayPoint.SetActive(true);
            Trail.gameObject.SetActive(true);
            Highlight.SetActive(true);
        }

        public void Deactivate()
        {
            WayPoint.SetActive(false);
            Trail.gameObject.SetActive(false);
            Highlight.SetActive(false);
            StopMoveTrail();
        }

        public void Dispose()
        {
            Destroy(WayPoint);
            Destroy(Trail.gameObject);
        }
    }
}