using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotoShopWeb.Data;
using MotoShopWeb.Models;
namespace MotoShopWeb.Controllers;
[Authorize(Policy="AdminOnly")]
public class OrdersAdminController(ApplicationDbContext db):Controller {
 public async Task<IActionResult> Index(int page=1){page=Math.Clamp(page,1,100000);ViewBag.Page=page;return View(await db.DonHangs.AsNoTracking().OrderByDescending(x=>x.MaDonHang).Skip((page-1)*20).Take(20).ToListAsync());}
 public async Task<IActionResult> Details(int id){var order=await db.DonHangs.AsNoTracking().Include(x=>x.ChiTietDonHangs).ThenInclude(x=>x.MauXe).ThenInclude(x=>x.MaXeNavigation).Include(x=>x.LichSuDonHangs).AsSplitQuery().SingleOrDefaultAsync(x=>x.MaDonHang==id);return order is null?NotFound():View(order);}
 [HttpPost]
 public async Task<IActionResult> Update(int id,string next){
  if(next is not ("DaXacNhan" or "DaHuy"))return BadRequest();
  var actor=int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
  await using var tx=await db.Database.BeginTransactionAsync();
  // Atomic transition: a stale second admin submit cannot overwrite another decision.
  var changed=await db.DonHangs.Where(x=>x.MaDonHang==id && x.TrangThai=="ChoXacNhan")
   .ExecuteUpdateAsync(s=>s.SetProperty(x=>x.TrangThai,next).SetProperty(x=>x.MaNv,actor));
  if(changed!=1){TempData["Error"]="Đơn không còn chờ xác nhận. Tải lại để xem trạng thái mới.";return RedirectToAction(nameof(Details),new{id});}
  db.LichSuDonHangs.Add(new LichSuDonHang{MaDonHang=id,MaNv=actor,TrangThai=next,ThoiGian=DateTime.Now,GhiChu=next=="DaHuy"?"Cửa hàng hủy yêu cầu mua xe.":"Cửa hàng đã tiếp nhận và xác nhận yêu cầu. Chưa ghi nhận thanh toán."});
  db.NhatKyHeThongs.Add(new NhatKyHeThong{MaNv=actor,HanhDong="XuLyDonHang",BangTacDong="DonHang",MaBanGhi=id,ChiTiet="ChoXacNhan → "+next,ThoiGian=DateTime.Now});
  await db.SaveChangesAsync();await tx.CommitAsync();TempData["Success"]="Đã cập nhật trạng thái yêu cầu.";return RedirectToAction(nameof(Details),new{id});
 }
}
