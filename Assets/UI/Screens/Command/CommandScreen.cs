using UnityEngine;
using UnityEngine.UIElements;

public class CommandScreen : UIScreen
{
    protected override UIState ActiveState => UIState.Command;

    [SerializeField] private Transform highlight;

    public override void Generate()
    {
        VisualElement container = new();
        container.AddToClassList("command-container");
        root.Add(container);

        CreateBoatContainer(container);
    }

    private void CreateBoatContainer(VisualElement _parent)
    {
        VisualElement boatContainer = new();
        boatContainer.AddToClassList("command-boat-container");
        _parent.Add(boatContainer);

        foreach(AIBoatController boatController in PlayerBoatController.Instance.AdmiralController.Subordinates)
        {
            CreateBoat(boatController);
        }
    }

    private void CreateBoat(AIBoatController _boatController)
    {

    }
}