using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoShopWeb.Data;
using MotoShopWeb.Models;
using MotoShopWeb.ViewModels;
namespace MotoShopWeb.Controllers;
[Authorize(AuthenticationSchemes="Customer"),ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public class OrdersController(ApplicationDbContext db):Controller {
 private int Customer=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 public async Task<IActionResult> Index(int page=1){page=Math.Clamp(page,1,100000);ViewBag.Page=page;return View(await db.DonHangs.AsNoTracking().Where(x=>x.MaKh==Customer).OrderByDescending(x=>x.MaDonHang).Skip((page-1)*20).Take(20).ToListAsync());}
 public async Task<IActionResult> Details(int id){var order=await db.DonHangs.AsNoTracking().Include(x=>x.ChiTietDonHangs).ThenInclude(x=>x.MauXe).ThenInclude(x=>x.MaXeNavigation).Include(x=>x.LichSuDonHangs).AsSplitQuery().SingleOrDefaultAsync(x=>x.MaDonHang==id&&x.MaKh==Customer);return order is null?NotFound():View(order);}
 [HttpGet]
 public async Task<IActionResult> Invoice(int id){
  await SchemaInitializer.EnsureCompatibleSchemaAsync(db);
  var order=await db.DonHangs.AsNoTracking()
   .Include(x=>x.ChiTietDonHangs).ThenInclude(x=>x.MauXe).ThenInclude(x=>x.MaXeNavigation)
   .Include(x=>x.LichSuDonHangs)
   .Include(x=>x.ThanhToans)
   .AsSplitQuery()
   .SingleOrDefaultAsync(x=>x.MaDonHang==id&&x.MaKh==Customer);
  if(order is null)return NotFound();

  var allWarranties=WarrantyOptionVm.Defaults();
  var selectedIds=new List<int>();
  var plan="DatCoc";
  var method="Chuyển khoản ngân hàng (VietQR)";
  string? customerNote=null;

  if(!string.IsNullOrWhiteSpace(order.GhiChu)){
   foreach(var part in order.GhiChu.Split('|',StringSplitOptions.RemoveEmptyEntries)){
    if(part.StartsWith("BH:")){
     foreach(var s in part[3..].Split(',',StringSplitOptions.RemoveEmptyEntries))
      if(int.TryParse(s,out var wid))selectedIds.Add(wid);
    } else if(part.StartsWith("PT:")){
     plan=part[3..];
    } else if(part.StartsWith("PM:")){
     method=part[3..];
    } else if(part.StartsWith("NOTE:")){
     customerNote=part[5..];
    }
   }
  }
  var selectedWarranties=allWarranties.Where(w=>selectedIds.Contains(w.Id)).ToList();
  if(selectedWarranties.Count==0)selectedWarranties=[allWarranties[0]];

  var bikesSubtotal=order.ChiTietDonHangs.Sum(c=>c.DonGia*c.SoLuong);
  var warrantyTotal=selectedWarranties.Sum(w=>w.Price);
  var payment=order.ThanhToans.OrderByDescending(t=>t.MaThanhToan).FirstOrDefault();

  return View(new OrderInvoiceVm{
   Order=order,
   Payment=payment,
   SelectedWarranties=selectedWarranties,
   BikesSubtotal=bikesSubtotal,
   WarrantyTotal=warrantyTotal,
   PaymentPlan=plan,
   PaymentMethod=payment?.PhuongThuc??method,
   CustomerNote=customerNote
  });
 }
 [HttpPost]
 public async Task<IActionResult> ConfirmPayment(int id){
  await SchemaInitializer.EnsureCompatibleSchemaAsync(db);
  var order=await db.DonHangs.Include(x=>x.ThanhToans).SingleOrDefaultAsync(x=>x.MaDonHang==id&&x.MaKh==Customer);
  if(order is null)return NotFound();
  var payment=order.ThanhToans.OrderByDescending(t=>t.MaThanhToan).FirstOrDefault();
  if(payment is not null && payment.TrangThai!="DaXacNhanTT"){
   payment.TrangThai="DaXacNhanTT";
   payment.NgayThanhToan=DateTime.Now;
  }
  db.LichSuDonHangs.Add(new LichSuDonHang{
   MaDonHang=id,
   TrangThai=order.TrangThai,
   ThoiGian=DateTime.Now,
   GhiChu=$"Khách hàng đã xác nhận hoàn tất thanh toán ({payment?.PhuongThuc ?? "Chuyển khoản"}) cho Hóa đơn #{id}."
  });
  await db.SaveChangesAsync();
  TempData["Success"]="Đã ghi nhận xác nhận thanh toán! Bộ phận kế toán & bán hàng sẽ liên hệ bàn giao xe và kích hoạt sổ bảo hành.";
  return RedirectToAction(nameof(Invoice),new{id});
 }
 [HttpPost]
 public async Task<IActionResult> Cancel(int id){
  var changed=await db.DonHangs.Where(x=>x.MaDonHang==id&&x.MaKh==Customer&&x.TrangThai=="ChoXacNhan")
   .ExecuteUpdateAsync(s=>s.SetProperty(x=>x.TrangThai,"DaHuy"));
  if(changed==1){
   db.LichSuDonHangs.Add(new LichSuDonHang{MaDonHang=id,TrangThai="DaHuy",ThoiGian=DateTime.Now,GhiChu="Khách hàng chủ động hủy yêu cầu mua xe."});
   await db.SaveChangesAsync();
   TempData["Success"]="Đã hủy yêu cầu mua xe #"+id+".";
  } else {
   TempData["Error"]="Yêu cầu này đã được xử lý hoặc không còn ở trạng thái chờ xác nhận.";
  }
  return RedirectToAction(nameof(Details),new{id});
 }
}
