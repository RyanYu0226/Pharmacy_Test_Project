using Microsoft.EntityFrameworkCore;
using T4U_Pharmacy_Common_Model;
using T4U_Pharmacy_Common_Model.Common;
using T4U_Pharmacy_Common_Model.ViewModel;
using T4U_Pharmacy_Repository.DBModels;
using static T4U_Pharmacy_Common_Model.Common.CommonStruct;

namespace T4U_Pharmacy_Service
{
    public class PharmacyMaskHandleService
    {
        private PharmacyService _pharmacyService;
        private MasksService _masksService;
        private PharmacyMasksService _pharmacyMasksService;
        private PharmacyMasksStockLogService _pharmacyMasksStockLogService;
        private SystemConfigService _systemConfigService;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="pharmacyService"></param>
        /// <param name="masksService"></param>
        public PharmacyMaskHandleService(PharmacyService pharmacyService, MasksService masksService, PharmacyMasksService pharmacyMasksService, PharmacyMasksStockLogService pharmacyMasksStockLogService, SystemConfigService systemConfigService)
        {
            _masksService = masksService;
            _pharmacyService = pharmacyService;
            _pharmacyMasksService = pharmacyMasksService;
            _pharmacyMasksStockLogService = pharmacyMasksStockLogService;
            _systemConfigService = systemConfigService;
        }

        /// <summary>
        /// 取得指定藥局下的口罩資料
        /// </summary>
        /// <param name="pharmacyId">藥局ID</param>
        /// <param name="sortBy">排序欄位，只接受name、price</param>
        /// <param name="sortOrder">排序方式，只接受asc、desc</param>
        /// <returns></returns>
        public async Task<PharmacyMasksViewModel> GetPharmacyMasksList(long pharmacyId, PharmacyMasksSortBy sortBy = PharmacyMasksSortBy.Name, SortOrderEnum sortOrder = SortOrderEnum.Asc)
        {
            PharmacyMasksViewModel obj = new PharmacyMasksViewModel();
            obj.Result = true;
            try
            {
                var pharmacies = _pharmacyMasksService.GetAll().Where(n => n.PharmacyId == pharmacyId);
                var list = await pharmacies.Select(n => new { n.Masks, n.Pharmacy, n.PharmacyMasksStockLogs, n.Price }).ToListAsync();
                var listDetail = new List<PharmacyMasksDetail>();
                if (list.Count > 0)
                {
                    obj.PharmacyId = list.FirstOrDefault().Pharmacy.PharmacyId;
                    obj.Name = list.FirstOrDefault().Pharmacy.Name;
                    obj.CashBalance = GetPharmacyCurrentCashBalance(pharmacyId);
                    listDetail = list.Select(
                        n => new PharmacyMasksDetail
                        {
                            MasksId = n.Masks.MasksId,
                            Name = n.Masks.Name,
                            Price = n.Price,
                            StockQuantity = n.PharmacyMasksStockLogs.Sum(s => s.StockQuantity)
                        }
                        ).ToList();
                    switch (sortBy)
                    {
                        case PharmacyMasksSortBy.Price:
                            listDetail = sortOrder == SortOrderEnum.Desc
                                ? listDetail.OrderByDescending(n => n.Price).ToList()
                                : listDetail.OrderBy(n => n.Price).ToList();
                            break;

                        case PharmacyMasksSortBy.Name:
                            listDetail = sortOrder == SortOrderEnum.Desc
                                ? listDetail.OrderByDescending(n => n.Name).ToList()
                                : listDetail.OrderBy(n => n.Name).ToList();
                            break;
                    }
                }
                obj.Data = listDetail;
            }
            catch (Exception ex)
            {
                string message = "發生意外錯誤";
                obj.Result = false;
                obj.Message = message;
            }
            return obj;
        }

