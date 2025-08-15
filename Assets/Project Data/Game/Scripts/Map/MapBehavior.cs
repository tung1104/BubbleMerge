using System.Collections;
using UnityEngine;
using Watermelon.BubbleMerge;

namespace Watermelon.Map
{
    public class MapBehavior : MonoBehaviour
    {
        private static MapBehavior instance;

        [SerializeField] MapData data;
        [SerializeField] Transform lightTransform;
        [SerializeField] Vector3 mapLightRotation;

        public int SelectedChunkId { get; set; }

        public MapChankBehavior SelectedChunk { get; private set; }
        public MapChankBehavior PrevChunk { get; private set; }
        public MapChankBehavior NextChunk { get; private set; }

        public float Width { get; private set; }
        public float Height { get; private set; }

        private Vector3 saveLightRotation;

        private float pressY;
        private float releaseY;
        private bool isMouseDown = false;
        private float currentPos;
        private float prevY;
        private float moveDelta;

        TweenCase rubberCase;

        private void Awake()
        {
            instance = this;

            Height = Camera.main.orthographicSize * 2;
            Width = Height * Camera.main.aspect;

            saveLightRotation = lightTransform.eulerAngles;
        }

        public void Init()
        {
            enabled = true;
            isMouseDown = false;

            SelectedChunkId = -1;

            int counter = 0;
            while (counter <= LevelController.MaxLevelReached)
            {
                SelectedChunkId++;

                var chunk = data.chunks[SelectedChunkId % data.chunks.Count].GetComponent<MapChankBehavior>();
                counter += chunk.LevelsCount;
            }

            SelectedChunk = Instantiate(data.chunks[SelectedChunkId % data.chunks.Count]).GetComponent<MapChankBehavior>();

            SelectedChunk.SetMap(this);
            SelectedChunk.Init(counter - SelectedChunk.LevelsCount);

            var selectedPos = -SelectedChunk.CurrentLevelPosition / Height + data.currentLevelPos;
            if (SelectedChunkId == 0 && selectedPos > 0) selectedPos = 0;

            SelectedChunk.SetPosition(0);

            if(SelectedChunkId != 0)
            {
                PrevChunk = Instantiate(data.chunks[(SelectedChunkId - 1) % data.chunks.Count]).GetComponent<MapChankBehavior>();

                PrevChunk.SetMap(this);
                PrevChunk.Init(counter - SelectedChunk.LevelsCount - PrevChunk.LevelsCount);

                PrevChunk.SetPosition(-PrevChunk.Height / Height);
            }

            NextChunk = Instantiate(data.chunks[(SelectedChunkId + 1) % data.chunks.Count]).GetComponent<MapChankBehavior>();

            NextChunk.SetMap(this);
            NextChunk.Init(counter);

            NextChunk.SetPosition(NextChunk.Height / Height);

            currentPos = 0;
            moveDelta = selectedPos;
            Move();

            lightTransform.eulerAngles = mapLightRotation;
        }

        public void Disable()
        {
            enabled = false;
            SelectedChunk.gameObject.SetActive(false);
            NextChunk.gameObject.SetActive(false);
            if(PrevChunk != null) PrevChunk.gameObject.SetActive(false);

            lightTransform.eulerAngles = saveLightRotation;

            StartCoroutine(DisableCoroutine());
        }

        private IEnumerator DisableCoroutine()
        {
            yield return new WaitForSeconds(0.5f);
            Destroy(NextChunk.gameObject);

            yield return new WaitForSeconds(0.5f);
            Destroy(SelectedChunk.gameObject);

            if(PrevChunk != null)
            {
                yield return new WaitForSeconds(0.5f);
                Destroy(PrevChunk.gameObject);
            }
        }

        #region Movement



        public static void EnableScroll()
        {
            instance.enabled = true;

            if(instance.SelectedChunk != null)
            {
                instance.isMouseDown = false;
            }
            
        }

        public static void DisableScroll()
        {
            instance.enabled = false;
        }

        public static void MoveToCenter()
        {
            instance.CenterMap();
        }

