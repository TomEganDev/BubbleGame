using System;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    public enum State : byte
    {
        Floor,
        Wall,
        Roof,
        Floating
    }
    
    [field: SerializeField]
    public State CurrentState { get; private set; }

    public ParticleSystem BubblePopVFX_Prefab;

    private int _spawnTick;
    public int SpawnTick => _spawnTick;
    private static Bubble[] _bubbleSlots = new Bubble[3];

    public void OnSpawn(State state)
    {
        // set state
        CurrentState = state;
        
        // bubble active queue logic
        _spawnTick = Time.frameCount;
        var oldestTick = int.MaxValue;
        var oldestIndex = -1;
        for (int i = 0; i < _bubbleSlots.Length; i++)
        {
            if (_bubbleSlots[i] == null)
            {
                Debug.Log($"Bubble claim FREE slot {i}");
                _bubbleSlots[i] = this;
                return;
            }
            if(_bubbleSlots[i].SpawnTick <= oldestTick)
            {
                oldestTick = _bubbleSlots[i].SpawnTick;
                oldestIndex = i;
            }
        }
        
        var destroyingBubble = _bubbleSlots[oldestIndex];
        Debug.Log($"Bubble claim TAKEN slot {oldestIndex}");
        _bubbleSlots[oldestIndex] = this;
        var vfxPos = destroyingBubble.transform.position;
        Instantiate(BubblePopVFX_Prefab, vfxPos, Quaternion.identity);
        Destroy(destroyingBubble.gameObject);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        var isPlayer = Player.Instance.IsPlayer(other.gameObject);
        if (!isPlayer)
        {
            return;
        }
        
        // TODO - fix hax properly for rocket jump spawning in floor thinking its roof
        if (CurrentState == State.Roof && Time.frameCount == _spawnTick)
        {
            Debug.LogWarning($"[{Time.frameCount}] HACKY_ROCKET_DETECTED Forcing Floor state");
            CurrentState = State.Floor;
        }
        
        Player.Instance.BCC.OnBubblePop(this);
        Instantiate(BubblePopVFX_Prefab, transform.position, Quaternion.identity);
        MainCamera.Instance.ScreenShake();
        Destroy(gameObject);
    }
}
