using System.Collections.Generic;
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

    private void Awake()
    {
        UIManager.OnStateChanged += UIManager_OnStateChanged;

        input = new();
        input.Player.CommandSelect.performed += CommandSelect_performed;
        input.Player.CommandDeselect.performed += CommandDeselect_performed;

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

        input.Player.CommandSelect.performed -= CommandSelect_performed;
        input.Player.CommandDeselect.performed -= CommandDeselect_performed;
        input.Player.Disable();
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
        }
    }

    private void CommandSelect_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {

    }

    private void CommandDeselect_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        
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
        public MeshRenderer Waypoint;
        public Transform Trail;
        public Button Button;
        public Label Header;
        public Label Description;

        private const float TRAIL_SPEED = 170f;
        public bool IsTrailMoving { get; private set; } = false;

        public CommandItem(AIBoatController _boatController, MeshRenderer _highlight, MeshRenderer _waypoint, Transform _trail, Button _button)
        {
            BoatController = _boatController;
            Highlight = _highlight;
            Waypoint = _waypoint;
            Trail = _trail;
            Button = _button;
            Header = Button.ElementAt(0) as Label;
            Description = Button.ElementAt(1) as Label;
        }

        public void SetDescription()
        {
            Description.text = $"{BoatController.Command}";
        }

        public void Update(Material _formationMaterial, Material _holdMaterial, Material _chargeMaterial)
        {
            if (Description.text != $"{BoatController.Command}")
            {
                Description.text = $"{BoatController.Command}";
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
                Waypoint.material = _material;
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
            Waypoint.transform.position = GetPositionAboveWater(BoatController.GetFormationPositionInWorld());
            ShowWaypoint();
        }

        private void SetWaypointPositionAtHold()
        {
            Waypoint.transform.position = GetPositionAboveWater(BoatController.HoldPosition.Value);
            ShowWaypoint();
        }

        private void ShowHighlight()
        {
            Highlight.gameObject.SetActive(true);
        }

        private void ShowWaypoint()
        {
            Waypoint.gameObject.SetActive(true);
            Trail.gameObject.SetActive(true);
        }

        private void HideWaypoint()
        {
            Waypoint.gameObject.SetActive(false);
            Trail.gameObject.SetActive(false);
        }

        private bool CanMoveTrail() => BoatController.FormationPosition.HasValue;

        private void MoveTrail()
        {
            if (!CanMoveTrail() || Vector3.Distance(Trail.transform.position, Waypoint.transform.position) < 1)
            {
                StopMoveTrail();
                return;
            }

            if (IsTrailMoving)
            {
                Trail.transform.position = GetPositionAboveWater(Vector3.MoveTowards(Trail.transform.position, Waypoint.transform.position, TRAIL_SPEED * Time.deltaTime));
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

        public void Activate()
        {
            Waypoint.gameObject.SetActive(true);
            Trail.gameObject.SetActive(true);
            Highlight.gameObject.SetActive(true);
        }

        public void Deactivate(Material _material)
        {
            Waypoint.gameObject.SetActive(false);
            Trail.gameObject.SetActive(false);
            Highlight.gameObject.SetActive(false);
            Highlight.material = _material;
            Waypoint.material = _material;
            SetBorder(Button, _material.color);
            StopMoveTrail();
        }

        public void Dispose()
        {
            Button.RemoveFromHierarchy();
            Destroy(Waypoint.gameObject);
            Destroy(Trail.gameObject);
        }
    }
}