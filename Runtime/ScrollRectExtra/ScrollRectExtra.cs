using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace harpyfox.UI
{
    /// <summary>
    /// Extension of the ScrollRect class that adds some touch input-focused quality of life features.
    /// Detects which child object in the ScrollRect content is closest to the centre, and calls relevant
    /// methods on interfaces IScrollSelectHandler and IScrollDeselectHandler as required.
    /// Leverages the above to automatically snap to the selected item in the ScrollRect when under a
    /// certain velocity.
    /// <summary>
    public class ScrollRectExtra : ScrollRect, IBeginDragHandler, IEndDragHandler
    {
        [HideInInspector] public UnityEvent<PointerEventData> OnBeginDragEvent;
        [HideInInspector] public UnityEvent<PointerEventData> OnEndDragEvent;
        [HideInInspector] public UnityEvent<GameObject, GameObject> OnSelectedChange;

        [HideInInspector] public float minSpeedToSnap = 100f;
        [HideInInspector] public float snapSpeed = 0.1f;

        GameObject currentSelected = null;
        GameObject prevSelected = null;
        Coroutine snapRoutine;

        protected override void OnEnable() {
            if (Application.isPlaying) base.OnEnable();
            if (content.childCount > 0 && Application.isPlaying) EventSystem.current.SetSelectedGameObject(content.GetChild(0).gameObject);
        }

        public override void OnBeginDrag(PointerEventData data) {
            base.OnBeginDrag(data);

            if (Application.isPlaying) OnBeginDragEvent?.Invoke(data);
            
        }
        public override void OnEndDrag(PointerEventData data) {
            base.OnEndDrag(data);

            if (Application.isPlaying) {
                MagnetScroll();
                OnEndDragEvent?.Invoke(data);
            }
            
        }

        void MagnetScroll() {
            if (snapRoutine != null) {
                StopCoroutine(snapRoutine);
            }

            snapRoutine = StartCoroutine( Snap() );
            
        }

        IEnumerator Snap() {
            // Only snap when the ScrollRect is under a certain speed.
            yield return new WaitUntil( () => Mathf.Abs(velocity.x) <= minSpeedToSnap);

            StopMovement();

            // Move the content so that the closest GameObject (current selected) is in centre of the ScrollRect.
            float difference = (transform.position.x + GetComponent<RectTransform>().rect.center.x) - currentSelected.transform.position.x;
            Vector3 posDifference = new Vector3(difference, 0f, 0f);
            Vector3 currentPosition = content.position;
            Vector3 newPosition = content.position + posDifference;

            float timer = 0f;
            float duration = snapSpeed;
            while (timer < duration) {
                content.position = Vector3.Lerp(currentPosition, newPosition, timer / duration);
                timer += Time.deltaTime;
                yield return null;
            }
        }

        void Update() {
            if (gameObject.activeInHierarchy && Application.isPlaying) UpdateSelected();
        }

        void UpdateSelected() {
            float centre = GetComponent<RectTransform>().rect.center.x;
            RectTransform closestRect = null;
            prevSelected = currentSelected;

            // Check which GameObject is closest to the centre of the panel
            for (int i = 0; i < content.childCount; i++) {
                RectTransform rect = content.GetChild(i).GetComponent<RectTransform>();

                if (closestRect == null) {
                closestRect = rect;
                continue;
                }

                if ( Mathf.Abs(centre - rect.position.x) < Mathf.Abs(centre - closestRect.position.x) ) {
                // found a closer one.
                closestRect = rect;
                }
            }

            if (closestRect != null) {
                currentSelected = closestRect.gameObject;
            }
            
            // Update EventSystem and send messages to relevant interfaces.
            if (currentSelected != prevSelected) {
                currentSelected?.GetComponent<IScrollSelectHandler>()?.OnScrollSelect();
                prevSelected?.GetComponent<IScrollDeselectHandler>()?.OnScrollDeselect();

                EventSystem.current.SetSelectedGameObject(currentSelected);
                OnSelectedChange?.Invoke(prevSelected, currentSelected);
            }
            
        }
    }
}
