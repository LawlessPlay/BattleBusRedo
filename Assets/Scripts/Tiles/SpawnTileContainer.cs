using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace TacticsToolkit
{
    public class SpawnTileContainer : MonoBehaviour
    {
        public List<SpawnTile> spawnTiles;
        public Entity.TeamType TeamID = Entity.TeamType.Player;

        private void Start()
        {
            spawnTiles = GetComponentsInChildren<SpawnTile>().ToList();
        }
    }
}
