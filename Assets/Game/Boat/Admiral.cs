using System.Collections;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public abstract class Admiral : MonoBehaviour
{
    public event UnityAction<AIBoatController, bool> OnSubordinateChanged;
    public event UnityAction<Command> OnCommandChanged;
    public string Name { get; private set; }
    public Boat Owner { get; private set; }

    public Admiral Enemy { get; protected set; }

    public readonly List<Boat> Fleet = new();
    public readonly List<AIBoatController> Subordinates = new();
    public Command Command { get; private set; }
    public void SetOwner(Boat boat)
    {
        Owner = boat;
        Fleet.Add(Owner);
    }

    public void RemoveOwner()
    {
        Fleet.Remove(Owner);
        Owner = null;
    }

    public void AddSubordinate(Boat _boat)
    {
        if (!Fleet.Contains(_boat))
        {
            AIBoatController controller = _boat.GetComponent<AIBoatController>();
            Assert.IsNull(controller.Admiral);
            controller.SetAdmiral(this);
            Subordinates.Add(controller);
            OnSubordinateChanged?.Invoke(controller, true);
            Fleet.Add(_boat);
        }
    }

    public void RemoveSubordinate(Boat _boat)
    {
        if (Fleet.Contains(_boat))
        {
            AIBoatController controller = _boat.GetComponent<AIBoatController>();
            Assert.IsTrue(controller.Admiral == this);
            controller.SetAdmiral(null);
            Subordinates.Remove(controller);
            OnSubordinateChanged?.Invoke(controller, false);
            Fleet.Remove(_boat);
        }
    }

    public abstract string GetSubordinateName();

    public void SetEnemy(Admiral _enemy)
    {
        Enemy = _enemy;
    }

    protected void SetName(string _name)
    {
        Name = _name;
    }

    public void SetCommandForSubordinates(int _command)
    {
        SetCommandForSubordinates((Command)_command);
    }

    public void SetCommandForSubordinates(Command _command)
    {
        foreach (AIBoatController boatController in Subordinates)
        {
            if (!boatController.Boat.IsSunk)
            {
                switch (_command)
                {
                    case Command.Unassigned:
                        boatController.SetCommand(Command.Unassigned);
                        break;

                    case Command.Formation when boatController.FormationPosition.HasValue:
                        boatController.SetCommand(Command.Formation);
                        break;

                    case Command.Hold when boatController.FormationPosition.HasValue:
                        boatController.SetCommand(Command.Hold);
                        break;

                    case Command.Charge:
                        boatController.SetCommand(Command.Charge);
                        break;

                    default:
                        boatController.SetCommand(Command.Unassigned);
                        break;
                }
            }
        }

        Command = _command;
        OnCommandChanged?.Invoke(Command);
    }
}