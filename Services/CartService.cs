using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MotoShopWeb.Data;
using MotoShopWeb.ViewModels;
namespace MotoShopWeb.Services;
public class CartService(IHttpContextAccessor accessor,ApplicationDbContext db) {
 private ISession Session=>accessor.HttpContext!.Session;
 private Dictionary<int,int> Read()=>JsonSerializer.Deserialize<Dictionary<int,int>>(Session.GetString("cart")??"{}")??[];
 public async Task Add(int colorId) {
  if(!await db.MauXes.AnyAsync(x=>x.MaMau==colorId && x.MaXeNavigation.HienThi))throw new InvalidOperationException("Mẫu xe hoặc màu này chưa thể đặt.");
  var cart=Read();
  if(!cart.ContainsKey(colorId)&&cart.Count>=10)throw new InvalidOperationException("Giỏ hàng tối đa 10 lựa chọn.");
  cart[colorId]=Math.Min(3,cart.GetValueOrDefault(colorId)+1); Save(cart);
 }
 private void Save(Dictionary<int,int> cart)=>Session.SetString("cart",JsonSerializer.Serialize(cart));
 public void Remove(int colorId){var cart=Read();cart.Remove(colorId);Save(cart);}
 public void Clear()=>Session.Remove("cart");
 public async Task<List<CartLine>> Lines(){
  var cart=Read();var ids=cart.Keys.ToList();
  var colors=await db.MauXes.AsNoTracking().Include(x=>x.MaXeNavigation).Where(x=>ids.Contains(x.MaMau)).ToListAsync();
  foreach(var missing in ids.Except(colors.Select(x=>x.MaMau)))cart.Remove(missing);Save(cart);
  return colors.OrderBy(x=>x.MaMau).Select(x=>new CartLine{Available=x.MaXeNavigation.HienThi,ColorId=x.MaMau,BikeId=x.MaXe,Color=x.TenMau,Name=x.MaXeNavigation.TenXe,Price=x.MaXeNavigation.GiaNiemYet,Quantity=cart[x.MaMau]}).ToList();
 }
 public async Task<bool> IsComplete()=> (await Lines()).All(x=>x.Available);
}
