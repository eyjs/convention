using LocalRAG.Entities;
using LocalRAG.Interfaces;
using LocalRAG.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LocalRAG.Services.Convention;

public class AttributeCategoryService : IAttributeCategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public AttributeCategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 행사의 속성 카테고리 목록을 Items 포함하여 반환합니다.
    /// </summary>
    public async Task<List<object>> GetCategoriesAsync(int conventionId)
    {
        var categories = await _unitOfWork.AttributeCategories.Query
            .Where(c => c.ConventionId == conventionId)
            .Include(c => c.Items)
            .OrderBy(c => c.OrderNum)
            .ThenBy(c => c.Id)
            .ToListAsync();

        return categories.Select(c => (object)new
        {
            c.Id,
            c.ConventionId,
            c.Name,
            c.Icon,
            c.OrderNum,
            c.CreatedAt,
            Items = c.Items
                .OrderBy(i => i.OrderNum)
                .ThenBy(i => i.Id)
                .Select(i => new
                {
                    i.Id,
                    i.AttributeCategoryId,
                    i.AttributeKey,
                    i.OrderNum
                })
                .ToList()
        }).ToList();
    }

    /// <summary>
    /// 속성 카테고리를 생성합니다. OrderNum은 마지막+1.
    /// </summary>
    public async Task<object> CreateCategoryAsync(int conventionId, string name, string? icon, List<string> attributeKeys, int? orderNum = null)
    {
        var maxOrder = await _unitOfWork.AttributeCategories.Query
            .Where(c => c.ConventionId == conventionId)
            .MaxAsync(c => (int?)c.OrderNum) ?? 0;

        var category = new AttributeCategory
        {
            ConventionId = conventionId,
            Name = name,
            Icon = icon,
            OrderNum = orderNum ?? maxOrder + 1,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.AttributeCategories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        // 속성 키 연결
        var newItems = attributeKeys
            .Select((key, index) => new AttributeCategoryItem
            {
                AttributeCategoryId = category.Id,
                AttributeKey = key,
                OrderNum = index + 1,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        foreach (var item in newItems)
        {
            await _unitOfWork.AttributeCategoryItems.AddAsync(item);
        }

        if (newItems.Count > 0)
            await _unitOfWork.SaveChangesAsync();

        return new
        {
            category.Id,
            category.ConventionId,
            category.Name,
            category.Icon,
            category.OrderNum,
            category.CreatedAt,
            Items = newItems.Select(i => new
            {
                i.Id,
                i.AttributeCategoryId,
                i.AttributeKey,
                i.OrderNum
            }).ToList()
        };
    }

    /// <summary>
    /// 카테고리 이름/아이콘을 수정하고 Items를 교체합니다 (기존 삭제 후 재생성).
    /// </summary>
    public async Task<object> UpdateCategoryAsync(int categoryId, string name, string? icon, List<string> attributeKeys, int? orderNum = null)
    {
        var category = await _unitOfWork.AttributeCategories.Query
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category == null)
            throw new KeyNotFoundException($"카테고리 {categoryId}를 찾을 수 없습니다.");

        category.Name = name;
        category.Icon = icon;
        if (orderNum.HasValue)
            category.OrderNum = orderNum.Value;

        // 기존 Items 삭제
        if (category.Items.Count > 0)
        {
            _unitOfWork.AttributeCategoryItems.RemoveRange(category.Items);
        }

        // 새 Items 생성
        var newItems = attributeKeys
            .Select((key, index) => new AttributeCategoryItem
            {
                AttributeCategoryId = categoryId,
                AttributeKey = key,
                OrderNum = index + 1,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        foreach (var item in newItems)
        {
            await _unitOfWork.AttributeCategoryItems.AddAsync(item);
        }

        _unitOfWork.AttributeCategories.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return new
        {
            category.Id,
            category.ConventionId,
            category.Name,
            category.Icon,
            category.OrderNum,
            category.CreatedAt,
            Items = newItems.Select(i => new
            {
                i.Id,
                i.AttributeCategoryId,
                i.AttributeKey,
                i.OrderNum
            }).ToList()
        };
    }

    /// <summary>
    /// 카테고리와 하위 Items를 삭제합니다.
    /// </summary>
    public async Task<bool> DeleteCategoryAsync(int categoryId)
    {
        var category = await _unitOfWork.AttributeCategories.Query
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category == null)
            return false;

        if (category.Items.Count > 0)
        {
            _unitOfWork.AttributeCategoryItems.RemoveRange(category.Items);
        }

        _unitOfWork.AttributeCategories.Remove(category);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// 해당 행사 참석자의 GuestAttribute에서 DISTINCT AttributeKey를 반환합니다.
    /// </summary>
    public async Task<List<string>> GetAttributeKeysAsync(int conventionId)
    {
        var keys = await _unitOfWork.UserConventions.Query
            .Where(uc => uc.ConventionId == conventionId)
            .SelectMany(uc => uc.User.GuestAttributes)
            .Select(ga => ga.AttributeKey)
            .Distinct()
            .OrderBy(k => k)
            .ToListAsync();

        return keys;
    }

    /// <summary>
    /// userId의 속성을 카테고리별로 그룹화합니다.
    /// 카테고리에 매핑된 속성은 해당 카테고리 아래, 미매핑 속성은 "기타" 카테고리에 포함됩니다.
    /// </summary>
    public async Task<object> GetGroupedAttributesAsync(int conventionId, int userId)
    {
        // 카테고리 + Items 로드
        var categories = await _unitOfWork.AttributeCategories.Query
            .Where(c => c.ConventionId == conventionId)
            .Include(c => c.Items)
            .OrderBy(c => c.OrderNum)
            .ThenBy(c => c.Id)
            .ToListAsync();

        // 사용자 속성 로드
        var userAttributes = await _unitOfWork.GuestAttributes.Query
            .Where(ga => ga.UserId == userId)
            .ToListAsync();

        var mappedKeys = categories
            .SelectMany(c => c.Items.Select(i => i.AttributeKey))
            .ToHashSet();

        var grouped = categories.Select(c => new
        {
            c.Id,
            c.Name,
            c.Icon,
            c.OrderNum,
            Attributes = c.Items
                .OrderBy(i => i.OrderNum)
                .Select(i => new
                {
                    Key = i.AttributeKey,
                    Value = userAttributes.FirstOrDefault(a => a.AttributeKey == i.AttributeKey)?.AttributeValue
                })
                .Where(a => a.Value != null)
                .ToList<object>()
        }).ToList<object>();

        // 미매핑 속성은 "기타" 카테고리로
        var etcAttributes = userAttributes
            .Where(a => !mappedKeys.Contains(a.AttributeKey))
            .OrderBy(a => a.AttributeKey)
            .Select(a => (object)new
            {
                Key = a.AttributeKey,
                Value = a.AttributeValue
            })
            .ToList();

        if (etcAttributes.Count > 0)
        {
            grouped.Add(new
            {
                Id = 0,
                Name = "기타",
                Icon = (string?)null,
                OrderNum = int.MaxValue,
                Attributes = etcAttributes
            });
        }

        return grouped;
    }

    /// <summary>
    /// categoryIds 순서대로 OrderNum을 업데이트합니다.
    /// </summary>
    public async Task ReorderCategoriesAsync(List<int> categoryIds)
    {
        for (int i = 0; i < categoryIds.Count; i++)
        {
            var category = await _unitOfWork.AttributeCategories.GetByIdAsync(categoryIds[i]);
            if (category != null)
            {
                category.OrderNum = i + 1;
                _unitOfWork.AttributeCategories.Update(category);
            }
        }
        await _unitOfWork.SaveChangesAsync();
    }
}
