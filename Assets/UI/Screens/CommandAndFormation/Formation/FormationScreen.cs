using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class FormationScreen : UIScreen
{
    protected override List<UIState> ActiveStates => new() { UIState.Formation };

    private ScrollView boatItemContainer;

    private const float Y_POSITION_FOR_HIGHLIGHTS = 7f;

    [SerializeField] private MeshRenderer highlightPrefab, wayPointPrefab;
    [SerializeField] private Transform trailPrefab;
    [SerializeField] private Material playerMaterial, formationMaterial, holdMaterial, chargeMaterial;

    private MeshRenderer playerHighlight;
    private readonly Dictionary<AIBoatController, CommandItem> commandItems = new();

    private InputSystem_Actions input;
    private bool canClickWater = true;
    private readonly List<CommandItem> SelectedWayPoints = new();
    private Vector3 wayPointMoveOrigin;

    private void Awake()
    {
        UIManager.OnStateChanged += UIManager_OnStateChanged;

        input = new();
        input.Player.WayPointSelect.started += WayPointSelect_started;
        input.Player.WayPointSelect.canceled += WayPointSelect_canceled;
        input.Player.WayPointDeselect.performed += WayPointDeselect_performed;

        playerHighlight = Instantiate(highlightPrefab, transform);
        playerHighlight.material = playerMaterial;
        playerHighlight.gameObject.SetActive(false);
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
        if (UIManager.Instance.State == UIState.Formation)
        {
            playerHighlight.transform.position = GetPositionAboveWater(PlayerBoatController.Instance.transform.position);

            foreach (CommandItem item in commandItems.Values)
            {
                item.Update(formationMaterial, holdMaterial, chargeMaterial);
            }

            if (SelectedWayPoints.Count > 0)
            {
                foreach (CommandItem item in SelectedWayPoints)
                {
                    Vector3 movement = GetRayHitOnWaterSurface(Camera.main.ScreenPointToRay(Input.mousePosition));
                    item.SetWayPointMovePosition(wayPointMoveOrigin + movement);
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
        if (!canClickWater) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.CompareTag("CommandWayPoint"))
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

        return _ray.origin + (_ray.direction * hypotenuse);
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
        Root.Add(container);

        Box background = new();
        background.AddToClassList("formation-background");
        container.Add(background);

        boatItemContainer = new();
        boatItemContainer.AddToClassList("formation-boat-item-container");
        boatItemContainer.RegisterCallback<MouseEnterEvent>(evt => canClickWater = false);
        boatItemContainer.RegisterCallback<MouseLeaveEvent>(evt => canClickWater = true);
        boatItemContainer.verticalScroller.highButton.RemoveFromHierarchy();
        boatItemContainer.verticalScroller.lowButton.RemoveFromHierarchy();
        boatItemContainer.horizontalScroller.RemoveFromHierarchy();
        background.Add(boatItemContainer);

        CreatePlayerItem(boatItemContainer, PlayerBoatController.Instance.Boat);
    }

    private void CreatePlayerItem(VisualElement _parent, Boat _boat)
    {
        Button button = new(() => OnPlayerItem(_boat));
        button.AddToClassList("formation-boat-item");
        SetBorder(button, playerMaterial.color);
        _parent.Add(button);

        Label header = new(_boat.Name);
        header.AddToClassList("formation-boat-item-header");
        SetFontSize(header, 25);
        button.Add(header);
    }

    private Button CreateSuborinateItem(VisualElement _parent, AIBoatController _boatController)
    {
        Button button = new(() => OnSubordinateItem(_boatController));
        button.AddToClassList("formation-boat-item");
        SetBorder(button, formationMaterial.color);
        _parent.Add(button);

        Label header = new(_boatController.Boat.Name);
        header.AddToClassList("formation-boat-item-header");
        SetFontSize(header, 25);
        button.Add(header);

        Label description = new();
        description.AddToClassList("formation-boat-item-description");
        SetFontSize(description, 20);
        button.Add(description);

        return button;
    }

    private void OnPlayerItem(Boat _boat)
    {
        CameraManager.Instance.FocusFormationCamera(_boat.transform.position);
    }

    private void OnSubordinateItem(AIBoatController _boatController)
    {
        CameraManager.Instance.FocusFormationCamera(_boatController.transform.position);
    }

    private void UIManager_OnStateChanged(UIState _state)
    {
        if (_state == UIState.Formation)
        {
            input.Player.Enable();
            playerHighlight.gameObject.SetActive(true);

            foreach (CommandItem item in commandItems.Values)
            {
                item.Activate();
            }
        }

        else
        {
            input.Player.Disable();
            playerHighlight.gameObject.SetActive(false);

            foreach (CommandItem item in commandItems.Values)
            {
                item.Deactivate(playerMaterial);
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
                Instantiate(trailPrefab, transform),
                CreateSuborinateItem(boatItemContainer, _boatController)));
            commandItems[_boatController].Deactivate(playerMaterial);
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
        public MeshRenderer Highlight;
        public MeshRenderer WayPoint;
        public MeshRenderer WayPointMove;
        public Transform Trail;
        public Button Button;
        public Label Header;
        public Label Description;
        public Vector3? WayPointMovePosition;

        private const float TRAIL_SPEED = 170f;
        public bool IsTrailMoving { get; private set; } = false;

        public CommandItem(AIBoatController _boatController, MeshRenderer _highlight, MeshRenderer _wayPoint, Transform _trail, Button _button)
        {
            BoatController = _boatController;
            Highlight = _highlight;
            WayPoint = _wayPoint;
            Trail = _trail;
            Button = _button;
            Header = Button.ElementAt(0) as Label;
            Description = Button.ElementAt(1) as Label;
        }

        public void Update(Material _formationMaterial, Material _holdMaterial, Material _chargeMaterial)
        {
            if (Description.text != $"Durability: {BoatController.Boat.GetPercentageDurability()}")
            {
                Description.text = $"Durability: {BoatController.Boat.GetPercentageDurability()}";
            }

            switch (BoatController.Command)
            {
                case Command.Formation:
                    SetMaterial(_formationMaterial);
                    SetHighlightPosition();
                    SetWaypointPositionAtFormation();
                    MoveTrail();
                    break;

                case Command.Hold:
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
            if (_material != Highlight.material)
            {
                Highlight.material = _material;
                WayPoint.material = _material;
                SetBorder(Button, _material.color);
            }
        }

        private void SetHighlightPosition()
        {
            Highlight.transform.position = GetPositionAboveWater(BoatController.transform.position);
            ShowHighlight();
        }

        private void SetWaypointPositionAtFormation()
        {
            WayPoint.transform.position = GetPositionAboveWater(BoatController.GetFormationPositionInWorld() + (WayPointMovePosition ?? Vector3.zero));
            ShowWayPoint();
        }

        private void SetWaypointPositionAtHold()
        {
            WayPoint.transform.position = GetPositionAboveWater(BoatController.HoldPosition.Value + (WayPointMovePosition ?? Vector3.zero));
            ShowWayPoint();
        }

        private void ShowHighlight()
        {
            Highlight.gameObject.SetActive(true);
        }

        private void ShowWayPoint()
        {
            WayPoint.transform.localScale = WayPointMovePosition.HasValue ? Vector3.one * 1.5f : Vector3.one;
            WayPoint.gameObject.SetActive(true);
            Trail.gameObject.SetActive(true);
        }

        private void HideWaypoint()
        {
            WayPoint.gameObject.SetActive(false);
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
            WayPoint.gameObject.SetActive(true);
            Trail.gameObject.SetActive(true);
            Highlight.gameObject.SetActive(true);
        }

        public void Deactivate(Material _material)
        {
            WayPoint.gameObject.SetActive(false);
            Trail.gameObject.SetActive(false);
            Highlight.gameObject.SetActive(false);
            Highlight.material = _material;
            WayPoint.material = _material;
            SetBorder(Button, _material.color);
            StopMoveTrail();
        }

        public void Dispose()
        {
            Button.RemoveFromHierarchy();
            Destroy(WayPoint.gameObject);
            Destroy(Trail.gameObject);
        }
    }
}