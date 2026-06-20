using TataSequencing;
using UnityEngine;

public class LaunchSequencerAtPosition : MonoBehaviour
{
    [SerializeField] private Sequencer sequencer;
    [SerializeField] private float heightToLaunch = -24f;

    private bool _alreadyLaunched = false;

    void Update()
    {
        if (_alreadyLaunched)
            return;

        if (transform.position.y >= heightToLaunch)
        {
            _alreadyLaunched = true;
            sequencer.Play();
        }
    }
}
