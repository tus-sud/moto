using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoShopWeb.Data;
using MotoShopWeb.Services;
using MotoShopWeb.ViewModels;
namespace MotoShopWeb.Controllers;
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public class CartController(CartService cart,PurchaseService purchases,ApplicationDbContext db):Controller {
 [HttpGet] public async Task<IActionResult> Index()=>View(await cart.Lines());
 [HttpPost] public async Task<IActionResult> Add(int colorId){try{await cart.Add(colorId);TempData["Success"]="Đã thêm xe vào giỏ (tối đa 3 xe mỗi màu).";}catch(InvalidOperationException e){TempData["Error"]=e.Message;}return RedirectToAction(nameof(Index));}
 [HttpPost] public IActionResult Remove(int colorId){cart.Remove(colorId);return RedirectToAction(nameof(Index));}
 [Authorize(AuthenticationSchemes="Customer"),HttpGet]
 public async Task<IActionResult> Checkout(){
  await SchemaInitializer.EnsureCompatibleSchemaAsync(db);
  var id=int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);var customer=await db.KhachHangs.SingleAsync(x=>x.MaKh==id);
  var lines=await cart.Lines();if(lines.Count==0)return RedirectToAction(nameof(Index));
  return View(new CheckoutForm{Name=customer.HoTen,Phone=customer.SoDienThoai??"",Address=customer.DiaChi??"",Lines=lines,Token=purchases.Token(id,lines),SelectedWarrantyIds=[1],WarrantyOptions=WarrantyOptionVm.Defaults()});
 }
 [Authorize(AuthenticationSchemes="Customer"),HttpPost]
 public async Task<IActionResult> Checkout(CheckoutForm form){
  await SchemaInitializer.EnsureCompatibleSchemaAsync(db);
  // Never trust client-posted line prices, totals, customer IDs or order status.
  form.Lines=await cart.Lines();
  form.WarrantyOptions=WarrantyOptionVm.Defaults();
  if(form.SelectedWarrantyIds is null || form.SelectedWarrantyIds.Count==0)form.SelectedWarrantyIds=[1];
  if(!ModelState.IsValid)return View(form);
  try{
   var id=await purchases.Place(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),form);
   TempData["Success"]="Đặt xe thành công! Dưới đây là thông tin Thanh toán & Hóa đơn của bạn.";
   return RedirectToAction("Invoice","Orders",new{id});
  }
  catch(InvalidOperationException e){ModelState.AddModelError("",e.Message);return View(form);}
 }
}
