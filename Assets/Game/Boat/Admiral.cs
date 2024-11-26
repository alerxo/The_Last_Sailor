using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class Admiral : MonoBehaviour
{
    public string Name { get; private set; } = "Admiral Johnson";
    public Boat Owner { get; private set; }

    public Admiral Enemy { get; protected set; }

    public readonly List<Boat> Fleet = new();
    public readonly List<AIBoatController> Subordinates = new();

    private const float SET_SUBORDINATE_DESTINATION_WAIT_DURATION = 0.2f;
    private bool canSetFleetDestinations = true;

    public float LongestSubordinateDistance { get; private set; }

    public Formation Formation { get; private set; }

    public virtual void Awake()
    {
        SetFormation(Formation.Spearhead);
    }

    private void Update()
    {
        if (canSetFleetDestinations)
        {
            canSetFleetDestinations = false;
            StartCoroutine(SetFleetDestinations());
        }
    }

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
            Fleet.Remove(_boat);
        }
    }

    public void SetEnemy(Admiral _enemy)
    {
        Enemy = _enemy;
    }

    protected void SetFormation(Formation _formation)
    {
        Formation = _formation;
    }

    private IEnumerator SetFleetDestinations()
    {
        Vector3[] positions = GetFleetPositions();

        float distance = 0;

        for (int i = 0; i < Subordinates.Count; i++)
        {
            if (i >= Subordinates.Count || i >= positions.Length)
            {
                break;
            }

            Subordinates[i].SetDestination(positions[i]);
            distance = Mathf.Max(distance, Subordinates[i].Distance);

            yield return new WaitForSeconds(SET_SUBORDINATE_DESTINATION_WAIT_DURATION);
        }

        LongestSubordinateDistance = distance;
        canSetFleetDestinations = true;
    }

    private Vector3[] GetFleetPositions()
    {
        switch (Formation)
        {
            case Formation.Line:
                return Formations.GetLine(transform.position, transform.forward, Subordinates.Count);

            case Formation.Spearhead:
                return Formations.GetSpearhead(transform, Subordinates.Count);

            case Formation.Ring:
                return Formations.GetRing(transform, Subordinates.Count);

            default:
                Debug.LogError($"Defaulted for case {Formation}");
                return null;
        }
    }
}