using UnityEngine;

public class EndAnimStarter : MonoBehaviour
{
    public VictoryScreenAnimation victoryAnimation; 
    public LossWedget lossWidget;                   

    void Start()
    {
        int stars = GameManager.Instance.stars;

        if (stars >= 2)
        {
            Debug.Log("Playing Victory Animation!");
            victoryAnimation.PlayAnimation();
        }
        else
        {
            Debug.Log("Playing Loss Animation!");
            lossWidget.PlayAnimation();
        }
    }
}
