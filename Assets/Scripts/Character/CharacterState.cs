using UnityEngine;

namespace Character
{
    public class CharacterState : MonoBehaviour
    {
        [field: SerializeField] public CharacterMovementState CurrentCharacterMovementState { get; private set; }
    }
}