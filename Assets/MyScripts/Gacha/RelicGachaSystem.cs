using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using RPG.Items.Relic;
using RPG.Gacha.Base;
using RPG.Inventory;


namespace RPG.Gacha
{

    public class RelicGachaSystem : BaseGachaSystem<RelicData, RelicRarity>
    {
        [Title("유물 뽑기 특수 설정")]
        [SerializeField]
        private bool enable10PullGuarantee = true;

        [Title("유물 인벤토리 연동")]
        [SerializeField]
        private RelicInventorySystem relicInventory;

        [SerializeField]
        private bool autoAddToInventory = true;

        protected override void Start()
        {
            // 기본 초기화
            base.Start();

            // 천장 설정 (유물은 더 낮게)
            pityCount = 50;
            guaranteedRarity = RelicRarity.Epic;

            // 확률 설정
            if (gachaRates.Count == 0)
            {
                gachaRates = new List<GachaRateConfig<RelicRarity>>
            {
                new GachaRateConfig<RelicRarity> { rarity = RelicRarity.Common, probability = 60f },
                new GachaRateConfig<RelicRarity> { rarity = RelicRarity.Rare, probability = 30f },
                new GachaRateConfig<RelicRarity> { rarity = RelicRarity.Epic, probability = 9f },
                new GachaRateConfig<RelicRarity> { rarity = RelicRarity.Legendary, probability = 1f }
            };
            }

            // 인벤토리 찾기
            if (relicInventory == null)
            {
                relicInventory = GetComponent<RelicInventorySystem>();
                if (relicInventory == null)
                {
                    relicInventory = FindObjectOfType<RelicInventorySystem>();
                }
            }
        }

        protected override void InitializeCache()
        {
            itemCache = new Dictionary<RelicRarity, List<RelicData>>();
            foreach (RelicRarity rarity in System.Enum.GetValues(typeof(RelicRarity)))
            {
                itemCache[rarity] = new List<RelicData>();
            }
        }

        protected override void LoadItemData()
        {
            var allRelics = Resources.LoadAll<RelicData>("Relics");

            if (allRelics == null || allRelics.Length == 0)
            {
                Debug.LogError("Resources/Relics 폴더에서 유물을 찾을 수 없습니다!");
                return;
            }

            foreach (var relic in allRelics)
            {
                if (relic != null && itemCache.ContainsKey(relic.rarity))
                {
                    itemCache[relic.rarity].Add(relic);
                }
            }

            // 로드 결과 출력
            Debug.Log("========== 유물 로드 완료 ==========");
            foreach (var kvp in itemCache)
            {
                Debug.Log($"{kvp.Key}: {kvp.Value.Count}개");
            }
            Debug.Log("====================================");
        }

        protected override RelicData GetRandomItem(RelicRarity rarity)
        {
            if (!itemCache.ContainsKey(rarity) || itemCache[rarity].Count == 0)
            {
                Debug.LogError($"{rarity} 등급의 유물이 없습니다!");

                // 대체 등급 찾기
                foreach (RelicRarity fallbackRarity in System.Enum.GetValues(typeof(RelicRarity)))
                {
                    if (itemCache.ContainsKey(fallbackRarity) && itemCache[fallbackRarity].Count > 0)
                    {
                        Debug.LogWarning($"{rarity} 대신 {fallbackRarity} 등급 유물로 대체합니다.");
                        rarity = fallbackRarity;
                        break;
                    }
                }
            }

            var relicList = itemCache[rarity];
            return relicList[Random.Range(0, relicList.Count)];
        }

        protected override RelicRarity DetermineRarity()
        {
            float random = Random.Range(0f, 100f);
            float cumulative = 0f;

            foreach (var rate in gachaRates)
            {
                cumulative += rate.probability;
                if (random <= cumulative)
                {
                    return rate.rarity;
                }
            }

            return RelicRarity.Common;
        }

        protected override RelicRarity GetRandomRarityWithMinimum(RelicRarity minimum)
        {
            var validRates = gachaRates.Where(r => IsRarityGreaterOrEqual(r.rarity, minimum)).ToList();
            float totalProb = validRates.Sum(r => r.probability);
            float random = Random.Range(0f, totalProb);
            float cumulative = 0f;

            foreach (var rate in validRates)
            {
                cumulative += rate.probability;
                if (random <= cumulative)
                {
                    return rate.rarity;
                }
            }

            return minimum;
        }

        protected override bool IsRarityGreaterOrEqual(RelicRarity rarity1, RelicRarity rarity2)
        {
            return rarity1 >= rarity2;
        }