        /// <summary>
        /// 透過關鍵字搜尋藥局和口罩
        /// </summary>
        /// <param name="keyword">關鍵字</param>
        /// <param name="pharmacyMaskSearchType">類型</param>
        /// <param name="page">類型</param>
        /// <param name="pageSize">類型</param>
        /// <returns></returns>
        public async Task<PharmacyMaskSearchResultViewModel> SearchPharmacyMaskByKeyword(string keyword, PharmacyMaskSearchType pharmacyMaskSearchType, int page = 1, int pageSize = 5)
        {
            PharmacyMaskSearchResultViewModel result = new PharmacyMaskSearchResultViewModel();
            result.Result = true;
            List<PharmacyMaskSearchResult> results = new List<PharmacyMaskSearchResult>();
            var totalCount = 0;
            switch (pharmacyMaskSearchType)
            {
                case PharmacyMaskSearchType.All:
                    var pharmacies = await _pharmacyService.GetAll().Where(n => n.Name.Contains(keyword))
                        .Select(n => new { Id = n.PharmacyId, Name = n.Name, Type = PharmacyMaskSearchType.Pharmacy.ToString() })
                        .ToListAsync();
                    var masks = await _masksService.GetAll().Where(n => n.Name.Contains(keyword))
                        .Select(n => new { Id = n.MasksId, Name = n.Name, Type = PharmacyMaskSearchType.Mask.ToString() })
                        .ToListAsync();
                    totalCount = pharmacies.Count + masks.Count; 
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
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
                    break;
                case PharmacyMaskSearchType.Pharmacy:
                    var pharmaciesList = await _pharmacyService.GetAll()
                        .Where(p => p.Name.Contains(keyword))
                        .Select(p => new { p.PharmacyId, p.Name })
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
                    totalCount = pharmaciesList.Count;
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
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
                    break;
                case PharmacyMaskSearchType.Mask:
                    var masksList = await _masksService.GetAll().Where(n => n.Name.Contains(keyword))
                        .Select(n => new { n.MasksId, n.Name })
                        .ToListAsync();
                    totalCount = masksList.Count;
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
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
                    break;
                    
            }
            result.Data = results;
            result.pageInfo = new ListResultViewModel().GetPageInfo(page, pageSize, totalCount);
            return result;
        }

        /// <summary>
        /// 更新藥局多個口罩價格與庫存
        /// </summary>
        /// <param name="pharmacyId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<PharmacyMasksViewModel> UpdateMaskData(long pharmacyId, UpdateMultiPharmacyMaskInput input)
        {
            PharmacyMasksViewModel result = new PharmacyMasksViewModel();
            result.Message = ValidUpdateMaskData(pharmacyId, input);
            result.Result = true;
            if (string.IsNullOrEmpty(result.Message))
            {
                var pharmacy = _pharmacyService.Get(n => n.PharmacyId == pharmacyId);
                var maskNameList = input.Masks.Select(n => n.MaskName).Distinct().ToList();
                var masks = _masksService.GetAll().Where(n => maskNameList.Contains(n.Name))
                    .Select(n => new { n.MasksId, n.Name })
                    .ToList();
                var pharmacyMasks = _pharmacyMasksService.GetAll().Where(n => n.PharmacyId == pharmacyId)
                    .Select(n => new { n.PharmacyMasksId ,n.Masks.Name, Stock = n.PharmacyMasksStockLogs.Sum(s => s.StockQuantity) })
                    .ToList();
                foreach (var updateMask in input.Masks)
                {
                    var matchPharmacyMask = pharmacyMasks.FirstOrDefault(n => n.Name == updateMask.MaskName);
                    long targetStock = 0;
                    long targetPharmacyMaskId = 0;
                    if (matchPharmacyMask != null)
                    {
                        targetStock = updateMask.Stock - matchPharmacyMask.Stock;
                        targetPharmacyMaskId = matchPharmacyMask.PharmacyMasksId;
                    }
                    else
                    {
                        targetStock = updateMask.Stock;
                        var matchMask = masks.FirstOrDefault(n => n.Name == updateMask.MaskName);
                        long targetMaskId = 0;
                        if(matchMask != null)
                        {
                            targetMaskId = matchMask.MasksId;
                        }
                        else
                        {
                            var mask = new Mask()
                            {
                                Name = updateMask.MaskName
                            };
                            var addMaskResult = _masksService.Add(mask);
                            if(addMaskResult > 0)
                            {
                                targetMaskId = mask.MasksId;
                            }
                        }
                        if(targetMaskId != 0)
                        {
                            var pharmacyMask = new PharmacyMask()
                            {
                                PharmacyId = pharmacyId,
                                MasksId = targetMaskId,
                                Price = updateMask.Price,
                                CreatedDate = DateTime.Now,
                                ModifiedDate = DateTime.Now
                            };
                            var addPharmacyMaskResult = _pharmacyMasksService.Add(pharmacyMask);
                            if(addPharmacyMaskResult > 0)
                            {
                                targetPharmacyMaskId = pharmacyMask.PharmacyMasksId;
                            }
                        }
                    }
                    if(targetPharmacyMaskId != 0)
                    {
                        var targetPharmacyMask = _pharmacyMasksService.Get(n => n.PharmacyMasksId == targetPharmacyMaskId);
                        targetPharmacyMask.Price = updateMask.Price;
                        targetPharmacyMask.ModifiedDate = DateTime.Now;
                        if (_pharmacyMasksService.Update(targetPharmacyMask) > 0)
                        {
                            var stock = new PharmacyMasksStockLog()
                            {
                                PharmacyMasksId = targetPharmacyMaskId,
                                StockQuantity = targetStock,
                                Price = updateMask.Price,
                                CreatedDate = DateTime.Now,
                            };
                            _pharmacyMasksStockLogService.Add(stock);
                        }
                    }
                }
                result = await GetPharmacyMasksList(pharmacyId);
            }
            else
            {
                result.Result = false;
            }
            return result;
        }

