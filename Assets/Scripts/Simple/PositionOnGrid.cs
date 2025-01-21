using UnityEngine;

namespace TacticsToolkit
{
    //On start up, link a character to the closest tile.
    public class PositionOnGrid : MonoBehaviour
    {
        void Start()
        {
            PositionEntityOnGrid();
        }
        
        // Start is called before the first frame update
        public void PositionEntityOnGrid()
        {
            var closestTile = MapManager.Instance.GetOverlayByTransform(transform.position);
            
            if (closestTile != null)
            {
                transform.position = closestTile.transform.position;

                //this should be more generic
                Entity entity = GetComponent<Entity>();
                entity.UpdateInitialSpritePosition(transform.position);

                if (entity != null)
                    entity.LinkCharacterToTile(closestTile);
            }
        }
    }
}
