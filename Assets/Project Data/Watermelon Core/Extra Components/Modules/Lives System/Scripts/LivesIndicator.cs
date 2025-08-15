using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Watermelon
{
    public class LivesIndicator : MonoBehaviour
    {
        [Space]
        [SerializeField] TMP_Text livesCountText;
        [SerializeField] Image infinityImage;
        [SerializeField] TMP_Text durationText;

        [Space]
        [SerializeField] Button addButton;
        [SerializeField] AddLivesPanel addLivesPanel;

        private LivesData Data { get; set; }

        private bool isInitialised;

        public void Init(LivesData data)
        {
            if (isInitialised) return;
            
            Data = data;

            if(addLivesPanel != null)
            {
                addButton.gameObject.SetActive(true);
                addButton.onClick.AddListener(() => addLivesPanel.Show());
            } else
            {
                addButton.gameObject.SetActive(false);
            }

            isInitialised = true;
        }

        public void SetInfinite(bool isInfinite)
        {
            infinityImage.gameObject.SetActive(isInfinite);
            livesCountText.gameObject.SetActive(!isInfinite);
        }

        public void SetLivesCount(int count)
        {
            if (!isInitialised) return;

            livesCountText.text = count.ToString();

            addButton.gameObject.SetActive(count != Data.maxLivesCount);
            if(count == Data.maxLivesCount)
            {
                FullText();
            }
        }

        public void SetDuration(TimeSpan duration) 
        {
            if (!isInitialised) return;

            if (duration >= TimeSpan.FromHours(1))
            {
                durationText.text = string.Format(Data.longTimespanFormat, duration);
            }
            else
            {
                durationText.text = string.Format(Data.timespanFormat, duration);
            }
        }

        public void FullText()
        {
            if (!isInitialised) return;

            durationText.text = Data.fullText;
        }

        private void OnEnable()
        {
            LivesManager.AddIndicator(this);
        }

        private void OnDisable()
        {
            LivesManager.RemoveIndicator(this);
        }
    }
}