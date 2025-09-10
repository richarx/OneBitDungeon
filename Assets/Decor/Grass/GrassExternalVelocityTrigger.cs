using System;
using System.Collections;
using Player.Scripts;
using UnityEngine;

namespace Decor.Grass
{
    public class GrassExternalVelocityTrigger : MonoBehaviour
    {
        private GrassVelocityController grassVelocityController;

        private PlayerStateMachine player;
        
        private Material material;

        private bool easeInCoroutineRunning;
        private bool easeOutCoroutineRunning;
        
        private int externalInfluenceProperty = Shader.PropertyToID("_ExternalInfluence");

        private float startingXVelocity;
        private float velocityLastFrame;

        private void Start()
        {
            player = PlayerStateMachine.instance;
            grassVelocityController = GetComponentInParent<GrassVelocityController>();
            material = GetComponent<SpriteRenderer>().material;
            startingXVelocity = material.GetFloat(externalInfluenceProperty);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!easeInCoroutineRunning && IsPlayer(other) && IsPlayerVelocityAboveThreshold())
                StartCoroutine(EaseIn(ComputePlayerInfluence()));
        }

        private void OnTriggerStay(Collider other)
        {
            if (IsPlayer(other))
            {
                bool wasAbove = WasPlayerVelocityAboveThresholdLastFrame();
                bool isAbove = IsPlayerVelocityAboveThreshold();

                if (wasAbove && !isAbove)
                {
                    StartCoroutine(EaseOut());
                }
                else if (!wasAbove && isAbove)
                {
                    StartCoroutine(EaseIn(ComputePlayerInfluence()));
                }
                else if (!easeInCoroutineRunning && !easeOutCoroutineRunning && isAbove)
                    grassVelocityController.ApplyInfluence(material, ComputePlayerInfluence());
                
                velocityLastFrame = player.moveVelocity.magnitude;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsPlayer(other))
                StartCoroutine(EaseOut());
        }

        private bool IsPlayer(Collider other)
        {
            return other.CompareTag("Player");
        }

        private bool IsPlayerVelocityAboveThreshold()
        {
            return Mathf.Abs(player.moveVelocity.magnitude) > Mathf.Abs(grassVelocityController.velocityThreshold);
        }
        
        private bool WasPlayerVelocityAboveThresholdLastFrame()
        {
            return Mathf.Abs(velocityLastFrame) > Mathf.Abs(grassVelocityController.velocityThreshold);
        }

        private float ComputePlayerInfluence()
        {
            return player.moveVelocity.magnitude * grassVelocityController.externalInfluenceStrength;
        }

        private IEnumerator EaseIn(float xVelocity)
        {
            easeInCoroutineRunning = true;

            float elapsedTime = 0.0f;
            while (elapsedTime < grassVelocityController.easeInTime)
            {
                elapsedTime += Time.deltaTime;

                float lerpedAmount = Mathf.Lerp(startingXVelocity, xVelocity, (elapsedTime / grassVelocityController.easeInTime));
                grassVelocityController.ApplyInfluence(material, lerpedAmount);

                yield return null;
            }
            
            easeInCoroutineRunning = false;
        }
        
        private IEnumerator EaseOut()
        {
            easeOutCoroutineRunning = true;

            float currentXInfluence = material.GetFloat(externalInfluenceProperty);
            
            float elapsedTime = 0.0f;
            while (elapsedTime < grassVelocityController.easeOutTime)
            {
                elapsedTime += Time.deltaTime;

                float lerpedAmount = Mathf.Lerp(currentXInfluence, startingXVelocity, (elapsedTime / grassVelocityController.easeOutTime));
                grassVelocityController.ApplyInfluence(material, lerpedAmount);

                yield return null;
            }
            
            easeOutCoroutineRunning = false;
        }
    }
}
