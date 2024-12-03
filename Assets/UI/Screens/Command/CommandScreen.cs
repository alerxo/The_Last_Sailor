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
    [SerializeField] private Material playerMaterial, activeMaterial, inactiveMaterial;

    private MeshRenderer playerHighlight, selectedHighlight;
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

        selectedHighlight = Instantiate(highlightPrefab, transform);
        selectedHighlight.gameObject.SetActive(false);
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
        SetHighlights();
    }

    private void SetHighlights()
    {
        if (UIManager.Instance.State == UIState.Command)
        {
            playerHighlight.gameObject.SetActive(true);
            playerHighlight.transform.position = GetPositionAboveWater(PlayerBoatController.Instance.transform.position);

            if (current != null)
            {
                selectedHighlight.gameObject.SetActive(true);
                selectedHighlight.transform.position = GetPositionAboveWater(current.transform.position);
            }

            else
            {
                selectedHighlight.gameObject.SetActive(false);
            }

            SetWayPoints();
        }

        else
        {
            playerHighlight.gameObject.SetActive(false);
            selectedHighlight.gameObject.SetActive(false);
        }
    }

    private void UIManager_OnStateChanged(UIState _state)
    {
        if (_state == UIState.Command)
        {
            input.Player.Enable();

            foreach (CommandItem item in commandItems.Values)
            {
                item.Waypoint.gameObject.SetActive(true);
            }

            Time.timeScale = 0.5f;
        }

        else
        {
            current = null;

            input.Player.Disable();

            foreach (CommandItem item in commandItems.Values)
            {
                item.Waypoint.gameObject.SetActive(false);
                item.Waypoint.material = inactiveMaterial;
                SetBorder(item.Button, inactiveMaterial.color);
            }

            Time.timeScale = 1f;
        }
    }

    private void AdmiralController_OnSubordinateChanged(AIBoatController _boatController, bool _wasAdded)
    {
        if (_wasAdded)
        {
            Assert.IsFalse(commandItems.ContainsKey(_boatController));
            commandItems.Add(_boatController, new CommandItem(Instantiate(wayPointPrefab, transform), CreateSuborinateItem(boatItemContainer, _boatController)));
            commandItems[_boatController].SetDescription(_boatController);
        }

        else
        {
            Assert.IsTrue(commandItems.ContainsKey(_boatController));
            commandItems[_boatController].Button.RemoveFromHierarchy();
            Destroy(commandItems[_boatController].Waypoint.gameObject);
            commandItems.Remove(_boatController);
        }
    }

    private void SetWayPoints()
    {
        foreach (KeyValuePair<AIBoatController, CommandItem> item in commandItems)
        {
            if (item.Key.FormationPosition.HasValue)
            {
                item.Value.Waypoint.gameObject.SetActive(true);
                item.Value.Waypoint.transform.position = GetPositionAboveWater(item.Key.GetFormationPositionInWorld());
            }

            else
            {
                item.Value.Waypoint.gameObject.SetActive(false);
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
        commandItems[current].Waypoint.material = activeMaterial;
        SetBorder(commandItems[current].Button, activeMaterial.color);
    }

    private void SetCurrentFormation(Ray _ray)
    {
        float theta = Vector3.Angle(-Vector3.up, _ray.direction);
        float adjacent = Camera.main.transform.position.y;
        float hypotenuse = adjacent / Mathf.Cos(theta * Mathf.Deg2Rad);
        Vector3 hit = _ray.origin + (_ray.direction * hypotenuse);
        current.SetFormationPosition(PlayerBoatController.Instance.transform.InverseTransformVector(hit - PlayerBoatController.Instance.transform.position));
        commandItems[current].SetDescription(current);
    }

    private void CommandDeselect_performed(UnityEngine.InputSystem.InputAction.CallbackContext _obj)
    {
        TryRemoveCurrent();
    }

    private void TryRemoveCurrent()
    {
        if (current != null)
        {
            commandItems[current].Waypoint.material = inactiveMaterial;
            SetBorder(commandItems[current].Button, inactiveMaterial.color);
            current = null;
            selectedHighlight.transform.position = ObjectPoolManager.InactiveObjectPosition;
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

    private class CommandItem
    {
        public MeshRenderer Waypoint;
        public Button Button;
        public Label Header;
        public Label Description;

        public CommandItem(MeshRenderer _waypoint, Button _button)
        {
            Waypoint = _waypoint;
            Button = _button;
            Header = Button.ElementAt(0) as Label;
            Description = Button.ElementAt(1) as Label;
        }

        public void SetDescription(AIBoatController _boatController)
        {
            Description.text = $"{(_boatController.FormationPosition.HasValue ? "In formation" : "Unassigned")}";
        }
    }
}