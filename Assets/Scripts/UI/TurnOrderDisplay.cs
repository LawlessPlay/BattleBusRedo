using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace TacticsToolkit
{
    public class TurnOrderDisplay : MonoBehaviour
    {
        public GameObject activeCharacter;

        public GameObject portraitPrefab;
        // Start is called before the first frame update

        public AnimationCurve curve;
        private List<GameObject> characterTurnOrder;
        private List<GameObject> currentTurnOrder;

        private List<Vector3> previewPositions = new List<Vector3>();
        
        void Start()
        {
            characterTurnOrder = new List<GameObject>();
            currentTurnOrder = new List<GameObject>();

            for(int i = -1; i < 20; i++)
            {
                previewPositions.Add(new Vector3(transform.position.x, transform.position.y - (120 * i), transform.position.z));
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                //UpdateTurnOrderDisplay(currentTurnOrder);
            }
        }

        public void SetTurnOrderDisplay(List<GameObject> characters)
        {
            if (characters.Count > 0 && characterTurnOrder != null)
            {
                if (characterTurnOrder.Count <= 0)
                {
                    CreatePreivewList(characters);
                }
                else
                {
                    UpdateTurnOrderDisplay(characters);
                }
            }
        }

        public void SolidifyTurnOrder()
        {
            for (int i = 0; i < previewPositions.Count; i++)
            {
                if(characterTurnOrder.Count > i)
                {
                    characterTurnOrder[i].transform.position = previewPositions[i];
                }
            }

            currentTurnOrder = characterTurnOrder;
        }

        public void SetPreviewList(List<GameObject> characters)
        {
            StartCoroutine(PreviewTurnOrderDisplay(characters));
        }


        //preview chracters new turn order
        public IEnumerator PreviewTurnOrderDisplay(List<GameObject> characters2)
        {
            //find all indexes where the image name = X
            var indexes = characterTurnOrder.Select((obj, index) => new { Object = obj, Index = index })
             .Where(x => x.Object.GetComponent<Image>().sprite.name == activeCharacter.GetComponentInChildren<SpriteRenderer>().sprite.name)
             .Select(x => x.Index).ToList();

            var characters = characterTurnOrder.Where(x => x.GetComponent<Image>().sprite.name == activeCharacter.GetComponentInChildren<SpriteRenderer>().sprite.name).Select(x => x.gameObject).ToList();

            indexes.RemoveAt(0);
            characters.RemoveAt(0);

            yield return StartCoroutine(RemovePreviewImage(characters, indexes));

            yield return new WaitForEndOfFrame();

            var newIndexes = characters2.Select((obj, index) => new { Object = obj, Index = index })
             .Where(x => x.Object.GetComponentInChildren<SpriteRenderer>().sprite.name == activeCharacter.GetComponentInChildren<SpriteRenderer>().sprite.name)
             .Select(x => x.Index).ToList();

            newIndexes.RemoveAt(0);

            yield return StartCoroutine(MoveChildren(newIndexes));

            yield return new WaitForEndOfFrame();

            for (int i = 0; i < newIndexes.Count; i++)
            {
                newIndexes[i] += i;
            }

            InsertPreviewImage(activeCharacter.GetComponentInChildren<SpriteRenderer>().sprite, newIndexes);
        }

        public void UpdateTurnOrderDisplay(List<GameObject> characters)
        {
            StopAllCoroutines();
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            characterTurnOrder = new List<GameObject>();
            CreatePreivewList(characters);
        }

        private void CreatePreivewList(List<GameObject> characters)
        {
            for (int i = 0; i < characters.Count; i++)
            {
                var spawnedObject = Instantiate(portraitPrefab, transform);

                if (characters[i].GetComponentInChildren<SpriteRenderer>())
                {
                    spawnedObject.GetComponent<Image>().sprite = characters[i].GetComponentInChildren<SpriteRenderer>().sprite;
                } else if (characters[i].GetComponent<Image>())
                {
                    spawnedObject.GetComponent<Image>().sprite = characters[i].GetComponent<Image>().sprite;
                }

                spawnedObject.transform.position = previewPositions[i];

                characterTurnOrder.Add(spawnedObject);
            }
            currentTurnOrder = characters;
        }

        private IEnumerator AnimateToPosition(List<GameObject> portraits, List<Vector3> targetPositions, float Duration)
        {
            float startTime = 0;
            if (portraits.Count > 0)
            {
                while (startTime < Duration)
                {
                    for (int i = 0; i < portraits.Count; i++)
                    {
                        var startPosition = portraits[i].transform.position;
                        float t = startTime / Duration;
                        float curveValue = curve.Evaluate(t);
                        portraits[i].transform.position = Vector3.Lerp(startPosition, targetPositions[i], curveValue);

                        startTime += Time.deltaTime;

                        yield return null;
                    }
                }

                // Ensure the object reaches the final position exactly
                for (int i = 0; i < portraits.Count; i++)
                {
                    portraits[i].transform.position = targetPositions[i];
                }
            }

            yield return null;
        }


        //move all children over one unit at index
        public IEnumerator MoveChildren(List<int> indexes)
        {
            //get all children
            var children = GetComponentsInChildren<Image>().Select(x => x.gameObject).ToList();
            var positions = new List<Vector3>();
            int step = 0;

            for (int i = 0; i < children.Count;i++)
            {
                if( step < indexes.Count && i >= indexes[step]){
                    step++;
                }

                positions.Add(previewPositions[i + step]);
            }

            yield return StartCoroutine(AnimateToPosition(children, positions, 0.05f));
            yield return null;
        }

        //move all children over one unit at index
        public IEnumerator ResetChildren()
        {
            //get all children
            var children = GetComponentsInChildren<Image>().Select(x => x.gameObject).ToList();
            yield return StartCoroutine(AnimateToPosition(children, previewPositions, 0.05f));
            yield return null;
        }

        public void InsertPreviewImage(Sprite preview, List<int> indexes)
        {
            var listPositions = new List<Vector3>();
            var newPreviewItems = new List<GameObject>();
            foreach (var item in indexes)
            {
                var spawnedObject = Instantiate(portraitPrefab, transform);
                spawnedObject.GetComponent<Image>().sprite = preview;
                var listPosition = previewPositions[item];
                spawnedObject.transform.position = new Vector3(listPosition.x + 100, listPosition.y, listPosition.z);
                newPreviewItems.Add(spawnedObject);
                listPositions.Add(new Vector3(listPosition.x - 20, listPosition.y, listPosition.z));

                if(item < characterTurnOrder.Count)
                    characterTurnOrder.Insert(item, spawnedObject);
                else
                    characterTurnOrder.Insert(characterTurnOrder.Count, spawnedObject);
            }

            StartCoroutine(AnimateToPosition(newPreviewItems, listPositions, 0.05f));
        }


        public IEnumerator RemovePreviewImage(List<GameObject> previews, List<int> indexes)
        {
            var listPositions = new List<Vector3>();

            foreach (var item in indexes)
            {
                if (item != 0)
                {
                    var position = previewPositions[item];
                    listPositions.Add(new Vector3(position.x + 120, position.y, position.z));
                }
            }

            yield return StartCoroutine(AnimateToPosition(previews, listPositions, 0.05f));

            for (int i = indexes.Count - 1; i >= 0; i--)
            {
                characterTurnOrder.RemoveAt(indexes[i]);
            }

            foreach (var item in previews)
            {
                Destroy(item);
            }

        }

        public void SetActiveCharacter(GameObject activeCharacter)
        {
            this.activeCharacter = activeCharacter;
        }
    }
}
