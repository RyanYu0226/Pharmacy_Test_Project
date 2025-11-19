using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using T4U_Pharmacy_Common_Model.Common;
using T4U_Pharmacy_Common_Model.ViewModel;

namespace T4U_Pharmacy_Service
{
    public class PharmacyMaskHandleService
    {
        private PharmacyService _pharmacyService;
        private MasksService _masksService;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="pharmacyService"></param>
        /// <param name="masksService"></param>
        public PharmacyMaskHandleService(PharmacyService pharmacyService, MasksService masksService)
        {
            _masksService = masksService;
            _pharmacyService = pharmacyService;
        }

        /// <summary>
        /// 透過關鍵字搜尋藥局和口罩
        /// </summary>
        /// <param name="keyword">關鍵字</param>
        /// <param name="pharmacyMaskSearchType">類型</param>
        /// <returns></returns>
        public async Task<PharmacyMaskSearchResultViewModel> SearchPharmacyMaskByKeyword(string keyword, PharmacyMaskSearchType pharmacyMaskSearchType)
        {
            PharmacyMaskSearchResultViewModel result = new PharmacyMaskSearchResultViewModel();
            result.Result = true;
            List<PharmacyMaskSearchResult> results = new List<PharmacyMaskSearchResult>();
            switch (pharmacyMaskSearchType)
            {
                case PharmacyMaskSearchType.All:
                    var pharmacies = await _pharmacyService.GetAll().Where(n => n.Name.Contains(keyword))
                        .Select(n => new { Id = n.PharmacyId, Name = n.Name, Type = PharmacyMaskSearchType.Pharmacy.ToString() })
                        .ToListAsync();
                    var masks = await _masksService.GetAll().Where(n => n.Name.Contains(keyword))
                        .Select(n => new { Id = n.MasksId, Name = n.Name, Type = PharmacyMaskSearchType.Mask.ToString() })
                        .ToListAsync();
                    results = pharmacies.Concat(masks)
                        .Select(item => new PharmacyMaskSearchResult
                        {
                            Type = item.Type,
                            Id = item.Id,
                            Name = item.Name,
                            Relevance = CalculateRelevance(keyword, item.Name)
                        })
                        .Where(x => x.Relevance > 0)
                        .OrderByDescending(x => x.Relevance)
                        .ToList();
                    break;
                case PharmacyMaskSearchType.Pharmacy:
                    var pharmaciesList = await _pharmacyService.GetAll()
                        .Where(p => p.Name.Contains(keyword))
                        .Select(p => new { p.PharmacyId, p.Name })
                        .ToListAsync();
                    results = pharmaciesList
                        .Select(p => new PharmacyMaskSearchResult
                        {
                            Id = p.PharmacyId,
                            Name = p.Name,
                            Type = PharmacyMaskSearchType.Pharmacy.ToString(),
                            Relevance = CalculateRelevance(keyword, p.Name)
                        })
                        .Where(x => x.Relevance > 0)
                        .OrderByDescending(x => x.Relevance)
                        .ToList();
                    break;
                case PharmacyMaskSearchType.Mask:
                    var masksList = await _masksService.GetAll().Where(n => n.Name.Contains(keyword))
                        .Select(n => new { n.MasksId, n.Name })
                        .ToListAsync();
                    results = masksList
                        .Select(p => new PharmacyMaskSearchResult
                        {
                            Id = p.MasksId,
                            Name = p.Name,
                            Type = PharmacyMaskSearchType.Mask.ToString(),
                            Relevance = CalculateRelevance(keyword, p.Name)
                        })
                        .Where(x => x.Relevance > 0)
                        .OrderByDescending(x => x.Relevance)
                        .ToList();
                    break;
                    
            }
            result.Data = results;
            return result;
        }

        private double CalculateRelevance(string keyword, string target)
        {
            if (string.IsNullOrWhiteSpace(keyword) || string.IsNullOrWhiteSpace(target))
                return 0.0;

            keyword = keyword.Trim().ToLower();
            target = target.Trim().ToLower();

            double score = 0;

            // 1️ 完全匹配
            if (target == keyword)
                return 1.0;

            // 2️ 將搜尋字詞拆成多個關鍵字
            var keywords = keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var kw in keywords)
            {
                if (target.StartsWith(kw))
                    score += 0.3;       // 開頭匹配加分
                else if (target.Contains(kw))
                    score += 0.2;       // 包含加分
            }

            // 3️ 字串距離 (Levenshtein) 適度容錯，距離越小分數越高
            int distance = Levenshtein(keyword, target);
            int maxLen = Math.Max(keyword.Length, target.Length);
            if (maxLen > 0)
            {
                double similarity = 1.0 - ((double)distance / maxLen); // 0~1
                similarity = Math.Max(similarity, 0);
                score += similarity * 0.5; // 權重 0.5
            }

            // 4️ 限制最大分數為 1
            return Math.Min(score, 1.0);
        }

        /// <summary>
        /// 距離函數
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public int Levenshtein(string s, string t)
        {
            if (string.IsNullOrEmpty(s)) return t.Length;
            if (string.IsNullOrEmpty(t)) return s.Length;

            var d = new int[s.Length + 1, t.Length + 1];

            for (int i = 0; i <= s.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= t.Length; j++) d[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost
                    );
                }

            return d[s.Length, t.Length];
        }
    }
}