        /// <summary>
        /// 更新藥局多個口罩價格與庫存檢驗
        /// </summary>
        /// <param name="pharmacyId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        private string ValidUpdateMaskData(long pharmacyId, UpdateMultiPharmacyMaskInput input)
        {
            var pharmacy = _pharmacyService.Get(n => n.PharmacyId == pharmacyId);
            if (pharmacy == null)
            {
                return string.Format("藥局不存在，藥局ID：{0}", pharmacyId);
            }
            //Mask至少要有一筆
            if(input.Masks == null || input.Masks.Count == 0)
            {
                return string.Format("口罩至少要有一筆資料");
            }
            //檢查調整價錢庫存後，剩餘金額是否不足
            var maskNameList = input.Masks.Select(n => n.MaskName).Distinct().ToList();
            var pharmacyMasks = _pharmacyMasksService.GetAll().Where(n => n.PharmacyId == pharmacyId)
                .Select(n => new { n.Masks.Name, Stock = n.PharmacyMasksStockLogs.Sum(s => s.StockQuantity) })
                .ToList();
            var currentCashBalance = GetPharmacyCurrentCashBalance(pharmacyId);
            decimal totalCash = 0;
            foreach(var updateMask in input.Masks)
            {
                var matchMask = pharmacyMasks.FirstOrDefault(n => n.Name == updateMask.MaskName);
                if (matchMask != null)
                {
                    var targetStock = updateMask.Stock - matchMask.Stock;
                    totalCash += targetStock * -updateMask.Price;
                }
                else
                {
                    totalCash += updateMask.Stock * -updateMask.Price;
                }
            }
            if (currentCashBalance + totalCash < 0)
            {
                return string.Format("藥局剩餘金額，無法負荷此庫存量，目前剩餘金額：{0}，此次需花費金額：{1}", currentCashBalance, -totalCash);
            }
            //檢驗庫存和金額不應該為負數
            foreach (var updateMask in input.Masks)
            {
                if(updateMask.Price < 0)
                {
                    return string.Format("金額不應該為負數，MaskName:{0}", updateMask.MaskName);
                }
                if(updateMask.Stock < 0)
                {
                    return string.Format("庫存不應該為負數，MaskName:{0}", updateMask.MaskName);
                }
            }
            //檢驗同一個口罩名稱只能出現一次
            foreach(var updateMask in input.Masks)
            {
                if(input.Masks.Where(n => n.MaskName == updateMask.MaskName).Count() > 1)
                {
                    return string.Format("口罩不能重複，MaskName:{0}", updateMask.MaskName);
                }
            }
            return "";
        }

        /// <summary>
        /// 取得藥局目前剩餘金額
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public decimal GetPharmacyCurrentCashBalance(long pharmacyId)
        {
            decimal currentCash = 0;
            var pharmacy = this._pharmacyService.Get(n => n.PharmacyId == pharmacyId);
            string strUserCashEndDate = _systemConfigService.GetValue(SystemConfigService.PHARMACY_STOCK_SETTLEMENT_TIME);
            if (!string.IsNullOrEmpty(strUserCashEndDate))
            {
                var dtUserCashEndDate = DateTime.Parse(strUserCashEndDate);
                var stockList = _pharmacyMasksStockLogService.GetAll().Where(n => n.PharmacyMasks.PharmacyId == pharmacyId && n.CreatedDate > dtUserCashEndDate).ToList();
                var sumTotal = stockList.Sum(n => -n.StockQuantity * n.Price);
                currentCash = pharmacy.CurrentCashBalance + sumTotal;
            }
            else
            {
                currentCash = pharmacy.CurrentCashBalance;
            }
            return currentCash;
        }

        /// <summary>
        /// 計算關聯性
        /// </summary>
        /// <param name="keyword">關鍵字</param>
        /// <param name="target">目標文字</param>
        /// <returns></returns>
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
        /// <param name="s">關鍵字</param>
        /// <param name="t">目標文字</param>
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