        private void CenterMap()
        {
            var selectedPos = -SelectedChunk.CurrentLevelPosition / Height + data.currentLevelPos;
            if (SelectedChunkId == 0 && selectedPos > 0) selectedPos = 0;

            SelectedChunk.SetPosition(0);
            currentPos = 0;

            moveDelta = selectedPos;

            Move();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                pressY = Input.mousePosition.y / Camera.main.pixelHeight;
                prevY = pressY;
                currentPos = SelectedChunk.Position;

                isMouseDown = true;

                rubberCase.KillActive();
            }
            else if(Input.GetMouseButtonUp(0))
            {
                isMouseDown = false;

                if (PrevChunk == null && SelectedChunk.Position > 0)
                {
                    BottomRubber();
                } else
                {
                    releaseY = Input.mousePosition.y / Camera.main.pixelHeight;
                    var dif = releaseY - prevY;

                    if(Mathf.Abs(dif) > 0.001f)
                    {
                        ContinuousScroll(dif);
                    }
                }
            } 
            else if (isMouseDown)
            {
                var mousePosY = Input.mousePosition.y / Camera.main.pixelHeight;
                prevY = mousePosY;

                moveDelta = mousePosY - pressY;

                Move();
            }
        }

        private void ContinuousScroll(float value)
        {
            rubberCase = Tween.DoFloat(value, 0, Mathf.Abs(value / 0.1f), (value) => {
                moveDelta += value;
                Move();
            }).SetEasing(Ease.Type.SineOut).OnComplete(() => {
                if (PrevChunk == null && SelectedChunk.Position > 0)
                {
                    rubberCase = Tween.DoFloat(SelectedChunk.Position, 0, 0.3f, (value) => {
                        SelectedChunk.SetPosition(value);
                        NextChunk.SetPosition(SelectedChunk.Position + NextChunk.Height / Height);
                    }).SetEasing(Ease.Type.QuadOut);
                }
            });
        }

        private void BottomRubber()
        {
            rubberCase = Tween.DoFloat(SelectedChunk.Position, 0, 0.3f, (value) => {
                SelectedChunk.SetPosition(value);
                NextChunk.SetPosition(SelectedChunk.Position + NextChunk.Height / Height);
            }).SetEasing(Ease.Type.QuadOut);
        }

        private void Move()
        {
            var pos = currentPos + moveDelta;

            if (pos > 0 && PrevChunk == null)
            {
                pos = Mathf.Clamp(Ease.Interpolate(Mathf.Clamp01(pos), Ease.Type.QuadOut), 0, 1f) * 0.2f;
            }

            SelectedChunk.SetPosition(pos);
            NextChunk.SetPosition(SelectedChunk.Position + NextChunk.Height / Height);
            if (PrevChunk != null)
            {
                PrevChunk.SetPosition(SelectedChunk.Position - PrevChunk.Height / Height);
            }

            if (moveDelta < 0)
            {
                if (NextChunk.Position < 0.6f)
                {
                    SpawnAbove();
                }
            }
            else
            {
                if (PrevChunk != null && SelectedChunk.Position > 0.5f)
                {
                    SpawnBelow();
                }
            }
        }

        private void SpawnAbove()
        {
            if (PrevChunk != null)
            {
                Destroy(PrevChunk.gameObject);
            }

            PrevChunk = SelectedChunk;
            SelectedChunk = NextChunk;
            SelectedChunkId++;

            NextChunk = Instantiate(data.chunks[(SelectedChunkId + 1) % data.chunks.Count]).GetComponent<MapChankBehavior>();

            NextChunk.SetMap(this);
            NextChunk.Init(SelectedChunk.StartLevelCount + SelectedChunk.LevelsCount);

            NextChunk.SetPosition(SelectedChunk.Position + NextChunk.Height / Height);

            pressY = Input.mousePosition.y / Camera.main.pixelHeight;
            currentPos = SelectedChunk.Position;

            moveDelta = 0;
        }

        private void SpawnBelow()
        {
            Destroy(NextChunk.gameObject);

            NextChunk = SelectedChunk;
            SelectedChunk = PrevChunk;
            SelectedChunkId--;

            if (SelectedChunk.StartLevelCount != 0)
            {
                PrevChunk = Instantiate(data.chunks[(SelectedChunkId - 1) % data.chunks.Count]).GetComponent<MapChankBehavior>();

                PrevChunk.SetMap(this);
                PrevChunk.Init(SelectedChunk.StartLevelCount - PrevChunk.LevelsCount);

                PrevChunk.SetPosition(SelectedChunk.Position - PrevChunk.Height / Height);
            }
            else
            {
                PrevChunk = null;
            }

            pressY = Input.mousePosition.y / Camera.main.pixelHeight;
            currentPos = SelectedChunk.Position;

            moveDelta = 0;
        }
    }

    #endregion
}