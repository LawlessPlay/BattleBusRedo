using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TacticsToolkit
{
    [ExecuteInEditMode()]
    public class Tooltip : MonoBehaviour
    {
        public TMP_Text title;
        public TMP_Text description;
        public Image image;

        public LayoutElement layoutElement;

        public float xPadding = 0;
        public float yPadding = 0;

        public int characterLimit;

        public bool isFixedPosition;
        public bool followMousePosition;

        public TooltipAlignment defaultAlignment;
        private TooltipAlignment alignment;

        public enum TooltipAlignment
        {
            TopLeft,
            Top,
            TopRight,
            Right,
            BottomRight,
            Bottom,
            BottomLeft,
            Left
        }

        private void Start()
        {
            alignment = defaultAlignment;
        }

        private void Update()
        {
            if (followMousePosition)
            {
                var mousePosition = Input.mousePosition;
                mousePosition = new Vector3(mousePosition.x + xPadding + (gameObject.GetComponent<RectTransform>().rect.width/2), mousePosition.y + yPadding + (gameObject.GetComponent<RectTransform>().rect.height/2),  mousePosition.z );
                
                transform.position = mousePosition;
                
            }
        }

        [SerializeField]
        private RectTransform canvas;

        public void SetContent(Sprite image, string title, string description, Vector3 position, Vector2 dimensions)
        {
            int titleLenght = 0;
            int descriptionLenght = 0;
            if (this.title != null)
            {
                this.title.text = title;
                titleLenght = this.title.text.Length;
            }

            if (this.description != null)
            {
                this.description.text = description;
                descriptionLenght = this.description.text.Length;
            }

            if (this.image != null)
                this.image.sprite = image;

            layoutElement.enabled = titleLenght > characterLimit || descriptionLenght > characterLimit;

            if (!isFixedPosition)
            {
                StartCoroutine(MoveTooltip(position, dimensions));
            }
        }

        public RectTransform buttonRectTransform;
        public IEnumerator MoveTooltip(Vector3 position, Vector2 dimensions)
        {
            yield return new WaitForEndOfFrame();
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            Vector2 tooltipDimensions = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
            ChooseValidAlignment(position, dimensions, tooltipDimensions);
        }

        private void MoveToAlignment(Vector3 position, Vector2 dimensions, Vector2 tooltipDimensions)
        {
            Vector3 tooltipPosition = position; 
            
            switch (alignment)
            {
                case TooltipAlignment.TopLeft:
                    tooltipPosition += new Vector3(-dimensions.x / 2 - tooltipDimensions.x / 2, dimensions.y / 2 + tooltipDimensions.y / 2, 0);
                    break;
                case TooltipAlignment.Top:
                    tooltipPosition += new Vector3(0, dimensions.y / 2 + tooltipDimensions.y / 2, 0);
                    break;
                case TooltipAlignment.TopRight:
                    tooltipPosition += new Vector3(dimensions.x / 2 + tooltipDimensions.x / 2, dimensions.y / 2 + tooltipDimensions.y / 2, 0);
                    break;
                case TooltipAlignment.Right:
                    tooltipPosition += new Vector3(dimensions.x / 2 + tooltipDimensions.x / 2, 0, 0);
                    break;
                case TooltipAlignment.BottomRight:
                    tooltipPosition += new Vector3(dimensions.x / 2 + tooltipDimensions.x / 2, -dimensions.y / 2 - tooltipDimensions.y / 2, 0);
                    break;
                case TooltipAlignment.Bottom:
                    tooltipPosition += new Vector3(0, -dimensions.y / 2 - tooltipDimensions.y / 2, 0);
                    break;
                case TooltipAlignment.BottomLeft:
                    tooltipPosition += new Vector3(-dimensions.x / 2 - tooltipDimensions.x / 2, -dimensions.y / 2 - tooltipDimensions.y / 2, 0);
                    break;
                case TooltipAlignment.Left:
                    tooltipPosition += new Vector3(-dimensions.x / 2 - tooltipDimensions.x / 2, 0, 0);
                    break;
            }

            // Set the position of the tooltip
            transform.position = tooltipPosition;
        }

        void ChooseValidAlignment(Vector3 position, Vector2 dimensions, Vector2 tooltipDimensions)
        {
            MoveToAlignment(position, dimensions, tooltipDimensions);
            
            Vector3[] objectCorners = new Vector3[4];
            GetComponent<RectTransform>().GetWorldCorners(objectCorners);
            TooltipAlignment[] alignments = (TooltipAlignment[])Enum.GetValues(alignment.GetType());

            var fitsInPlace = CheckAlignment(position, dimensions, tooltipDimensions, defaultAlignment, objectCorners);

            if (!fitsInPlace)
            {
                foreach (var alignmentToCheck in alignments)
                {
                    if (CheckAlignment(position, dimensions, tooltipDimensions, alignmentToCheck, objectCorners)) break;
                }
            }
        }

        private bool CheckAlignment(Vector3 position, Vector2 dimensions, Vector2 tooltipDimensions,
            TooltipAlignment alignmentToCheck, Vector3[] objectCorners)
        {
            this.alignment = alignmentToCheck;
            MoveToAlignment(position, dimensions, tooltipDimensions);
            GetComponent<RectTransform>().GetWorldCorners(objectCorners);
            var isValid = objectCorners.All(x => x.x < Screen.width && x.x > 0 && x.y < Screen.height && x.y > 0);
                
            if (isValid)
                return true;
            return false;
        }


        public void ResetContent()
        {
            if (title != null)
                this.title.text = "";
            
            if (description != null)
                this.description.text = "";
        }

    }
}
