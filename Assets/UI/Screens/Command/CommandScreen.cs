using NUnit.Framework;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

public class CommandScreen : UIScreen
{
    protected override UIState ActiveState => UIState.Command;

    private ScrollView boatItemContainer;

    private const float Y_POSITION_FOR_HIGHLIGHTS = 7f;

    [SerializeField] private MeshRenderer highlightPrefab, wayPointPrefab;
    [SerializeField] private Transform trailPrefab;
    [SerializeField] private Material playerMaterial, activeMaterial, inactiveMaterial;

    private MeshRenderer playerHighlight;
    private readonly Dictionary<AIBoatController, CommandItem> commandItems = new();
    private AIBoatController current;
    private bool canClickWater = true;

    private InputSystem_Actions input;

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
        if (UIManager.Instance.State == UIState.Command)
        {
            playerHighlight.transform.position = GetPositionAboveWater(PlayerBoatController.Instance.transform.position);

            foreach (CommandItem item in commandItems.Values)
            {
                item.SetHighlight();
                item.SetWaypoint();

                if (item.IsTrailMoving) item.MoveTrail();
                else if (item.CanMoveTrail()) item.StartMoveTrail();
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

        else if (current != null && canClickWater)
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
            AssignCurrent(boatController);
        }
    }

    private void AssignCurrent(AIBoatController _boatController)
    {
        current = _boatController;
        commandItems[current].Select(activeMaterial);
    }

    private void SetCurrentFormation(Ray _ray)
    {
        float theta = Vector3.Angle(-Vector3.up, _ray.direction);
        float adjacent = Camera.main.transform.position.y;
        float hypotenuse = adjacent / Mathf.Cos(theta * Mathf.Deg2Rad);
        Vector3 hit = _ray.origin + (_ray.direction * hypotenuse);
        current.SetFormationPosition(PlayerBoatController.Instance.transform.InverseTransformVector(hit - PlayerBoatController.Instance.transform.position));
        current.SetCommand(Command.Formation);
        commandItems[current].SetDescription();
        commandItems[current].StopMoveTrail();
    }

    private void CommandDeselect_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        TryRemoveCurrent();
    }

    private void TryRemoveCurrent()
    {
        if (current != null)
        {
            commandItems[current].Deselect(inactiveMaterial);
            current = null;
        }
    }

    public static Vector3 GetPositionAboveWater(Vector3 _position)
    {
        _position.y = Y_POSITION_FOR_HIGHLIGHTS;

        return _position;
    }

    public override void Generate()
    {
        VisualElement container = new();
        container.AddToClassList("command-container");
        root.Add(container);

        Box background = new();
        background.AddToClassList("command-background");
        container.Add(background);

        boatItemContainer = new();
        boatItemContainer.AddToClassList("command-boat-item-container");
        boatItemContainer.RegisterCallback<MouseEnterEvent>(evt => canClickWater = false);
        boatItemContainer.RegisterCallback<MouseLeaveEvent>(evt => canClickWater = true);
        background.Add(boatItemContainer);

        CreatePlayerItem(boatItemContainer, PlayerBoatController.Instance.Boat);
    }

    private void CreatePlayerItem(VisualElement _parent, Boat _boat)
    {
        Button button = new(() => OnPlayerItem(_boat));
        button.AddToClassList("command-boat-item");
        SetBorder(button, playerMaterial.color);
        _parent.Add(button);

        Label header = new(_boat.Name);
        header.AddToClassList("command-boat-item-header");
        SetFontSize(header, 25);
        button.Add(header);
    }

    private Button CreateSuborinateItem(VisualElement _parent, AIBoatController _boatController)
    {
        Button button = new(() => OnSubordinateItem(_boatController));
        button.AddToClassList("command-boat-item");
        SetBorder(button, inactiveMaterial.color);
        _parent.Add(button);

        Label header = new(_boatController.Boat.Name);
        header.AddToClassList("command-boat-item-header");
        SetFontSize(header, 25);
        button.Add(header);

        Label description = new();
        description.AddToClassList("command-boat-item-description");
        SetFontSize(description, 20);
        button.Add(description);

        return button;
    }

    private void OnPlayerItem(Boat _boat)
    {
        CameraManager.Instance.FocusCommandCamera(_boat.transform.position);
    }

    private void OnSubordinateItem(AIBoatController _boatController)
    {
        TryRemoveCurrent();
        AssignCurrent(_boatController);
        CameraManager.Instance.FocusCommandCamera(_boatController.transform.position);
    }

    private void UIManager_OnStateChanged(UIState _state)
    {
        if (_state == UIState.Command)
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
            current = null;

            input.Player.Disable();

            playerHighlight.gameObject.SetActive(false);

            foreach (CommandItem item in commandItems.Values)
            {
                item.Deactivate(inactiveMaterial);
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
            commandItems[_boatController].SetDescription();
        }

        else
        {
            Assert.IsTrue(commandItems.ContainsKey(_boatController));
            commandItems[_boatController].Dispose();
            commandItems.Remove(_boatController);
        }
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

        private const float TRAIL_SPEED = 150f;
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
            Description.text = $"{(BoatController.FormationPosition.HasValue ? "In formation" : "Unassigned")}";
        }

        public void Select(Material _material)
        {
            Highlight.material = _material;
            Waypoint.material = _material;
            SetBorder(Button, _material.color);
        }

        public void Deselect(Material _material)
        {
            Highlight.material = _material;
            Waypoint.material = _material;
            SetBorder(Button, _material.color);
        }

        public void SetHighlight()
        {
            Highlight.transform.position = GetPositionAboveWater(BoatController.transform.position);
        }

        public void SetWaypoint()
        {
            if (BoatController.FormationPosition.HasValue)
            {
                Waypoint.gameObject.SetActive(true);
                Waypoint.transform.position = GetPositionAboveWater(BoatController.GetFormationPositionInWorld());
            }

            else
            {
                Waypoint.gameObject.SetActive(false);
            }
        }

        public bool CanMoveTrail() => BoatController.FormationPosition.HasValue;

        public void StartMoveTrail()
        {
            Trail.gameObject.SetActive(true);
            Trail.transform.position = BoatController.transform.position;
            IsTrailMoving = true;
        }

        public void MoveTrail()
        {
            if (!CanMoveTrail() || Vector3.Distance(Trail.transform.position, Waypoint.transform.position) < 1)
            {
                StopMoveTrail();
                return;
            }

            Trail.transform.position = GetPositionAboveWater(Vector3.MoveTowards(Trail.transform.position, Waypoint.transform.position, TRAIL_SPEED * Time.deltaTime));
        }

        public void StopMoveTrail()
        {
            IsTrailMoving = false;
            Trail.gameObject.SetActive(false);
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