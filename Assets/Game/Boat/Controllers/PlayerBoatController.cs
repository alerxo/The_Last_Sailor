using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerBoatController : Boat
{
    protected override float MaxHealth => 100;

    protected override void OnHit()
    {
        base.OnHit();

        CameraManager.Instance.ShakeCamera(1f, 0.7f);
    }

    public override void Destroyed()
    {
        SceneManager.LoadScene("Game");
    }

    private void Update()
    {
        ChangeMovement(input.Player.Move.ReadValue<Vector2>());  
    }
}
