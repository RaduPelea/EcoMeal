using Ecomeal.client.Models;

namespace Ecomeal.client.Services;

public enum CartAddResult
{
    Added,
    AlreadyInBasket,
    DifferentBusiness
}

public class CartService
{
    private readonly List<CartItemModel> _items = new();

    public IReadOnlyList<CartItemModel> Items => _items;
    public int Count => _items.Count;
    public decimal Total => _items.Sum(i => i.Price);
    public string? BusinessName => _items.Count > 0 ? _items[0].BusinessName : null;
    public int? BusinessId => _items.Count > 0 ? _items[0].BusinessId : null;

    public event Action? OnChanged;

    public CartAddResult Add(CartItemModel item)
    {
        if (_items.Any(i => i.PackageId == item.PackageId))
            return CartAddResult.AlreadyInBasket;

        if (_items.Count > 0 && _items[0].BusinessId != item.BusinessId)
            return CartAddResult.DifferentBusiness;

        _items.Add(item);
        OnChanged?.Invoke();
        return CartAddResult.Added;
    }

    public void Remove(int packageId)
    {
        _items.RemoveAll(i => i.PackageId == packageId);
        OnChanged?.Invoke();
    }

    public void Clear()
    {
        _items.Clear();
        OnChanged?.Invoke();
    }
}
