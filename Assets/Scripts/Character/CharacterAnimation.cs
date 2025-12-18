using System;
using UnityEngine;

namespace Character
{
    public class CharacterAnimation : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private CharacterInput characterInput;

        [SerializeField] private float locomotionBlendSpeed = 4f;

        private readonly int _inputXHash = Animator.StringToHash("InputX");
        private readonly int _inputYHash = Animator.StringToHash("InputY");
        
        private Vector2 _currentBlendInput = Vector2.zero;

        private void Update()
        {
            UpdateAnimationState();
        }

        private void UpdateAnimationState()
        {
            _currentBlendInput = Vector2.Lerp(_currentBlendInput, characterInput.MovementInput, locomotionBlendSpeed);
            animator.SetFloat(_inputXHash, _currentBlendInput.x);
            animator.SetFloat(_inputYHash, _currentBlendInput.y);
        }
    }
}