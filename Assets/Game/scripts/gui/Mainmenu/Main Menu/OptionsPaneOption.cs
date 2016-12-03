﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Raider.Game.GUI.Components
{

    public class OptionsPaneOption : EventTrigger
    {

        public class OptionsPaneContents
        {
            public readonly string name;
            public readonly Sprite image;
            public readonly string description;

            public OptionsPaneContents(string _name, string _description)
            {
                name = _name;
                description = _description;
            }

            public OptionsPaneContents(string _name, string _description, Sprite _sprite)
            {
                name = _name;
                description = _description;
                image = _sprite;
            }

        }

        public OptionsPaneContents optionData;

        void Start()
        {
            if (optionData == null)
            {
                Debug.Log("OptionPaneOption instanced with no OptionPaneContents!");
                Destroy(this.transform);
            }

            transform.Find("Label").GetComponent<Text>().text = optionData.name;
            this.name = optionData.name;

            GetComponent<Button>().onClick.AddListener(new UnityEngine.Events.UnityAction(OnClick));
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            OptionsPaneHandler.instance.OptionHover(optionData);
            base.OnPointerEnter(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            OptionsPaneHandler.instance.OptionStopHover();
            base.OnPointerExit(eventData);
        }

        public void OnClick()
        {
            OptionsPaneHandler.instance.OptionClicked(this.name);
        }
    }
}