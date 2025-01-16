using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TacticsToolkit.ArrowTranslator;

namespace TacticsToolkit
{
    //The tile object.
    public class OverlayTile : MonoBehaviour
    {
        public int G;
        public int H;
        public int F { get { return (G + H); } }
        public int accumulativeMovementCost;

        public bool isBlocked;
        public OverlayTile previous;
        public Vector3Int gridLocation;
        public Vector2Int grid2DLocation { get { return new Vector2Int(gridLocation.x, gridLocation.y); } }
        public List<Sprite> arrows;
        public TileData tileData;
        public Entity activeCharacter;

        public bool isFocused;

        public SpriteRenderer childSpriteRenderer;
        public SpriteRenderer pathSpriteRenderer;

        [HideInInspector]
        public int remainingMovement;

        public enum TileColors
        {
            MovementColor,
            AttackRangeColor,
            AttackColor
        }

        private void Start()
        {
            accumulativeMovementCost = GetMoveCost();
        }

        //Color a tile
        public void ShowTile(Color color)
        {
            childSpriteRenderer.color = color;
            //StartCoroutine(LerpColor(color));
        }
        
        private IEnumerator LerpColor(Color targetColor)
        {
            Color initialColor = childSpriteRenderer.color;
            float elapsedTime = 0f;

            while (elapsedTime < 0.2f)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / 0.1f;
                childSpriteRenderer.color = Color.Lerp(initialColor, targetColor, t);
                yield return null;
            }

            // Ensure the final color is set
            childSpriteRenderer.color = targetColor;
        }
        
        //Remove the color from a tile.
        public void HideTile()
        {
            childSpriteRenderer.color = new Color(1, 1, 1, 0);
            SetArrowSprite(ArrowDirection.None);
        }

        //Sets the arrow sprite for displaying the path.
        public void SetArrowSprite(ArrowDirection d)
        {
            var arrow = pathSpriteRenderer;
            if (d == ArrowDirection.None)
            {
                arrow.color = new Color(1, 1, 1, 0);
            }
            else
            {
                arrow.color = new Color(1, 1, 1, 1);
                arrow.sprite = arrows[(int)d];
            }
        }

        public int GetMoveCost() => tileData != null ? tileData.MoveCost : 1;
    }
}
