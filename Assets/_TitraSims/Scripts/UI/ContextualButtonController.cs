using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public class ContextualButtonController : MonoBehaviour
    {
        public static ContextualButtonController Instance { get; private set; }
        [SerializeField] private ContextualButton contextualActionButtonPrefab;
        
        private List<ContextualButton> contextualButtons = new();
        
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        public void GenerateContextualButton(int size)
        {
            foreach (ContextualButton contextualButton in contextualButtons)
            {
                Destroy(contextualButton);
            }
            contextualButtons.Clear();
            for (int i = 0; i < size; i++)
            {
                contextualButtons.Add(Instantiate(contextualActionButtonPrefab, transform));
            }
        }
        
        public void DestroyButtons()
        {
            for (int i = contextualButtons.Count - 1; i >= 0; i--)
            {
                if (!contextualButtons[i])
                {
                    continue;
                }
                Destroy(contextualButtons[i].gameObject);
            }
            contextualButtons.Clear();
        }
        
        public void RegisterAction(int targetButtonIndex, Action action)
        {
            // ToDo : Check if index valid
            contextualButtons[targetButtonIndex].RegisterAction(action);
        }
        
        public void RegisterTextToButton(int targetButtonIndex, string text)
        {
            // ToDo : Check if index valid
            contextualButtons[targetButtonIndex].RegisterText(text);
        }

        public List<ContextualButton> GetContextualButtons()
        {
            return contextualButtons;
        }
        
        public ContextualButton GetContextualButton(int targetButtonIndex)
        {
            return contextualButtons.Count > targetButtonIndex ? contextualButtons[targetButtonIndex] : null;
        }
    }
}