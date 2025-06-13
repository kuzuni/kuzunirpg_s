using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;

namespace RPG.Gacha.Base
{

    // 뽑기 시스템 베이스 클래스 (공통 기능 구현)
    public abstract class BaseGachaSystem<T, TRarity> : MonoBehaviour, IGachaSystem<T>
        where T : class, IGachaItem
        where TRarity : System.Enum
    {
        [Title("뽑기 설정")]
        [SerializeField]
        protected List<GachaRateConfig<TRarity>> gachaRates = new List<GachaRateConfig<TRarity>>();

        [Title("천장 시스템")]
        [SerializeField]
        protected int pityCount = 90;

        [SerializeField]
        [ProgressBar(0, "@pityCount", 0.8f, 0.8f, 0.3f)]
        protected int currentPityCount = 0;

        [SerializeField]
        protected TRarity guaranteedRarity;

        [Title("뽑기 기록")]
        [SerializeField]
        protected List<string> recentPullHistory = new List<string>();

        protected Dictionary<TRarity, List<T>> itemCache;

        // 추상 메서드 (하위 클래스에서 구현)
        protected abstract void InitializeCache();
        protected abstract void LoadItemData();
        protected abstract T GetRandomItem(TRarity rarity);
        protected abstract TRarity DetermineRarity();
        protected abstract TRarity GetRandomRarityWithMinimum(TRarity minimum);
        protected abstract bool IsRarityGreaterOrEqual(TRarity rarity1, TRarity rarity2);

        protected virtual void Start()
        {
            InitializeCache();
            LoadItemData();
        }

        // IGachaSystem 구현
        public virtual T PullSingle()
        {
            currentPityCount++;

            // 천장 도달
            if (currentPityCount >= pityCount)
            {
                currentPityCount = 0;
                return PerformGuaranteedPull(guaranteedRarity);
            }

            // 일반 뽑기
            TRarity selectedRarity = DetermineRarity();

            // 높은 등급이면 천장 리셋
            if (IsRarityGreaterOrEqual(selectedRarity, guaranteedRarity))
            {
                currentPityCount = 0;
            }

            T item = GetRandomItem(selectedRarity);
            if (item != null)
            {
                AddToHistory(item);
            }

            return item;
        }

        public virtual List<T> PullMultiple(int count)
        {
            List<T> results = new List<T>();
            for (int i = 0; i < count; i++)
            {
                var item = PullSingle();
                if (item != null)
                {
                    results.Add(item);
                }
            }
            return results;
        }

        public virtual List<T> Pull11()
        {
            List<T> results = new List<T>();

            // 10회 일반 뽑기
            for (int i = 0; i < 10; i++)
            {
                results.Add(PullSingle());
            }

            // 11번째는 보장 로직 적용 (하위 클래스에서 오버라이드 가능)
            results.Add(PullSingle());

            ShowMultipleResults(results, "11회 뽑기");
            return results;
        }

        public float GetPityProgress()
        {
            return pityCount > 0 ? (float)currentPityCount / pityCount : 0f;
        }

        public int GetCurrentPityCount()
        {
            return currentPityCount;
        }

        public void ResetPity()
        {
            currentPityCount = 0;
            Debug.Log("천장 카운트가 리셋되었습니다.");
        }

        public virtual Dictionary<int, float> GetRateTable()
        {
            var table = new Dictionary<int, float>();
            foreach (var rate in gachaRates)
            {
                table[System.Convert.ToInt32(rate.rarity)] = rate.probability;
            }
            return table;
        }

        // 보조 메서드
        protected virtual T PerformGuaranteedPull(TRarity minimumRarity)
        {
            var rarity = GetRandomRarityWithMinimum(minimumRarity);
            var item = GetRandomItem(rarity);

            if (item != null)
            {
                AddToHistory(item);
                Debug.Log($"<color=yellow>★보장★ {item.GetRarityName()} {item.ItemName} 획득!</color>");
            }

            return item;
        }

        protected virtual void AddToHistory(T item)
        {
            if (item == null) return;

            var color = ColorUtility.ToHtmlStringRGB(item.GetRarityColor());
            string log = $"<color=#{color}>{item.GetRarityName()} - {item.ItemName}</color>";

            recentPullHistory.Insert(0, log);
            if (recentPullHistory.Count > 50)
            {
                recentPullHistory.RemoveAt(recentPullHistory.Count - 1);
            }
        }

        protected virtual void ShowMultipleResults(List<T> results, string title)
        {
            Debug.Log($"========== {title} 결과 ==========");

            var sortedResults = results.OrderByDescending(e => e.GetRarityLevel());

            foreach (var item in sortedResults)
            {
                var color = ColorUtility.ToHtmlStringRGB(item.GetRarityColor());
                Debug.Log($"<color=#{color}>{item.GetRarityName()} - {item.ItemName}</color>");
            }

            Debug.Log("================================");
        }

        [Title("디버그")]
        [Button("뽑기 확률 정보", ButtonSizes.Medium)]
        protected virtual void ShowRateInfo()
        {
            Debug.Log("========== 뽑기 확률 정보 ==========");
            foreach (var rate in gachaRates)
            {
                Debug.Log($"{rate.rarity}: {rate.probability}%");
            }
            Debug.Log($"\n천장: {pityCount}회 ({guaranteedRarity} 이상 확정)");
            Debug.Log("====================================");
        }
    }

    // 뽑기 확률 설정
    [System.Serializable]
    public class GachaRateConfig<TRarity> where TRarity : System.Enum
    {
        public TRarity rarity;
        [Range(0f, 100f)]
        public float probability;
    }
}