        // 오버라이드된 메서드들
        public override RelicData PullSingle()
        {
            var result = base.PullSingle();

            if (result != null)
            {
                ShowSingleResult(result);

                // 인벤토리에 자동 추가
                if (autoAddToInventory && relicInventory != null)
                {
                    var instance = new RelicInstance(result);
                    relicInventory.AddItem(instance);
                }
            }

            return result;
        }

        // 10회 뽑기 (희귀 이상 1개 보장)
        public List<RelicData> Pull10()
        {
            List<RelicData> results = new List<RelicData>();
            bool hasRareOrBetter = false;

            // 9회 일반 뽑기
            for (int i = 0; i < 9; i++)
            {
                var relic = base.PullSingle();
                if (relic != null)
                {
                    results.Add(relic);
                    if (relic.rarity >= RelicRarity.Rare)
                    {
                        hasRareOrBetter = true;
                    }
                }
            }

            // 10번째 뽑기 (희귀 이상 보장)
            if (enable10PullGuarantee && !hasRareOrBetter)
            {
                var guaranteed = PerformGuaranteedPull(RelicRarity.Rare);
                results.Add(guaranteed);
            }
            else
            {
                results.Add(base.PullSingle());
            }

            ShowMultipleResults(results, "유물 10회 뽑기");

            // 인벤토리에 추가
            if (autoAddToInventory && relicInventory != null)
            {
                foreach (var relic in results)
                {
                    if (relic != null)
                    {
                        var instance = new RelicInstance(relic);
                        relicInventory.AddItem(instance);
                    }
                }
            }

            return results;
        }

        private void ShowSingleResult(RelicData relic)
        {
            var color = ColorUtility.ToHtmlStringRGB(relic.GetRarityColor());
            Debug.Log($"<color=#{color}>★ 유물 획득: [{relic.GetRarityName()}] {relic.relicName} ★</color>");
        }

        protected override void ShowMultipleResults(List<RelicData> results, string title)
        {
            Debug.Log($"========== {title} 결과 ==========");

            var sortedResults = results.OrderByDescending(r => r.GetRarityLevel());

            foreach (var relic in sortedResults)
            {
                var color = ColorUtility.ToHtmlStringRGB(relic.GetRarityColor());
                Debug.Log($"<color=#{color}>[{relic.GetRarityName()}] {relic.relicName}</color>");
            }

            // 통계
            var stats = results.GroupBy(r => r.rarity)
                              .OrderByDescending(g => g.Key)
                              .Select(g => new { Rarity = g.Key, Count = g.Count() });

            Debug.Log("\n--- 획득 통계 ---");
            foreach (var stat in stats)
            {
                var color = ColorUtility.ToHtmlStringRGB(RelicRarityColors.GetRarityColor(stat.Rarity));
                Debug.Log($"<color=#{color}>{RelicRarityColors.GetRarityName(stat.Rarity)}: {stat.Count}개</color>");
            }

            Debug.Log("================================");
        }

        [Title("뽑기 기능")]
        [ButtonGroup("Gacha")]
        [Button("유물 1회 뽑기", ButtonSizes.Large), GUIColor(0.3f, 0.8f, 0.3f)]
        private void Pull1Button()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Play Mode에서만 뽑기가 가능합니다!");
                return;
            }
            PullSingle();
        }

        [ButtonGroup("Gacha")]
        [Button("유물 10회 뽑기", ButtonSizes.Large), GUIColor(0.5f, 0.5f, 1f)]
        private void Pull10Button()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Play Mode에서만 뽑기가 가능합니다!");
                return;
            }
            Pull10();
        }

        [Title("디버그")]
        [Button("유물 데이터 재로드", ButtonSizes.Large)]
        [GUIColor(0.3f, 0.8f, 0.8f)]
        private void ReloadRelicData()
        {
            InitializeCache();
            LoadItemData();
            Debug.Log("유물 데이터를 다시 로드했습니다.");
        }

        [ShowInInspector, ReadOnly]
        [DictionaryDrawerSettings(KeyLabel = "타입", ValueLabel = "개수")]
        private Dictionary<RelicType, int> RelicTypeCount
        {
            get
            {
                var count = new Dictionary<RelicType, int>();
                foreach (RelicType type in System.Enum.GetValues(typeof(RelicType)))
                {
                    count[type] = 0;
                }

                if (itemCache != null)
                {
                    foreach (var kvp in itemCache)
                    {
                        foreach (var relic in kvp.Value)
                        {
                            count[relic.relicType]++;
                        }
                    }
                }

                return count;
            }
        }
    }
}